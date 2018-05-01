#!/usr/bin/env bash

##
# This script builds the extension for the Chrome browser.
#
##
echo "Building Google Search Navigator for Chrome"

# copy the sources into the working directory
BIN=build/chrome
OBJ="$BIN/obj"
echo "Copying files..."

# cleanup the previous build
rm -rf "$OBJ"
mkdir -p "$OBJ"

cp src/* "$OBJ"

echo "Creating package..."
zip -FS "$BIN/package.zip" $OBJ/*

echo "Build complete"
