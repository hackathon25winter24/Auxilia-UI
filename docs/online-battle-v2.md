# V2オンライン戦闘の受信と画面更新

`BattleConnector`がV2の応答を受信し、Unityのメインスレッドで
`BattleDataForOnline`に保存する。更新があれば`Changed`を通知し、
それを購読した`ViewManager.ChangeView(string type)`がtypeに対応するイベントを発火する。

```text
V2 RPC / StreamGame
    → BattleConnector
    → BattleDataForOnline（保存・投影・演出の重複排除）
    → Changed(type)
    → ViewManager.ChangeView(type)
    → Grid / Character / UI / BaseUI
```

## 保存するデータ

| プロパティ | 内容 |
| --- | --- |
| `Snapshot` | Snapshot全体。roomId、レート、rulesVersion、選択済みプレイヤー、ログ番号、演出ウィンドウを含む |
| `State` | Snapshot内のState全体。プレイヤー・拠点・個体ID・HP・効果・技使用履歴・形態・地形・手番・期限・勝敗・イベント等 |
| `Definitions` | GetDefinitionsの全応答。キャラクター定義JSONとrulesVersion |
| `ActionLogs` | この試合で取得した履歴。command、before、after、presentationを保持し、sequenceで重複排除 |
| `NextLogSequence` | 最後に取得した履歴応答のnextSequence |
| `PresentationSequence` | 連続して受信済みの演出バッチのsequence |
| `HasPresentationGap` | 最新Snapshotまでの演出履歴に不足があるか |

生成されたProtobufのオブジェクトはUnity標準のシリアライズ対象ではないため、
完全な応答をクローンしてSO内に保持し、同時に`Snapshot Json`、`Definitions Json`、
`Action Logs Json`へ保存してInspectorでも確認できるようにしている。
これはプレイ中の受信ストアであり、端末へのセーブデータではない。
`player1`、`player2`、`uniqueGrids`等は既存UI用の投影値で、Stateの一部である。
全情報を参照するには上記の型付きプロパティを使う。
受信ストアをViewや入力側から書き換えないこと。

Stateの座標はサーバー座標のまま保存する。2P向けの左右反転はViewのみで行う。
キャラクターの通信IDは`State.Characters[i].Id`（文字列の個体ID）。
`CharactersBattleData.unique_id`は旧アセット配列用の番号で、通信には使わない。
同一キャラクター定義を両プレイヤーが選んでも、個体IDは混同しない。
旧`now_character_move_cost`はV2では未提供値として-1にする。
編成の基礎移動コストは`BaseMoveCost(definitionId)`を使う。
状態異常・地形を含む実際の消費コストと移動可否はサーバーが判定する。

## 接続とコマンド

ログインは`BattleServiceV2.Login`でセッショントークンを取得する。
Battle・ルーム・プロフィール更新にはBearer認証を付ける。
既存のユーザープロフィールAPIはバックエンドに残っているものを利用する。

1. `RoomServiceV2.StartMatch`で試合を作成する。
2. `Battle.GetRoomGameData(roomId, ct)`で現在のmatchIdを取得・保存する。
3. `Battle.GetDefinitions(ct)`で定義を取得・保存する。
4. `Battle.RegisterCharacters(definitionIds, ct)`、`Battle.Ready(ct)`で準備する。
5. サーバーの`State.Started`を確認してBattleSceneへ進む。
6. BattleSceneの`BattleOnlineManager`が初期取得後に`StartStream(ct)`を開始する。

```csharp
// いずれも成功応答は自動的にSOへ保存され、ChangeViewが呼ばれる。
await NetworkManager.Instance.Battle.SendMove(characterId, serverX, serverY, ct);
await NetworkManager.Instance.Battle.SendAttack(characterId, attackIndex,
    new Game.Network.V2.Position { X = targetX, Y = targetY },
    new Game.Network.V2.Position { X = directionX, Y = directionY }, ct);
await NetworkManager.Instance.Battle.SendTurnEnd(ct);
await NetworkManager.Instance.Battle.Surrender(ct);
```

コマンドには最新revisionと新しいcommandIdが自動で付く。
移動・攻撃・ターン終了・降参は送信を直列化する。
通信結果が不明なUnavailable/DeadlineExceededでは、同じcommandId・同じrevisionで一度再送する。
拒否された操作を別のcommandIdで自動実行することはない。
ダメージ計算、HP・地形の更新、自動ターン進行はサーバーが担当する。

ルームのV2サービスは双方向ストリームを持つが、現在のgRPC-Web接続では
双方向ストリームを利用できないため、ルーム一覧・参加者一覧を定期取得する。
戦闘の`StreamGame`はサーバーストリームとして受信する。

## Viewと演出の拡張

BattleSceneのManagersには`BattleOnlineManager`と`ViewManager`を配置済み。
TitleSceneのNetworkManagerを含め、同じ`BattleDataForOnline.asset`を参照する。
別のシーンで使用する場合も、受信先とViewの参照先を同じSOに設定する。
シーン終了で戦闘ストリームをキャンセルし、Viewの購読を解除する。

受信した`presentation_batches[].events[].type`をそのまま通知する。
`State.LastEvent.Type`だけでは、1操作内の追撃・複数対象・復活などが失われるので使用しない。
SOの`Changed`は`Action<string>`で、購読は`Changed += ChangeView`とする。
`Changed += ChangeView()`はメソッド呼び出しになり、イベントの購読にはならない。
`Initialize(data)`でSOを変更すると、以前のSOの購読を解除して新しいSOを購読する。

`ChangeView(type)`は既存の`moved`、`damaged`、`healed`等の引数なしイベントに振り分ける。
1応答に`MOVED → DAMAGED → DAMAGED → COST_CHANGED`があれば、この4回すべてを順に通知する。
受信トランザクションの保存完了後に発火するため、各コールバックから最新盤面を読める。
アニメーションに必要な途中の座標やHPは、最終Stateの差分ではなく演出イベントを使う。

```csharp
// 登録先のViewManagerと受信先は同じBattleDataForOnlineを参照させる。
viewManager.damaged += OnDamaged;

void OnDamaged()
{
    // 通知中の対象・原因・数値。非同期演出に渡すときはクローンを保持する。
    var damage = viewManager.CurrentEvent.Clone();
    ulong sequence = viewManager.CurrentSequence;
    // damage.TargetId / Amount / BeforeValue / AfterValue / Cause などを使って描画する。
}
```

`CurrentEvent`と`CurrentSequence`は通知中のみ有効で、通知終了後はnullと0に戻る。
既存の`TryDequeuePresentation(out batch)`も利用可能だが、同じ演出を
typeイベントとキューの両方から再生しないこと。

| 戦闘イベントを持たない更新 | type | ViewManagerのイベント |
| --- | --- | --- |
| 初回同期、時計だけの更新、演出なしの盤面更新 | `STATE_SYNC` | `state_synced` |
| キャラクター定義の更新 | `DEFINITIONS_CHANGED` | `definitions_changed` |
| 取得した履歴・履歴カーソルの更新 | `ACTION_LOG_CHANGED` | `action_logs_changed` |

これら3種類はフロント側の同期通知で、サーバーの攻撃イベントではない。
`state_synced`は現在のStateをそのまま描画する用途に使う。
初回取得は過去の攻撃を再生せず、受信した盤面をそのまま表示する。
以後のUnary応答・Stream・再接続に含まれる同じsequenceは重複しない。
32遷移の同梱範囲から漏れた場合だけ、ConnecterがFetchActionLogをページ取得して補完する。
補完中も最新Stateは保持し、演出は連続するsequenceになるまで待機する。

同じSnapshotの再受信は変更通知を出さない。
古いrevision・logSequence・同revisionで古いserverTimeは無視する。
新しい試合では前の試合の履歴・演出キューをリセットする。

## 検証

Unity 6000.0.84f1の`Tools > Auxilia > Validate V2 receive pipeline`から、
実サーバーにアクセスせずに受信処理を検証できる。
完全な応答の保持、個体IDの区別、巻き戻り防止、演出順序・重複排除・履歴補完、
保存後のChangeView通知、コマンドの認証・revision・commandIdを確認する。
`Validate typed view events`では、typeの順序、同じtypeの複数発火、重複排除、
履歴補完、通知中のイベント情報、SO変更時の購読解除を確認する。

```powershell
Unity.exe -batchmode -nographics -quit -projectPath "<Auxilia-UI>" `
  -executeMethod BattleV2Validation.Run -logFile "validation.log"
```

実際の2クライアント接続や各演出の見た目は、このオフライン検証には含まれない。
Protoの正本はバックエンドの`proto/v2`。
フロント内の`Assets/Scripts/Network/Proto`と`Generated/V2`を更新するときは、
依存する`room.proto`と`room_match.proto`も同じ版でC#コードを生成する。
