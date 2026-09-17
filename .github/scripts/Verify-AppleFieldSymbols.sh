#!/usr/bin/env bash
#
# Checks that every [Field (..., "__Internal")] in an Apple binding names a symbol the framework
# it is bound against actually exports.
#
# A binding project only compiles C#. A field naming a symbol that libwebrtc has since dropped
# builds, packs, restores and compiles in a consumer's app without complaint, then fails when that
# app *links*:
#
#   Undefined symbols for architecture arm64:
#     "_kRTCIsacCodecName", referenced from: <initial-undefines>
#
# Nothing in the app referenced it. "__Internal" makes the native linker emit -u <symbol>, so the
# symbol is required unconditionally whether or not anything uses it. That is issue #37, which
# shipped and stayed broken for a year because every CI job here builds bindings and packages and
# none of them links an app.
#
# Runs on macOS: nm is what reads a Mach-O binary's exports.
#
# Usage: .github/scripts/Verify-AppleFieldSymbols.sh
set -euo pipefail

cd "$(dirname "$0")/../.."

bindings=WebRTCme.Bindings/Maui

# The committed frameworks, not built output - these are what the bindings are compiled against and
# what ends up in the package. Mac Catalyst's is flat here; build-versioned-framework.sh rebuilds it
# into the Versions/A layout during the build, and that rewrite does not change which symbols the
# binary exports.
declare -a slices=(
  "ios-arm64|$bindings/WebRTCme.Bindings.Maui.iOS/WebRTC.xcframework/ios-arm64/WebRTC.framework/WebRTC|$bindings/WebRTCme.Bindings.Maui.iOS/ApiDefinitions.cs"
  "ios-simulator|$bindings/WebRTCme.Bindings.Maui.iOS/WebRTC.xcframework/ios-arm64_x86_64-simulator/WebRTC.framework/WebRTC|$bindings/WebRTCme.Bindings.Maui.iOS/ApiDefinitions.cs"
  "maccatalyst|$bindings/WebRTCme.Bindings.Maui.MacCatalyst/WebRTC.framework/WebRTC|$bindings/WebRTCme.Bindings.Maui.MacCatalyst/ApiDefinitions.cs"
)

failed=0

for slice in "${slices[@]}"; do
    IFS='|' read -r name binary api <<< "$slice"

    if [[ ! -f $binary ]]; then
        echo "::error::$name: no framework binary at $binary"
        failed=1
        continue
    fi

    symbols=$(mktemp)
    fields=$(mktemp)

    nm -gU "$binary" | awk '{print $3}' | sed 's/^_//' | sort -u > "$symbols"

    # The leading [[:space:]]* matters: without it this also matches commented-out bindings and
    # reports symbols that were already dealt with as if they were still declared.
    grep -E '^[[:space:]]*\[Field \("[A-Za-z0-9_]+", "__Internal"\)\]' "$api" \
        | sed -E 's/.*\("([A-Za-z0-9_]+)".*/\1/' | sort -u > "$fields"

    missing=$(comm -23 "$fields" "$symbols")

    if [[ -n $missing ]]; then
        echo "::error::$name declares __Internal fields the framework does not export:"
        while read -r symbol; do
            [[ -n $symbol ]] && echo "::error::  $symbol"
        done <<< "$missing"
        echo "Comment the binding out, or update the framework. Either way an app linking against"
        echo "this would fail with 'Undefined symbols for architecture arm64'."
        failed=1
    else
        echo "$name: $(wc -l < "$fields" | tr -d ' ') declared fields, all exported by the framework."
    fi

    rm -f "$symbols" "$fields"
done

exit $failed
