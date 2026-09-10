#!/bin/sh
# Rebuilds WebRTC.framework as a versioned macOS bundle, from the flat copy in the repository.
#
# Why this exists: macOS codesign requires a framework embedded in an app's Contents/Frameworks to
# be a versioned bundle - Versions/A plus symlinks at the root - and refuses anything else with
# "bundle format is ambiguous (could be app or framework)". A flat, iOS-style framework does not
# work here, with Info.plist at the root or in Resources; both were tried.
#
# The repository cannot store it that way. Git on Windows checks symlinks out as small text files
# holding their target path unless core.symlinks is on, so a versioned bundle arrives with a
# 23-byte WebRTC where the 26MB binary should be - and that is exactly what shipped in every
# Windows-built package until it was noticed. So the repository keeps the framework flat, which
# survives any checkout, and this script reassembles the layout macOS wants, at build time, on
# macOS only.
#
#   $1  flat framework in the repository (source)
#   $2  versioned framework to create (destination, usually under obj/)
set -e

src=$1
dst=$2

if [ -z "$src" ] || [ -z "$dst" ]; then
    echo "usage: $0 <flat-framework> <versioned-framework>" >&2
    exit 2
fi

if [ ! -f "$src/WebRTC" ]; then
    echo "error: $src/WebRTC not found - the flat framework is missing or incomplete." >&2
    exit 1
fi

# The destination is deleted below, so refuse if it is the source. An MSBuild property that has
# not been defined yet expands to nothing, which is enough to collapse the two paths onto each
# other - and that deletes the committed framework. It happened once; hence this check.
src_real=$(cd "$src" 2>/dev/null && pwd -P)
dst_real=$(cd "$dst" 2>/dev/null && pwd -P || echo "")
if [ -n "$dst_real" ] && [ "$src_real" = "$dst_real" ]; then
    echo "error: source and destination are the same path ($src_real) - refusing to delete it." >&2
    exit 1
fi

rm -rf "$dst"
mkdir -p "$dst/Versions/A"

# -R keeps the executable bit on the binary, which the linker and codesign both need.
cp -R "$src/Headers"   "$dst/Versions/A/Headers"
cp -R "$src/Modules"   "$dst/Versions/A/Modules"
cp -R "$src/Resources" "$dst/Versions/A/Resources"
cp    "$src/WebRTC"    "$dst/Versions/A/WebRTC"

ln -s A "$dst/Versions/Current"
ln -s Versions/Current/Headers   "$dst/Headers"
ln -s Versions/Current/Modules   "$dst/Modules"
ln -s Versions/Current/Resources "$dst/Resources"
ln -s Versions/Current/WebRTC    "$dst/WebRTC"

echo "Rebuilt $dst as a versioned framework."
