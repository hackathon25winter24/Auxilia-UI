# Unity開発用サーバーへの接続

バックエンドの`Unity/Auxilia`で実行：

```powershell
docker compose -f compose.local.yml up -d --build --wait --wait-timeout 180
```

Docker Desktopが起動していることが前提。MariaDBとAuxiliaが起動し、認証・ロビー・戦闘を提供する。
接続先は`http://localhost:18080/`。Editorではこれが既定値。
`Tools > Auxilia > Server`からLocalまたはProductionを選び、Play Modeを再開始すると切り替わる。
`AUXILIA_SERVER_URL`環境変数が設定されている場合はそちらを優先する。
ビルドしたプレイヤーの既定値は本番URL。ローカルへ自動切替はしない。

運用・停止・ポート変更・DB保持の詳細はバックエンドの`docs/local-development.md`を参照。
本変更は通信先の切替のみ。ゲームUIとScene、既存UseCaseへの接続状況は変更していない。
