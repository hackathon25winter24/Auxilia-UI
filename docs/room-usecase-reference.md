# Room UseCase 利用リファレンス

2026-10-19時点の実装に基づく。計画ではなく、現在の公開メソッドを呼んだときの動作を説明する。

現在、`UseCase`というクラスは`RoomUseCase`の1種類。以下では、その各公開操作を個別のユースケースとして説明する。
関連する`LobbyConnector`・`RoomData`・`NetworkManager.ConfigureRoom`も全公開メンバーを記載する。
戦闘への引き継ぎは[BattleConnector利用補足](battle-connector-reference.md)、全体設計は[実装計画](room-authoritative-v2-plan.md)を参照。

## 1. 責務と呼び出し経路

```text
呼出元（入力・画面制御など）
  → RoomUseCase：操作・受信の寿命、保存順序、競合・再接続の管理
  → LobbyConnector：認証ヘッダーを付けたRPC通信
  → バックエンド：席・所有権・準備状態・開始可否の確定
  → RoomUseCase：応答／配信の部屋・世代・revisionを確認
  → RoomData：全Snapshotと互換フィールドを保存
  → Changed(type) → MatchAvailable(matchId, phase)
```

- UseCaseは描画、ボタンの見た目、効果音、Scene遷移を行わない。
- サーバーの結果を保存する。クライアントが先に席や準備状態を書き換える楽観更新はしない。
- 全公開操作はUnityメインスレッドから呼ぶ。受信後もメインスレッドへ戻してSOを更新する。
- `RoomUseCase`は通常のC#オブジェクト。GameObjectへAddComponentしない。
- 旧`MatchingConnector`と旧UIは残っているが、新UseCaseへ自動転送されない。同じRoomDataに対し旧UIの直接書き込みと新UseCaseを併用しない。
- コールバックは同期実行。例外を外へ投げたり長時間ブロックすると受信処理を妨げる。非同期処理は呼出側で例外処理とキャンセルを管理する。

## 2. 初期化

### `NetworkManager.ConfigureRoom(RoomData data) → RoomUseCase`

| 引数 | 意味 |
|---|---|
| `data` | 受信情報を保存する既存のRoomDataアセット。表示・通知購読側も同じインスタンスを参照する。null不可 |

前提はNetworkManagerの初期化とログイン完了。`Core.PlayerId`が空の場合は`InvalidOperationException`。
dataがnullなら`ArgumentNullException`。

呼ぶと次の処理を行う。

1. 以前の`NetworkManager.Room`があればDisposeする。これはサーバーからの退出ではない。
2. 通常RPC用Channelと配信用StreamChannelからLobbyConnectorを生成する。
3. 現在のPlayerId・NetworkManager破棄トークンを使ってRoomUseCaseを生成する。
4. SOのオンライン情報を初期化し、IsBusyをfalseにする。
5. 新しいUseCaseを`NetworkManager.Room`へ保存し、戻り値として返す。

この時点では通信も入室も行わない。毎回の描画やボタンクリックで呼ばず、接続層の初期化時に一度呼ぶ。
アカウントを切り替えたら再構成する。以前のUseCaseのイベント購読も呼出側で解除する。
`NetworkManager.Room`はConfigureRoom前にはnull。NetworkManager破棄時には自動Disposeされる。

### `new RoomUseCase(LobbyConnector connector, RoomData data, string playerId, CancellationToken ct = default)`

テストや独自の構成コードから直接生成する場合の入口。

| 引数 | 意味・制約 |
|---|---|
| `connector` | 通信を委譲するLobbyConnector。null不可 |
| `data` | 保存先SO。null不可 |
| `playerId` | ログイン済みユーザーID。空文字・null不可。SOの自分のインデックス計算に使用する。サーバーの認証はBearerトークンで別途行う |
| `ct` | UseCase全体の寿命。個別RPCとWatchに連結する。シーンを越えて使う場合は短命なシーンのトークンを渡さない |

生成時に`data.ResetOnline()`と`data.SetBusy(false)`を呼ぶ。引数不正はArgumentNullExceptionまたはArgumentException。
直接生成した場合は所有者がDisposeする。寿命トークンのキャンセルだけではSOの明示的なクリアまで保証されないため、終了処理にはDisposeを使う。

## 3. 共通契約

### 戻り値と結果の確認

Create、Join、Resume、Refresh、EnterRing、LeaveRing、SetReady、Rename、StartMatch、Leaveはすべて`UniTask`。
Snapshotを戻り値として返さない。`await`後は`room.Data.Snapshot`を参照する。

ただし、実行中にDisconnect／Disposeされ、応答の世代が古くなった場合、応答を破棄して正常終了することがある。
`await`の完了だけで「今も入室中」「状態が必ず適用された」と判断せず、SnapshotとConnectionを確認する。
また、ストリームが先に新しい状態を届けると、操作応答が保存されないことがある。これは巻き戻りを防ぐ正常な動作。

### `CancellationToken ct = default`

- 個別操作のRPCを中断するためのトークン。省略時は呼出元による期限なし。
- UseCaseの寿命トークンと連結してRPCへ渡す。
- Create／Join／Resume成功後のWatchは、この個別トークンを引き継がない。呼出元シーンの終了後も受信を継続する。
- キャンセル時はOperationCanceledExceptionまたはRPCのCancelledになり得る。
- キャンセルはサーバー処理の巻き戻しではない。作成・入室・退出などが既に成立している可能性がある。
- 結果不明なら再取得で確認する。特にCreateを無条件に再送すると部屋が増える可能性がある。

### 同時操作とrevision

- Create／Join／Resumeと6種類の変更操作はIsBusyをtrueにする。同時呼び出しは待ち行列にせずInvalidOperationExceptionで拒否する。
- 通信処理終了後はfinallyでIsBusyをfalseにする。
- RefreshはIsBusyを変更せず、他の処理と並行実行できる。返答順序はrevisionで整理する。
- 変更操作には保存済みの最新`Data.Revision`を`ExpectedRevision`として付ける。呼出側から数値を渡す必要はない。
- サーバー側で状態が先に変わった場合はAborted。UseCaseがRefreshを試みるが、元の操作を再送しない。
- ConnectionがReconnectingでも変更要求自体はローカルで禁止しない。操作受付の制御はConnection、IsBusy、SnapshotのCan系を合わせて判断する。

## 4. 各操作

### `Create(string name, CancellationToken ct = default)` — 部屋を作成して参加する

`name`は部屋名。バックエンドが前後空白を除去し、空でなくUTF-8で128バイト以内か検証する。文字数128ではない。nullは生成コード等の引数エラーになるため渡さない。

前提：このUseCaseが現在の部屋を保持していないこと。保持中なら同じ部屋であってもInvalidOperationException。

実行するとConnectingになり、`CreateLobby(room_name)`を送る。サーバーが部屋を作成し、自分を所有者・1Pとして登録する。
成功後にSOを初期化し、返されたSnapshotを保存してWatchRoomを開始する。初期状態はWaiting、準備未完了。
RoomSync、OperationChangedなどが発生し、最初の配信受信後にConnectedになる。

注意：このRPCには作成の冪等キーがない。通信切断時に自動再試行しない。部屋一覧で作成済みか確認する。

### `Join(uint id, CancellationToken ct = default)` — 指定部屋へ参加する

`id`はサーバーが発行した数値のroom_id。UI一覧の配列番号でも、文字列のmatch_idでもない。0は不可、バックエンドの有効範囲は1〜Int32.MaxValue。

`JoinLobby(room_id)`を送る。新規参加の場合、サーバーが空き1P→空き2P→観戦席の順で割り当てる。定員8名。
同じユーザーが既に参加している場合は席・準備状態を維持する。新規参加があるとサーバーは準備状態を解除する。
成功後の保存・通知・Watch開始はCreateと同じ。

開始済みの部屋への新規参加はFailedPrecondition。既存メンバーの再入室は可能。
満室はResourceExhausted、存在しない部屋はNotFound。UseCaseが既に接続先を保持する場合、先にLeaveまたはDisconnectが必要。

### `Resume(uint id, CancellationToken ct = default)` — 参加済み部屋の受信を再開する

`id`の意味はJoinと同じ。`GetLobby(room_id)`を送り、参加済みであることを確認する。
新規入室、席の再割り当て、準備状態変更は行わない。
成功後にSOを初期化・保存し、Watchを開始する。既にSelecting／BattlingならMatchAvailableも通知される。

メンバーでなければPermissionDenied。使いどころはDisconnect後の再開や再ログイン後の復帰。
ネットワークの一時切断ではWatchが自動再接続するので、通常はResumeを繰り返し呼ばない。

### `Refresh(CancellationToken ct = default)` — 現在の部屋の状態だけを再取得する

room_id引数は不要。UseCaseが保持する部屋へGetLobbyを送る。
新しいrevisionならSOを保存・通知する。同一または古いrevisionなら何もしない。
新たなWatchの開始・停止、入室・退出、IsBusy変更は行わない。

未接続またはDispose後はInvalidOperationException。RPC例外はそのまま呼出元へ返る。
**直接呼んだRefreshの失敗はFailedイベントへ流れない。** 呼出元でcatchする。
既にWatchが終端エラーで停止して内部room_idを失った場合、Refreshでは復帰できない。Resumeを使う。

### `EnterRing(CancellationToken ct = default)` — 観戦から対戦席へ移る

引数はキャンセルのみ。席番号やユーザーIDを渡さない。
`ApplyRoomCommand(EnterRing, expected_revision)`を送る。
Waiting中・自分が観戦者・対戦席に空きありの場合、サーバーが席を選ぶ。参加者の準備状態を解除する。
結果を保存後、通常MembersChanged、ReadyChanged、PermissionsChangedなどを通知する。実際に変わった項目のみ通知される。
既に対戦席にいる場合や満席の場合はFailedPrecondition。UI側ではCanEnterRingを参照できる。

### `LeaveRing(CancellationToken ct = default)` — 対戦席から観戦へ移る

`ApplyRoomCommand(LeaveRing, expected_revision)`を送る。
Waiting中の対戦者のみ実行できる。自分は観戦者となり、準備状態を解除する。
現在のバックエンドでは1Pが抜けると既存2Pを1Pへ繰り上げる。参加者全体の準備状態も解除する。
所有権は席とは独立しているので、所有者が観戦席へ移ってもOwnerIdは変わらない。
部屋からの退出ではなく、Watchは継続する。

### `SetReady(bool ready, CancellationToken ct = default)` — 準備状態を指定する

`ready=true`で準備完了、`false`で解除。サーバーに「反転」を指示するAPIではない。
`ApplyRoomCommand(SetReady, ready, expected_revision)`を送る。
Waiting中の対戦者のみ有効。観戦者は準備できない。所有者／1Pも自動的には準備完了にならない。
同じ値を指定し、サーバー状態が変化しなければrevisionとSnapshot通知は増えない場合がある。

ここでの準備完了は**ルームの開始条件**。戦闘側の`BattleConnector.Ready`（編成確定）とは別。

### `Rename(string name, CancellationToken ct = default)` — 部屋名を変更する

`name`の制約はCreateと同じ。`ApplyRoomCommand(Rename, room_name, expected_revision)`を送る。
Waiting中の所有者のみ実行可能。1Pかどうかでは判定しない。
結果を保存してNameChangedなどを通知する。呼出元が先にroom_nameを変更する必要はない。
非所有者はPermissionDenied、不正な部屋名はInvalidArgument。

### `StartMatch(CancellationToken ct = default)` — 編成選択段階へ進める

`ApplyRoomCommand(StartMatch, expected_revision)`を送る。match_idや対戦者IDは渡さない。
条件はWaiting、所有者からの要求、1P／2Pが存在、両対戦者が準備完了。
所有者が観戦者でも条件を満たせば開始できる。

成功するとサーバーがV2試合を作成し、phase=Selecting、match_idを返す。
SO保存後にPhaseChanged等、続いて`MatchAvailable(matchId, LobbyPhase.Selecting)`を通知する。
**この時点では戦闘は開始していない。** 編成登録やBattle.Ready、Scene変更はこのメソッドでは行わない。

開始直前に他参加者の状態が変わるとAborted／FailedPreconditionになり得る。CanStartは表示の判断材料であり、成功の保証ではない。

### `Leave(CancellationToken ct = default)` — サーバーから退出する

Waiting中に`ApplyRoomCommand(LeaveRoom, expected_revision)`を送る。
送信前にWatchを停止し、退出による資格喪失通知と退出応答の競合を避ける。

成功すると、サーバーはメンバー削除、必要なら所有権移譲、残り参加者の準備解除、空室削除を行う。
まず退出後のSnapshotをSOへ保存・通知し、その後DisconnectでSOを空にする。
最後の退出ならCLOSED SnapshotによるRoomClosedが通知された後、RoomSyncでクリアされる。
RoomClosedのハンドラー中に必要な情報を保存すること。`await Leave()`後のSnapshotは通常null。

要求失敗・キャンセル後、まだ同じ接続先が有効ならWatchを再開する。
サーバーでは退出が成立していた場合、再接続はNotFound／PermissionDeniedで終わる可能性がある。
Selecting／Battling中はこの操作を拒否する。Disconnectを退出の代わりとして扱わない。

### `Disconnect() → void` — ローカル受信を停止する

引数なし。通信による退出要求は送らない。
内部の世代を進め、部屋IDを解除、Watchをキャンセル・破棄し、通知済みmatch_id記録を消し、SOをResetOnlineする。
進行中のUnaryはDisconnectだけではキャンセルされないが、その応答は世代チェックで保存されなくなる。
進行中操作のIsBusyは、その操作のfinallyが動くまでtrueのままの場合がある。
このUseCaseは再利用でき、操作完了後にCreate／Join／Resumeできる。

サーバー上の参加・席・準備は残る。正式な退出はLeave。Dispose後にDisconnectを呼ばない。

### `Dispose() → void` — UseCaseを終了する

引数なし。Disconnectに加え、寿命トークンをキャンセル・破棄し、IsBusyをfalseにする。
複数回呼んでも2回目以降は何もしない。サーバーへ退出を送らない。
Dispose後のCreate／Join／変更操作はObjectDisposedException、RefreshはInvalidOperationExceptionになる。
再利用したい場合はConfigureRoomで新しいUseCaseを作る。

## 5. プロパティと通知

### `RoomUseCase.Data → RoomData`

初期化時に渡したSOそのもの。UseCase利用者は読むだけにし、更新はUseCaseへ任せる。

### `RoomUseCase.Failed : Action<Grpc.Core.Status>`

`StatusCode`は失敗種別、`Detail`はサーバー等の説明文。
Create／Join／Resume／変更操作のRpcExceptionと、WatchのRPCエラーで通知する。
Unaryでは通知した後に例外も再throwする。イベントとcatchの両方で同じエラー画面を出さないよう、表示方針を統一する。
Refresh単独、ローカル引数エラー、OperationCanceledExceptionは、このイベントで一律には通知されない。
NetworkManager.OnErrorMessageへ自動転送もしない。

| エラー | 典型例・対応 |
|---|---|
| InvalidArgument | 部屋名や部屋ID不正。入力を修正 |
| Aborted | revision競合。再取得後に再操作。自動再送しない |
| FailedPrecondition | 準備不足、席条件不成立、開始済み等。最新状態を表示 |
| ResourceExhausted | 満室。別の部屋を選ぶ等 |
| Unauthenticated | 再ログイン・UseCase再構成 |
| PermissionDenied | 所有者でない、メンバーでない等。操作／所属を確認 |
| NotFound | 部屋が削除された等。古い画面を利用し続けない |
| Unavailable / DeadlineExceeded | 結果不明の可能性。状態を確認。変更要求を無条件再送しない |
| Cancelled | 呼出元のキャンセルかを判定し、必要なら結果を再確認 |

WatchはNotFound／PermissionDenied／Unauthenticatedで停止する。SOは最後のSnapshotを保持し、ConnectionをUnavailableにする。
その他のRPC失敗や通常EOFではReconnectingとし、1秒後にWatchを再開する。
バックエンドの変更確認間隔は500ms。1秒はクライアントの再接続待機時間であり、通常配信の更新間隔ではない。

### `RoomUseCase.MatchAvailable : Action<string, LobbyPhase>`

| 引数 | 意味 |
|---|---|
| `matchId` | バックエンドが作成した試合の文字列ID。room_idとは別 |
| `phase` | SelectingまたはBattling。通知時点のサーバー段階 |

Snapshotを受理し、RoomData.Changed通知が終わった後に呼ばれる。
同一接続セッション内で`(matchId, phase)`ごとに一度。重複配信では再通知しない。
Disconnectや新しい入室成功で記録をクリアするため、Resume後に同じ試合の通知が再度起こり得る。
初回SnapshotがBattlingならSelecting通知を作り出さず、Battlingを直接通知する。
観戦者にも通知される。編成操作を行うかどうかは自分のmember.Seatを確認する。
新しく購読しただけでは過去通知を再生しない。購読時にはSnapshotも必ず読む。

### `RoomData`の読み取り項目

| プロパティ | 内容 |
|---|---|
| Snapshot | 全RoomSnapshotのクローン。未取得／Reset後はnull。キャッシュして1回の表示に使用可能 |
| Revision | 保存済みrevision。未取得は0。部屋単位の版番号で、日時やイベント件数ではない |
| MatchId | 現在の試合ID。未取得／Waitingでは通常空文字 |
| Connection | Disconnected / Connecting / Connected / Reconnecting / Unavailable |
| IsBusy | 入室系・変更系の操作が進行中か。RefreshとWatchは対象外 |
| room_id / room_name | Snapshotからの互換投影値 |
| room_my_index | usersData上の自分の位置。未所属なら-1。1P/2P番号ではない |
| usersData | 実メンバー数の配列。固定8枠ではない。空き枠の表示は利用側で補う |

SnapshotにはOwnerId、Phase、Members、CanStart、StartBlockReason、CanReady、CanRename、CanEnterRing、CanLeaveRing、CanLeaveRoomもある。
MembersにはUserId、Name、Rate、Seat、Ready、JoinedAtがある。Seatは0=観戦、1=1P、2=2P。
StartBlockReasonはNOT_WAITING、NOT_MEMBER、NOT_OWNER、SEATS_NOT_FILLED、NOT_READY等。開始可能時は空文字。
Can系は受信時点・呼出者向けの情報。Connection=Unavailable時に古いCan=trueをそのまま操作許可として使わない。
usersDataのis_hostはOwnerIdとの一致で計算する。user_stateの1Pと同義ではない。

### `RoomData.Changed : Action<RoomChangeType>`

| type | 発火条件 |
|---|---|
| RoomSync | ResetOnline、初回Snapshot、またはrevisionは進んだが下記分類に差分がない場合 |
| MembersChanged | メンバーのID・名前・レート・席・入室時刻または並びの変化 |
| ReadyChanged | メンバーID・Readyの組の変化。入退室でも発火し得る |
| OwnerChanged | OwnerId変更 |
| NameChanged | RoomName変更 |
| PhaseChanged | PhaseまたはMatchId変更 |
| PermissionsChanged | Can系またはStartBlockReason変更 |
| RoomClosed | 受理したSnapshotがClosed。WatchのNotFoundとは別 |
| ConnectionChanged | SetConnectionで接続状態が変わった場合 |
| OperationChanged | SetBusyでIsBusyが変わった場合 |

保存した全フィールドを、各ハンドラーから参照できる。複数変更時はMembers→Ready→Owner→Name→Phase→Permissionsの順で、存在する差分だけ通知する。Closedなら最後にRoomClosedも通知する。
初回は分類別通知を列挙せずRoomSync（ClosedならRoomClosedも）を通知する。
ResetOnlineでもRoomSyncが起こるためSnapshot=nullを処理すること。初回入室時は「空へのReset」と「初回保存」でRoomSyncが複数回起こり得る。
通知中に別の保存やResetが実行された場合は、旧状態に対する残りの種類通知を打ち切る。
Snapshotの変更通知中は、ConnectionがまだConnectedではない場合がある。Watchは保存後にConnectedを設定する。

## 6. SOを更新する公開メソッド（通常はUseCase専用）

| メソッド | 引数・動作・戻り値 |
|---|---|
| ResetOnline() | 引数なし。Snapshot/JSON/互換フィールドをクリア、Connection=Disconnected、RoomSync通知。IsBusyは変更しない |
| StoreOnline(RoomSnapshot incoming, string playerId) | incoming=受信全状態、playerId=自分のID。無効Snapshot、別部屋、同一以下revisionを拒否してfalse。受理時はクローン・JSON・投影を保存して変更通知、true。別部屋への変更は先にResetが必要 |
| SetConnection(RoomConnectionState state) | 接続状態を保存。変化があればConnectionChanged。同値なら通知なし |
| SetBusy(bool busy) | 実行中状態を保存。変化があればOperationChanged。同値なら通知なし |

InspectorのonlineSnapshotJsonは実行中の確認用。永続セーブや起動時復元ではない。
Snapshotはコピーを返すが、旧互換フィールドはpublicのため直接書き換え可能。利用側から変更すると同期契約を壊すので、必ずUseCaseの操作要求を使用する。

## 7. LobbyConnector（低レベル通信）

通常はUseCase経由で使う。直接呼んでもSO更新・通知・再接続・revision付与は行わない。

| 公開メンバー | 引数・実行内容 |
|---|---|
| LobbyConnector(NetworkClientCore core) | 初期化済みCore。ChannelでUnary、StreamChannelでWatchを実行。各呼出時にSessionHeadersを取得 |
| LobbyConnector(client unary, client streaming, Func<Metadata> headers) | テスト／注入用。前2つは生成LobbyServiceV2Client、headersは各RPCに付けるMetadata生成関数 |
| Task<RoomSnapshot> Create(string name, CancellationToken ct) | nameをCreateLobbyRequest.RoomNameに設定し、CreateLobbyAsync。応答をそのまま返す |
| Task<RoomSnapshot> Join(uint roomId, CancellationToken ct) | JoinLobbyAsync。roomIdは数値部屋ID |
| Task<RoomSnapshot> Get(uint roomId, CancellationToken ct) | GetLobbyAsync。メンバー資格が必要 |
| Task<RoomSnapshot> Apply(RoomCommand request, CancellationToken ct) | ApplyRoomCommandAsync。requestにはRoomId、Action、ExpectedRevisionと必要なReady/RoomNameを設定済みで渡す |
| AsyncServerStreamingCall<RoomSnapshot> Watch(uint roomId, CancellationToken ct) | WatchRoomを開始。呼出者がResponseStream.MoveNext(ct)で読み、最後にcall.Dispose()する |

低レベルのctは省略不可。Unaryはusingでcallを破棄し、エラーはcatchせず上へ返す。自動再送なし。

## 8. 使用例と利用側の責任

以下は呼出側の構成例。実際のUI・Sceneコードには組み込んでいない。

```csharp
// ログイン後の初期化。購読解除のため参照を保持する。
RoomUseCase room = NetworkManager.Instance.ConfigureRoom(roomData);
roomData.Changed += OnRoomChanged;
room.MatchAvailable += OnMatchAvailable;
room.Failed += OnRoomFailed;

void OnRoomChanged(RoomChangeType type)
{
    var snapshot = roomData.Snapshot;
    if (snapshot == null) return; // Reset/退出を表示側で扱う
    // 全保存済み。snapshotとConnection/IsBusyを表示モデルへ渡す。
}

void OnMatchAvailable(string matchId, LobbyPhase phase)
{
    // 利用側が通知を受け取り、編成画面/戦闘画面への移行を決定する。
    // 観戦者は編成送信しない。同じmatchIdへの二重遷移は利用側でも防ぐ。
}

void OnRoomFailed(Grpc.Core.Status status)
{
    // 状態通知として記録する。await側でも同じ失敗を表示するなら二重表示を避ける。
}

try
{
    await room.Join(roomId, cancellationToken);
    // 成功後は自動配信。毎フレームRefreshを呼ばない。
}
catch (OperationCanceledException) { /* 結果が不明なら後で確認 */ }
catch (Grpc.Core.RpcException e) { /* e.StatusCodeに応じて処理 */ }

// 表示側の破棄時。受信継続が必要ならDisposeしない。
roomData.Changed -= OnRoomChanged;
room.MatchAvailable -= OnMatchAvailable;
room.Failed -= OnRoomFailed;
```

操作順の例：

1. 所有者Create、相手Join。各端末でSnapshotの席・所有者を表示。
2. 対戦者2人がそれぞれSetReady(true)。
3. 所有者がCanStartを確認しStartMatch。
4. Selecting通知でBattle.GetRoomGameDataを実行し、現在のmatch_idが通知と一致することを確認。
5. 対戦者がRegisterCharactersで編成登録。両編成登録後にBattle.Ready。
6. BattleのStartedまたはRoomのBattlingを見て表示側が戦闘画面へ進む。
7. 戦闘終了後、RoomのWaitingを表示。退出ならRoom.Leave。

RoomとBattleの配信は独立しているため、同時到着や到着順を仮定しない。
MatchAvailableのハンドラーが登録されていなかった期間は、Snapshotから現在の段階を復元する。

## 9. 検証と実装参照

- `Assets/Scripts/Network/RoomUseCase.cs`
- `Assets/Scripts/Network/LobbyConnector.cs`
- `Assets/Scripts/ChangeableData_server/RoomDataOnline.cs`
- `Assets/Scripts/Network/NetworkManager.cs`
- `Assets/Editor/RoomUseCaseValidation.cs`

Unityメニュー：Tools > Auxilia > Validate room use case。
再接続を含めたバッチ：`-executeMethod RoomUseCaseValidation.RunAll`（`-quit`を付けない）。
模擬RPCで保存順序、認証、重複排除、旧応答破棄、競合後Refresh、再接続、退出を検証する。
実サーバーや複数Unity端末の統合確認とは別。
