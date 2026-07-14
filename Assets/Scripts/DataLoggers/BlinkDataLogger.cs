using UnityEngine;
using System.Collections.Generic;
using DataLoggers.CSVWriter;
using DataLoggers.CSVWriter.Definitions;
using DataLoggers.DataPoints;
using DataLoggers.TestConfigurations;

public class BlinkDataLogger : BaseDataLogger
{
    public OVRFaceExpressions faceExpressions;
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    public AudioSource metronomeAudio;
    public float beepInterval = 2.0f;
    public float initialDelay = 3.0f; 

    private Coroutine metronomeRoutine;

    private List<BlinkDataPoint> dataBuffer;

    void Update()
    {
        if (isLogging && faceExpressions != null && faceExpressions.FaceTrackingEnabled)
        {
            CaptureData();
        }
    }

    public void StartTest()
    {
        dataBuffer = new List<BlinkDataPoint>(3000);
        InitializeTest("Blink");
        StartTestTimer();
        metronomeRoutine = StartCoroutine(
            MetronomeSequence.Run(
                metronomeAudio,
                initialDelay,
                beepInterval,
                () => isLogging));
    }

    private void CaptureData()
    {
        dataBuffer.Add(new BlinkDataPoint
        {
            TimeMs = (Time.time - testStartTime) * 1000f,
            LeftBlinkWeight = faceExpressions
                .GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL),
            RightBlinkWeight = faceExpressions
                .GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR),
            LConf = leftEyeGaze != null ? leftEyeGaze.Confidence : 0f,
            RConf = rightEyeGaze != null ? rightEyeGaze.Confidence : 0f
        });
    }

    protected override void EndTest()
    {
        isLogging = false;
        if (metronomeRoutine != null) StopCoroutine(metronomeRoutine);
        
        WriteBufferToFile();
        ShowCompletionAlert("Blink");
    }

    private void WriteBufferToFile()
    {
        CsvFile.Write(filePath, dataBuffer, BlinkCsvDefinition.Definition);
        dataBuffer.Clear();
    }
}
