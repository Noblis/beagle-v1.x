#!/usr/bin/env bash
# Compare: run the MERGED (Maleki efficiency + Noblis multi-objective) engine on Feynman
# for N minutes and report total births, so it can be compared against the original
# multi-objective fork (run_mo_feynman.sh).
#
# Usage: ./run_merged_feynman.sh [formula=1] [minutes=1]
set -euo pipefail
cd "$(dirname "$0")"
FORMULA="${1:-1}"
MIN="${2:-1}"

echo "==> beagle-merge (Maleki efficiency + multi-objective): Feynman #$FORMULA, ${MIN} min" >&2
dotnet build Beagle/Run -c Release >/dev/null
cd Beagle/Run/bin/Release/net10.0
dotnet Run.dll RunFeynman="$FORMULA" NoEscMenu StopAfterMin="$MIN"
