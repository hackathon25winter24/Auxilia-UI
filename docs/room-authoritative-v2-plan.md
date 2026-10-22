# Roomのサーバー権威型リファクタ計画

利用方法の詳細は[Room UseCaseリファレンス](room-usecase-reference.md)を参照。
戦闘への引き継ぎ・各戦闘操作は[BattleConnector利用補足](battle-connector-reference.md)に記載。

## 担当範囲と実装順

1. API・状態遷移の確定（本書）。
2. バックエンド：整合したSnapshot、認証、サーバーによる席決定、開始権限、単方向配信、競合テスト。
3. コネクタ：LobbyServiceV2接続、応答と配信の共通保存、再接続、部屋世代・revisionによる旧応答破棄。
4. RoomData：全Snapshotを保持するScriptableObject、保存後のChanged(type)通知。
5. フロント担当：SOを読む表示、操作要求、画面遷移、Sceneのボタン参照修正。
6. 2クライアントで入室から編成登録・戦闘開始まで統合検証。

1・2に続き、3・4のUseCase・コネクタ・SO・通知を実装済み。5・6は未実施。描画・Sceneは変更しない。
既存APIは残すが、新フロントは新しいLobbyServiceV2を使用する。

### 2026-10-19 実装記録

- 1・2のAPI定義、サーバー登録、Snapshot保存・revision管理、操作処理、配信処理を実装。
- Proto正本：`Unity/Auxilia/proto/v2/lobbyV2.proto`。Go生成コードは同リポジトリの`pb/v2`。
- 起動時の既存Migrateから`lobby_v2_versions`テーブルを追加。既存の部屋データは初回取得時にrevisionを割り当てる。
- 所有者のみ開始できる条件を既存StartMatch／Battle.CreateGameにも適用。既存APIの非所有者による開始はPermissionDeniedになる。
- 既存JoinRoomも再入室で席・準備状態を保持するよう修正。従来の観戦者ready=true要求は拒否する。
- `go test ./...`、関連パッケージの`go vet`、gRPC-Web配信テストを実施。
- MySQL専用テスト用DSNは未設定。並行テストはSQLiteで成功しているが、実MySQLの行ロック検証は未実施。
- 実サーバーへのデプロイ、UnityのC#生成、コネクタ接続、UI表示・画面遷移、実機2クライアント検証は今回行っていない。
- CreateLobbyには冪等キーがない。通信結果不明なら既存部屋一覧で確認し、作成要求を無条件に再送しない。

### コネクタ・SO・通知の実装

`LobbyConnector`が生成C#クライアントを使用し、Unaryとサーバーストリームを別チャンネルで通信する。
`RoomUseCase`が操作と購読のライフサイクルを持つ。既存MatchingConnectorの旧APIは互換用に残す。
`RoomDataOnline.cs`は既存RoomDataをpartialで拡張し、Snapshotの全情報とInspector用JSONを保存する。
従来のフィールドは表示互換の投影値。所有者はowner_idで判定する。Snapshotのgetterはクローンを返す。

```csharp
// ログイン後、接続層の初期化コードから一度呼ぶ。同じSOを表示側へ渡す。
var room = NetworkManager.Instance.ConfigureRoom(roomData);
roomData.Changed += OnRoomChanged;
room.MatchAvailable += OnMatchAvailable;
room.Failed += OnRoomFailed;
await room.Join(roomId, cancellationToken); // またはCreate(name)、Resume(roomId)
```

全メソッドはUnityメインスレッドから呼ぶ。ConfigureRoomは前のUseCaseをDisposeするため、毎描画呼ばない。
アカウント切替時は必ず再構成する。RoomDataを暗黙に探索・生成しない。
既存UIは未接続のため、そのままでは新経路を利用しない。旧UIのSO書き込みと新UseCaseを同時稼働させない。

| 公開操作 | 意味 |
|---|---|
| Create / Join | 作成・入室の成功応答を保存し、継続受信を開始 |
| Resume | GetLobbyで既存メンバー資格を確認して受信再開。再入室・席変更しない |
| Refresh | 最新Snapshot取得。古い応答は保存しない |
| EnterRing / LeaveRing | 席変更の希望を送る |
| SetReady(bool) / Rename / StartMatch | 最新revisionと操作意図を送る |
| Leave | サーバーで退出確定後、受信停止・SO解除 |
| Disconnect / Dispose | ローカル購読を停止。サーバーからの退出を意味しない |

操作の同時実行は拒否して二重クリックを防ぐ。IsBusyとOperationChangedで処理中状態を通知する。
接続先変更はLeaveまたはDisconnect後に行う。後者の場合、以前のサーバー参加状態は残る。
Unary応答とStreamを共通の保存処理に通し、room_id・世代・revisionで旧応答と重複を除外する。
競合や結果不明の操作は再送せずRefreshし、元のRpcExceptionを呼出元へ返す。FailedにもStatusを通知する。
Create/Joinのキャンセルではサーバー処理が既に成功した可能性があるため、必要に応じResumeや部屋一覧で確認する。

通信断・通常EOFは1秒待って再接続する。購読のNotFound/PermissionDenied/Unauthenticatedは再接続を停止し、
Connection=UnavailableとFailedを通知する。最後のSnapshotは診断用に保持するため、操作可否表示にはConnectionも参照する。
受信接続は操作ごとのCancellationTokenから独立し、Disconnect/DisposeまたはNetworkManagerの破棄まで存続する。

Changedの型はRoomChangeType：RoomSync、MembersChanged、ReadyChanged、OwnerChanged、NameChanged、
PhaseChanged、PermissionsChanged、RoomClosed、ConnectionChanged、OperationChanged。
1応答で複数の変更種類を通知する場合も、全フィールド保存後に発火する。RoomClosedはサーバーのCLOSED Snapshotを受信した場合。
サーバー削除によるWatchの終了はFailed(NotFound)で判定する。
MatchAvailable(matchId, phase)はSELECTING/BATTLINGごとに同一match_idを一度通知する（接続セッション内）。
新しい購読者は初回にData.Snapshotも読む。UseCaseはScene遷移や描画メソッドを呼ばない。
戦闘の初期取得・編成送信への接続は通知の利用側で行う。観戦者かどうかはSnapshot.Membersで判定する。

Unityの`Tools > Auxilia > Validate room use case`でオフライン検証可能。
バッチ検証は`-executeMethod RoomUseCaseValidation.RunAll`（`-quit`なし）で再接続と既存Battle検証も実行する。

## 確定したルール

- 所有者と1P/2Pは別概念。作成者は所有者かつ1P。入室者は空いている1P、2P、観戦席の順にサーバーが割り当てる。定員8名。
- 同じユーザーの再入室は席・準備状態を保持する。観戦と対戦席の変更は操作要求で行い、席番号は送らない。
- 両対戦者が準備完了したら所有者が開始要求できる。所有者が観戦席でも開始可能。
- 開始要求は準備を暗黙に変更しない。両者とも明示的なSetReadyが必要。
- 待機中の参加者構成・席変更で対戦者の準備を解除する。所有者退出時は入室順（同時刻はuser_id順）で所有権を移譲。空室は削除する。
- SELECTING/BATTLINGでは入退室・改名・席・ルーム準備変更を禁止。対戦中の切断は即座に退出や敗北へ変換しない。
- 切断してもメンバーは保持する。WatchRoom再接続で復帰。従来のjoined_atによる3分削除は廃止する。自動退出のためのheartbeat/leaseは別途導入するまで無効（放置部屋は明示退出で解消）。

## 状態遷移

`WAITING → SELECTING → BATTLING → WAITING`

- StartMatch成功でmatch_idが確定しSELECTINGへ。これは戦闘開始ではない。
- 対戦者のみBattleServiceV2.RegisterCharactersを呼び、両編成が揃ってからReadyを送る。
- BattleのState.Started=trueでBATTLINGへ。観戦者は登録・Readyを送らない。
- 対戦終了または編成中のCancelGameでWAITINGへ戻り、全員の準備を解除する。match_idは空になる。
- 最後の退出にはCLOSED Snapshotを返す。削除済み部屋のGet/WatchはNotFound。

## API（game.network.v2.LobbyServiceV2）

全RPCにBearer認証が必要。user_id、owner_id、is_gaming、確定席番号を入力させない。

| RPC | 入力 | 出力 |
|---|---|---|
| CreateLobby | room_name（trim後1〜128 UTF-8 bytes） | 作成・入室済みRoomSnapshot |
| JoinLobby | room_id | 入室済みRoomSnapshot。再入室は状態を維持 |
| GetLobby | room_id | メンバーのみ取得可能なRoomSnapshot |
| WatchRoom | room_id | 初回全Snapshot、その後revision変化時に全Snapshotを配信するサーバーストリーム |
| ApplyRoomCommand | room_id、action、expected_revision、必要に応じready/room_name | 操作確定後のRoomSnapshot |

actionはENTER_RING、LEAVE_RING、SET_READY、RENAME、START_MATCH、LEAVE_ROOM。
SET_READYは反転ではなく希望値を送る。全コマンドに最新expected_revisionが必要。
競合はAborted。取得し直してユーザーに再操作させ、古い要求を自動的に再実行しない。
通信結果不明時はGetLobbyで確定状態を確認する。CreateLobbyは冪等キーを持たないため自動再送しない。

Snapshotはroom_id、revision、room_name、owner_id、phase、match_id、members、操作可否を持つ。
memberはuser_id、name、rate、seat、ready、joined_at。can_startとstart_block_reasonは呼出者ごとに計算。
revisionは部屋の公開状態変化で単調増加。観測間の変更をまとめることがあり、イベント履歴番号ではない。
サーバーは部屋行をロックしてSnapshotを組み立て、DBへrevisionと公開状態ハッシュを保存する。
旧API・戦闘API経由の変更も次の取得/配信で検出する。更新後のSnapshot返却も同じトランザクション内で行う。
WatchRoomは500ms間隔でDB確認し、変化時のみ配信。即時通知基盤ではなく最大約500ms＋通信時間の追随。

エラー：Unauthenticated、PermissionDenied、NotFound、InvalidArgument、FailedPrecondition、ResourceExhausted、Aborted。
Watchの購読解除は退出ではない。認証・メンバー資格は配信ループで再検証する。

## コネクタとフロントへの引き継ぎ

MatchingConnector → RoomData保存 → Changed(type) → 表示・遷移の購読者。
コネクタから描画ManagerやSceneChangeManagerを呼ばない。
初回は保存済みSOも読む。再接続・Unary/Streamの重複をrevisionで除外し、遷移をmatch_idで一度に制限する。
ROOM_SYNC、MEMBERS_CHANGED、READY_CHANGED、OWNER_CHANGED、NAME_CHANGED、PHASE_CHANGED、ROOM_CLOSEDを通知候補とする。
現行の1P＝所有者判定、クライアントの席決定、プロフィール個別取得、直接SO書換えを廃止する。
SelectUIManagerのコメントアウトされた編成送信をV2へ接続し、RoomのSELECTING通知とBattleのStarted通知を区別する。

## 検証

バックエンド：認証、権限、初回/更新配信、revision競合、再入室、準備・席変更、満室、所有者退出、開始の二重実行防止、終了後復帰。
SQLiteで論理・RPC検証、MySQLで実際の行ロック競合検証を行う。利用可能な環境のみ実行し未実施項目を明記する。
後続統合：2クライアントで同じmatch_idへ一度だけ遷移すること、切断復帰、観戦者の操作拒否、古い応答の破棄。
