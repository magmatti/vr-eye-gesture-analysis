# VR Eye Gesture Analysis

A Unity VR application for collecting eye-tracking data on Meta Quest Pro.
The project provides guided test scenes for fixation, saccades, and blinks,
then saves the captured tracking data as CSV files.

## What it records

- Eye gaze rotations for the left and right eye
- Head rotation from the center eye anchor
- Eye tracking confidence values
- Blink weights from Meta face expression tracking
- Target rotations and timestamps

CSV files are saved to Unity's `Application.persistentDataPath` 
with timestamped filenames after each test case finishes.
The app targets a `90 Hz` runtime,
 which is the maximum supported display frequency on Meta Quest Pro.

## Running the project

Open the project in Unity `6000.3.10f1`, let Unity restore the packages
and run it on a Meta Quest Pro with eye and face tracking enabled 
(the user will be prompted for permissions).
Start from `MainMenuScene` to choose an individual test or the combined gesture test.

## Getting data

After running tests on the headset, connect the device over USB and use 
[ADB](https://developer.android.com/tools/adb) to pull CSV files from app storage.

You can use:

```bash
adb shell ls /sdcard/Android/data/com.Politechnika.VrEyeGestureAnalysis/files/
```

```bash
adb pull /sdcard/Android/data/com.Politechnika.VrEyeGestureAnalysis/files/ ./data/
```