using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DataLoggers.DataPoints;

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
        metronomeRoutine = StartCoroutine(MetronomeSequence());
    }

    private IEnumerator MetronomeSequence()
    {
        yield return new WaitForSeconds(initialDelay);

        while (isLogging)
        {
            if (metronomeAudio != null && metronomeAudio.clip != null)
            {
                metronomeAudio.Play();
            }
            yield return new WaitForSeconds(beepInterval);
        }
    }

    private void CaptureData()
    {
        dataBuffer.Add(new BlinkDataPoint
        {
            TimeMs = (Time.time - testStartTime) * 1000f,
            LeftBlinkWeight = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL),
            RightBlinkWeight = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR),
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
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Time_ms,LeftBlinkWeight,RightBlinkWeight,LeftConfidence,RightConfidence");

            StringBuilder sb = new StringBuilder(128);
            foreach (var p in dataBuffer)
            {
                sb.Clear();
                sb.Append(p.TimeMs.ToString("F0", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LeftBlinkWeight.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RightBlinkWeight.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LConf.ToString("F2", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RConf.ToString("F2", CultureInfo.InvariantCulture));
                writer.WriteLine(sb.ToString());
            }
        }
        dataBuffer.Clear();
    }
}
