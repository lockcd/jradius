#!/bin/bash
set -euo pipefail

# Settings
OUTDIR="bin"
PKGDIR="packages_lib"
TARGET_FRAMEWORK="netstandard2.0"  # Adjust as needed (e.g., net8.0, netstandard2.1)
OUTPUT_EXE="JRadius.exe"

# List all required NuGet packages with versions here
declare -A NUGET_PACKAGES=(
  ["Microsoft.Extensions.Logging"]="9.0.8"
  ["Microsoft.Extensions.Logging.Abstractions"]="9.0.8"
  ["Microsoft.Extensions.DependencyInjection"]="9.0.8"
)

download_and_extract() {
  local pkg=$1
  local ver=$2
  local url="https://www.nuget.org/api/v2/package/${pkg}/${ver}"
  local nupkg_file="${pkg}.${ver}.nupkg"
  local tmp_dir="tmp_${pkg}"

  echo "📦 Downloading $pkg $ver..."
  curl -L -o "$nupkg_file" "$url"

  echo "📂 Extracting package..."
  rm -rf "$tmp_dir"
  mkdir "$tmp_dir"
  unzip -q "$nupkg_file" -d "$tmp_dir"

  local dll_path=""
  local target_path="$tmp_dir/lib/$TARGET_FRAMEWORK"

  if [[ -d "$target_path" ]]; then
    dll_path=$(find "$target_path" -name "*.dll" | head -n 1)
  fi

  if [[ -z "$dll_path" ]]; then
    # fallback to any DLL in lib
    echo "⚠️ Target framework '$TARGET_FRAMEWORK' not found for $pkg, attempting fallback..."
    dll_path=$(find "$tmp_dir/lib" -name "*.dll" | head -n 1)
  fi

  if [[ -f "$dll_path" ]]; then
    mkdir -p "$PKGDIR"
    cp "$dll_path" "$PKGDIR/"
    echo "✅ Copied DLL: $(basename "$dll_path")"
  else
    echo "⚠️ DLL not found for $pkg $ver (target framework: $TARGET_FRAMEWORK)"
  fi

  rm -rf "$tmp_dir"
  rm -f "$nupkg_file"
}

echo "🔄 Starting NuGet packages download and extraction..."
rm -rf "$PKGDIR" # Clean up old packages

for pkg in "${!NUGET_PACKAGES[@]}"; do
  download_and_extract "$pkg" "${NUGET_PACKAGES[$pkg]}"
done

# --- Build JRadius.Core ---
echo "⚙️ Building JRadius.Core library..."
CORE_SRC_FILES=$(find core-dotnet -type f -name "*.cs")
if [[ -z "$CORE_SRC_FILES" ]]; then
  echo "❌ No C# source files found for JRadius.Core."
  exit 1
fi

CORE_REFS="-r:/usr/lib/mono/4.5/mscorlib.dll -r:System.dll -r:System.Core.dll -r:/usr/lib/mono/4.8-api/Facades/System.Runtime.dll -r:/usr/lib/mono/4.8-api/Facades/netstandard.dll -r:/usr/lib/mono/4.8-api/Facades/System.Threading.Tasks.dll"
for dll in "$PKGDIR"/*.dll; do
  CORE_REFS="$CORE_REFS -r:$dll"
done

mkdir -p "$OUTDIR"
mono-csc -target:library -out:"$OUTDIR/JRadius.Core.dll" $CORE_REFS $CORE_SRC_FILES
echo "✅ JRadius.Core.dll built successfully."

# --- Future modules can be added here ---
# echo "⚙️ Building JRadius.Client library..."
# CLIENT_SRC_FILES=...
# CLIENT_REFS=...
# mono-csc -target:library -out:"$OUTDIR/JRadius.Client.dll" $CLIENT_REFS $CLIENT_SRC_FILES -r:"$OUTDIR/JRadius.Core.dll"
# echo "✅ JRadius.Client.dll built successfully."
