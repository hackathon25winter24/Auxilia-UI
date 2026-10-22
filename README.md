# ローカル環境でAuxiliaのサーバーを起動する方法
前提としてdockerアプリが立ち上がっていることが必要

## サーバーを起動
```
docker compose -f compose.local.yml up -d --build
```

## 停止する。(DBデータは残す)

```
docker compose -f compose.local.yml down
```

## サーバーログを見る

```
docker compose -f compose.local.yml logs -f app
```

# ローカルと本番環境の切り替え
Unity Editorから、メニューの Tools → Auxilia → Server から Local (localhost 18080) または Production を選び、Play Modeを再起動すると反映される。
