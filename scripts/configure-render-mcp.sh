#!/usr/bin/env bash
# Configura o Render MCP no Cursor (~/.cursor/mcp.json).
# Uso:
#   RENDER_API_KEY='rnd_...' ./scripts/configure-render-mcp.sh
# ou rode interativo (pede a chave sem eco).

set -euo pipefail

MCP_JSON="${HOME}/.cursor/mcp.json"

if [[ -z "${RENDER_API_KEY:-}" ]]; then
  read -rs -p "Cole a Render API Key (Account Settings → API Keys, começa com rnd_): " RENDER_API_KEY
  echo
fi

KEY="${RENDER_API_KEY//[[:space:]]/}"
if [[ -z "$KEY" ]]; then
  echo "Erro: RENDER_API_KEY vazia." >&2
  exit 1
fi

if [[ "$KEY" == 20c015f58475ee60346bac823ecb38e9 ]] || [[ ${#KEY} -eq 32 && "$KEY" =~ ^[0-9a-f]+$ ]]; then
  echo "Erro: isso parece Account ID (32 hex), não API Key. Crie uma key em:" >&2
  echo "  https://dashboard.render.com/u/settings#api-keys" >&2
  exit 1
fi

mkdir -p "$(dirname "$MCP_JSON")"

python3 - "$MCP_JSON" "$KEY" <<'PY'
import json
import sys
from pathlib import Path

path = Path(sys.argv[1])
key = sys.argv[2]

data = {}
if path.exists():
    data = json.loads(path.read_text(encoding="utf-8"))

servers = data.setdefault("mcpServers", {})

# Preserva outros servidores (ex.: context7)
render = {
    "url": "https://mcp.render.com/mcp",
    "headers": {
        "Authorization": f"Bearer {key}",
    },
}
servers["render"] = render

path.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
print(f"OK: Render MCP configurado em {path}")
print("Reinicie o Cursor (ou Reload Window) e peça: listar serviços Render no workspace JOTANUNES.")
PY
