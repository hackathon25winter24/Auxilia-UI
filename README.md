# ローカル環境でAuxiliaのサーバーを起動する

```
docker compose -f compose.local.yml up -d --build
```

# 停止する。(DBデータは残す)

```
docker compose -f compose.local.yml down
```

# サーバーログを見る

```
docker compose -f compose.local.yml logs -f app
```
