#!/bin/bash

# 1. setup adb tool to use this script -> https://developer.android.com/tools/adb
# 2. in order to run this script you have to add exec permission
# 3. to add exec permission run chmod +x collect_data.sh inside /Scripts directory
# 4. than simply run it to collect data -> ./collect_data.sh

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DEST=~/Downloads/eye_data_$TIMESTAMP
SOURCE=/sdcard/Android/data/com.Politechnika.VrEyeGestureAnalysis/files/

# checking device connection
DEVICE=$(adb devices | grep -v "List of devices" | grep "device$")
if [ -z "$DEVICE" ]; then
    echo "No headset connected. Please connect your device and try again."
    exit 1
fi
echo "Device found."

# search for .csv files in destination folder
CSV_FILES=$(adb shell ls "$SOURCE" 2>/dev/null | grep "\.csv$")
if [ -z "$CSV_FILES" ]; then
    echo "No .csv files found in $SOURCE. Use test env to save data"
    exit 1
fi
echo "Found CSV result files. Starting pull..."

mkdir -p "$DEST"
echo "$CSV_FILES" | while read file; do
    adb pull "$SOURCE$file" "$DEST/"
done

COUNT=$(ls "$DEST" | wc -l | tr -d ' ')
echo "Pulled $COUNT CSV file(s) to $DEST"
