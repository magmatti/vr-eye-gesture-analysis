#!/bin/bash

# 1. in order to run this script you have to add exec permission
# 2. to add exec permission run chmod +x collect_data.sh inside /Scripts directory
# 3. than simply run it to collect data -> ./collect_data.sh

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DEST=~/Downloads/eye_data_$TIMESTAMP
SOURCE=/sdcard/Android/data/com.Politechnika.VrEyeGestureAnalysis/files/

mkdir -p "$DEST"

adb shell ls "$SOURCE" | grep "\.csv$" | while read file; do
    adb pull "$SOURCE$file" "$DEST/"
done

echo "Files successfully saved to $DEST"
