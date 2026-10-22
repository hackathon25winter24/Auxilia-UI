# ルームから対戦・復帰までの画面接続

## 実装した流れ

1. Home → MatchingScene（一覧）。一覧取得は従来の読み取りAPIを使用する。
2. 作成は `RoomUseCase.Create`、入室は `Join` を呼ぶ。席はサーバーが割り当て、応答をRoomDataに保存してからRoomSceneへ進む。
3. RoomSceneは `RoomData.Changed` を購読し、保存済みスナップショットから名前・席・準備・権限を表示する。参加者ごとの追加問い合わせや旧1秒ポーリングは行わない。
4. 準備ボタンは `SetReady`。オーナーを含め各プレイヤーが準備する。サーバーから `CanStart=true` が届いたオーナーのボタンは「対戦開始」となり、`StartMatch` を呼ぶ。失敗時に自動で再送しない。
5. 部屋の `Selecting` を検知してSelectSceneへ進む。途中復帰で `Battling` の場合も、SelectSceneで現在の戦闘状態を取得してからBattleSceneへ進む。
6. SelectSceneは `GetGameData(roomId)` で試合IDとストリームを確立し、`GetDefinitions` を取得する。「決定」は `BattleCharacterIds.ToServer` による定義ID変換 → `RegisterCharacters` → `Ready`。登録失敗時はReadyを送らない。
7. BattleDataForOnlineの通知で `State.Started` を確認したときだけBattleSceneへ進む。観戦者も同じ状態を購読する。ローカルの選択タイマーによる開始は行わない。
8. `State.Finished` を確認してResultSceneへ一度だけ遷移する。
9. 「部屋に戻る」はRoomUseCaseの `Refresh`（未接続なら `Resume`）後にRoomSceneへ戻る。退室せず席を維持する。再戦は部屋で再び準備・開始する。
10. リザルトからホーム・一覧へ出る場合は `Leave` 成功後に移動する。

## 接続の所有者と表示

NetworkManagerは同じユーザー・RoomDataへのConfigureRoomで既存UseCaseを再利用する。RoomUseCaseのWatchRoomは画面をまたいで継続する。画面破棄時は表示通知の購読だけ解除する。BattleConnectorのストリームもコネクタが所有し、選択→戦闘で維持する。

既存の画面・ボタン配置は維持した。RoomSceneの名前変更・席変更の既存UnityEventもUseCaseに接続した。ResultSceneには共有RoomDataと現行名のBattleDataForOnline参照を設定した。

## 検証

- `dotnet build Assembly-CSharp-Editor.csproj --no-restore`：ランタイム・Editorコードのコンパイル。
- 関連4シーンの21件のUnityEventのメソッド接続と共有RoomDataの参照を静的確認。
- バックエンド `go test ./handler/grpcv2 ./infrastructure/gormv2`：既存RPC・WatchRoom・席競合・準備・戦闘開始・終了処理のテスト。
- Unityを2クライアントで動かした画面操作の通し検証は未実施。開いているEditorは終了していない。

## 2クライアントで確認する操作

1. 異なるアカウントでログインし、一方が作成、他方が入室する。双方に同じ参加者・異なる席が表示されること。
2. 両者の準備を任意の順で操作する。相手が未準備のとき開始されず、両者が準備した後にオーナーだけが開始できること。
3. 開始後、双方が選択シーンへ移ること。異なる3体を決定し、片方のみReadyでは戦闘へ進まず、両者Readyで戦闘に進むこと。
4. 対戦終了後、両者がリザルトへ移り、部屋へ戻ると元の部屋ID・席が維持され、readyが解除されていること。
5. 再び準備・開始すると新しい試合IDになり、前の試合の終了状態でリザルトへ戻されないこと。
6. 観戦者の参加、オーナーの席変更、名前変更、待機中の退室を確認する。
7. 通信切断時に作成・入室・準備が成功扱いにならず、連打で重複遷移しないこと。復帰後の最新状態を確認する。
