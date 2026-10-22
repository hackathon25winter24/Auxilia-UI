# BattleConnector利用補足：Room UseCaseから戦闘へ

2026-10-19時点。現在`BattleUseCase`という独立クラスはなく、戦闘の操作・受信窓口は`BattleConnector`。
RoomUseCaseと戻り値・エラー・再送の契約が異なるため、本書で全公開メソッドを整理する。
Room側は[Room UseCaseリファレンス](room-usecase-reference.md)、演出通知の詳細は[online-battle-v2.md](online-battle-v2.md)を参照。

## 1. 役割・共通仕様

- `NetworkManager.Instance.Battle`から取得するMonoBehaviour。通常はNetworkManagerが初期化する。
- 通信で受けたSnapshot／Definitions／ActionLogsをBattleDataForOnlineへ保存し、その後にChanged(type)を通知する。
- 描画・Scene遷移は行わない。入力側からは操作意図だけを送る。
- 数値room_idは試合の検索用、文字列match_idは試合操作用。各操作は保存済みState.MatchIdを自動使用する。
- メインスレッドから呼び、非同期操作はawaitする。ctは個別操作のキャンセル。省略時default。
- State未取得のまま編成・準備・戦闘操作を行うとInvalidOperationException。
- Snapshotが返っても、既に新しい配信を受信していることがある。表示には戻り値ではなくDataの最新状態を使う。
- RpcExceptionは多くの公開操作で捕捉され、NetworkClientCore.ShowErrorMessage経由で通知した後にnullが返る。RoomUseCaseの「Failed通知後に再throw」とは異なる。
- キャンセルやローカル引数エラーは例外として伝わり得る。nullチェックと例外処理の両方が必要。

## 2. 初期化と保存先

### `Initialize(NetworkClientCore networkCore, BattleDataForOnline data) → void`

networkCoreは初期化済みの通信基盤。通常ChannelとStreamChannelから生成クライアントを作る。
dataは受信先SO。同じSOをViewManagerなどが購読する。BindDataを呼んで保存先を設定する。
通常はNetworkManagerが呼ぶため、利用側で繰り返さない。

### `BindData(BattleDataForOnline data) → void`

data=nullはArgumentNullException。同じインスタンスなら何もしない。
異なるSOなら配信停止、内部世代更新、部屋ID解除、新SOのResetRuntimeを行う。
進行中の古い世代の応答は保存しなくなる。サーバーの試合や参加状態は変更しない。

### `Data → BattleDataForOnline`

現在の保存先。State、Snapshot、Definitions、ActionLogs等はここから読む。
RoomData.Snapshotと違い、戦闘SOの型付きgetterはすべて防御的クローンを返す設計ではないため、参照先を書き換えない。

## 3. 定義と試合取得

### `GetDefinitions(CancellationToken ct = default) → UniTask<DefinitionsResponse>`

GetDefinitions RPCを送る。キャラクター定義JSONとrulesVersionをSOへ保存し、内容が変われば定義更新を通知する。
編成選択・移動コスト表示などに使用する。受信エラーでは通常null。
部屋を切り替えるとResetRuntimeで定義も消えるため、現在の部屋の初期取得後に呼ぶとよい。

### `CreateGameData(uint roomId, CancellationToken ct = default) → UniTask<Snapshot>`

roomIdはサーバーの数値部屋ID。Battle.CreateGameを送って試合を作成・取得する。
サーバー側で所有者・参加資格・対戦者準備状態を検証する。
新しい部屋なら古い配信を停止し、SOを初期化する。
成功応答を保存した後、未終了の試合について継続配信を自動開始する。

新Room経路ではRoomUseCase.StartMatchが試合を作成するので、通知後はこのメソッドではなくGetRoomGameDataを使う。
既に作成済みの試合がある場合でも、現在のバックエンドはこの作成経路に所有者制限を適用する。

### `GetRoomGameData(int roomId, CancellationToken ct = default) → UniTask<Snapshot>`

roomIdに対応する試合をGetRoomGame RPCで取得する。負数はuintへのchecked変換で例外になる。
違う部屋へ移る場合は配信停止・SO初期化・世代更新を行う。
Snapshot保存後、未終了で必要なら配信開始。同じ試合を再取得しても購読を増やさない。
個別ctは自動開始した配信の寿命に引き継がれない。

### `GetGameData(int roomId, CancellationToken ct = default) → UniTask<Snapshot>`

GetRoomGameDataの別名。同じ引数・副作用・戻り値。

### `GetGameData(CancellationToken ct = default) → UniTask<Snapshot>`

保存済みのmatch_idを使ってGetGameData RPCを送る。現在試合の状態再取得。
初回の試合検索の代わりにはならない。このオーバーロード単独では配信を開始しない。
新しい状態を保存・通知し、古い応答はSOへ反映しない。

## 4. 編成・戦闘開始

### `RegisterCharacters(string[] definitionIds, CancellationToken ct = default) → UniTask<Snapshot>`

definitionIdsはキャラクター定義の文字列ID配列。表示用の配列番号や、試合内の個体IDではない。
バックエンドが3体の定義IDと編成条件を検証する。例：`new[] { "zina", "jude", "dana" }`。
現在のmatch_idとともにRegisterCharacters RPCを送り、編成変更後のSnapshotを保存・通知する。
戦闘開始前の対戦者のみ操作する。観戦者は呼ばない。編成再登録で準備状態が変化し得るので最新Stateを確認する。

### `Ready(CancellationToken ct = default) → UniTask<Snapshot>`

保存済みmatch_idに対してBattle.Ready RPCを送る。引数にtrue/falseはない。
両方の編成が登録済みであることが必要。対戦者がそれぞれ準備確定し、サーバーが戦闘開始条件を判断する。
結果のState.Startedや配信を確認する。呼んだ端末だけで戦闘開始を確定しない。
RoomUseCase.SetReady(true)とは別段階。

### `CancelGame(CancellationToken ct = default) → UniTask<Snapshot>`

保存済みmatch_idの対戦準備をCancelGame RPCで中止する。
戦闘開始前の対戦者用。開始後は拒否される。成功すると試合が終了扱いとなり、ルームはWaitingへ戻る。
Roomからの退出は行わない。Room側の配信・再取得で復帰状態を確認する。

## 5. 戦闘中の操作

以下4操作は送信を直列化する。RoomUseCaseと違い、進行中操作があれば待機する。
送信直前のState.Revisionと新規CommandIdを自動付与する。呼出側でダメージ・消費コストを決定して送らない。
Unavailable／DeadlineExceededなら500ms後、**同じCommandId・revisionで1度だけ再送**する。
拒否された操作を新しいIDでやり直すことはない。失敗時は最新Stateの再取得を試み、通常nullを返す。

### `SendMove(string characterId, int x, int y, CancellationToken ct = default) → UniTask<Snapshot>`

| 引数 | 意味 |
|---|---|
| characterId | State.Characters[].Id。試合内の個体ID |
| x / y | 移動先のサーバー座標。表示上の2P反転座標をそのまま送らない |
| ct | 待機・通信・必要な再送／復旧処理のキャンセル |

ApplyMove RPCを送る。手番、所有キャラクター、移動可能範囲、コスト等はサーバーが検証・適用する。
返却Stateを保存し、移動等の演出通知を発火する。ローカルの位置を先に確定しない。

### `SendAttack(string characterId, int attackIndex, Position target, Position direction, CancellationToken ct = default) → UniTask<Snapshot>`

| 引数 | 意味 |
|---|---|
| characterId | 攻撃者の試合内個体ID |
| attackIndex | サーバーの技番号。0始まりで、定義に対応する番号を使用 |
| target | Game.Network.V2.Position。技の対象となるサーバー座標 |
| direction | Game.Network.V2.Position。方向指定が必要な技の方向成分。targetの別名ではない |
| ct | キャンセル |

targetとdirectionは送信要求を組み立てるときにCloneする。nullを渡せるが、その技に必要な指定が欠ければサーバーで不正操作になる可能性がある。
ApplyAttack RPCを送り、サーバーが範囲・対象・ダメージ・状態異常・追撃などを決定する。
複数の演出イベントが返る場合はSO保存後に順序付きで通知する。

### `SendTurnEnd(CancellationToken ct = default) → UniTask<Snapshot>`

現在の試合にEndTurn RPCを送る。終了させるプレイヤーIDは渡さない。
手番・フェーズをサーバーが検証し、遷移後のSnapshotを保存する。

### `Surrender(CancellationToken ct = default) → UniTask<Snapshot>`

現在の試合にSurrender RPCを送る。勝者IDをクライアントで指定しない。
サーバーが降参と試合終了を処理し、終了後のStateを保存・通知する。Roomの退出は別操作。

## 6. 履歴と継続受信

### `FetchActionLog(ulong afterSequence, uint limit = 100, CancellationToken ct = default) → UniTask<LogResponse>`

afterSequenceは既取得のログ番号。返るのはこの番号より後の履歴。最初から取得する場合は0。
limitは1ページの希望件数。現在のバックエンドは0または100超を100に補正する。
保存済みmatch_idを使って取得し、SOへ履歴を統合・重複排除して変更通知する。
戻り値のNextSequenceは次ページ取得のカーソルに使用する。

通常の演出ごとに呼ぶ必要はない。Snapshot同梱の演出履歴に欠落があった場合、コネクタが自動取得する。
履歴が現在Snapshotより先まで存在しても、対応する盤面が届くまで演出通知を保留する。

### `StartStream(CancellationToken ct = default) → void`

保存済みmatch_idに対してStreamGameを開始する。StateがなければInvalidOperationException。
既存配信を停止して置き換える。ここへ明示的に渡したctは配信全体の寿命に連結される。
GetRoomGameData／CreateGameData成功時には自動開始するので、通常は別途呼ばない。

受信応答をSOへ保存・通知する。終了状態を受信すると配信を終了する。
通信断や正常EOFでは3秒待って再接続し、Unauthenticated／PermissionDenied／NotFoundでは停止する。
RoomUseCaseの再接続待機時間1秒とは別設定。

### `StopStream() → UniTask`

引数なし。配信のキャンセルトークンをキャンセル・破棄し、完了済みUniTaskを返す。
SOを消さず、サーバーに降参・退出・キャンセルを送らない。
進行中のすべてのUnary処理の終了を待つメソッドでもない。
再開はStartStreamまたはGetRoomGameData。コネクタ破棄時にも自動停止する。

## 7. Roomからの引き継ぎ例

```csharp
// Selecting/Battlingの通知を受けた利用側の処理の概略。
// 非同期イベントハンドラーでは、外側で例外・キャンセルを必ず管理する。
var roomSnapshot = roomData.Snapshot;
if (roomSnapshot == null || roomSnapshot.MatchId != notifiedMatchId) return;

var battle = NetworkManager.Instance.Battle;
var initial = await battle.GetRoomGameData(checked((int)roomSnapshot.RoomId), ct);
if (initial == null || battle.Data.State?.MatchId != notifiedMatchId) return;
await battle.GetDefinitions(ct);

// 観戦者には以下を実行させない。
// 対戦者の編成確定操作：
var selected = await battle.RegisterCharacters(definitionIds, ct);
if (selected == null) return;
// 両者の編成が登録済みであることを受信データから確認した後：
var ready = await battle.Ready(ct);
if (ready == null) return;
// Startedがtrueになるまで受信状態を待つ。ここでSceneを直接変更するのは利用側の責務。
```

通知されたmatch_idと取得後のState.MatchIdが違えば、次の試合等へ切り替わっている可能性がある。
古い通知のまま編成や操作を送らず、Roomの最新Snapshotからやり直す。
この確認後も非同期処理の間に状態は変わり得るため、利用側の操作寿命・画面世代も管理する。

## 8. 実装参照

- `Assets/Scripts/Network/BattleConnector.cs`
- `Assets/Scripts/ChangeableData_server/BattleDataForOnline.cs`
- `Assets/Scripts/BattlesV2/ViewManager.cs`
- `Assets/Editor/BattleV2Validation.cs`

本書追加にあたり実装コードは変更していない。
