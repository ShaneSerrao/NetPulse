#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
if ! command -v pandoc >/dev/null 2>&1; then
  echo "pandoc not found; please install: sudo apt-get install -y pandoc" >&2
  exit 1
fi
mkdir -p docs/out
pandoc -o docs/out/WHITEPAPER.pdf docs/WHITEPAPER.md
pandoc -o docs/out/DIAGRAMS.pdf docs/DIAGRAMS.md
echo "PDFs written to docs/out"