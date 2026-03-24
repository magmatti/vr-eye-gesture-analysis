using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Data;
using NUnit.Framework;

public class BlinkDataLogger : MonoBehaviour
{
    
    [Header("Tracking Components")]
    public OVRFaceExpressions faceExpressions;
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public float beepInterval = 2.0f;

    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;
    private Coroutine soundRoutine;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"BlinkData_{timestamp}.csv");
    }

    void Update()
    {
        // button "A" on quest's controller will start or stop logging
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (!isLogging) StartBlinkTest();
            else StopBlinkTest();
        }

        if (isLogging && faceExpressions != null && faceExpressions.FaceTrackingEnabled)
        {
            GetDataAndLogIntoFile();
        }
    }

    private void StartBlinkTest()
    {
        isLogging = true;
        writer = new StreamWriter(filePath, true);

        writer.WriteLine("Time_ms,LeftBlinkWeight,RightBlinkWeight,LeftConfidence,RightConfidence");
        Debug.Log($"Started Blink Logging to: {filePath}");

        soundRoutine = StartCoroutine(SoundSequence());
    }

    private void StopBlinkTest()
    {
        isLogging = false;
        if (soundRoutine != null) StopCoroutine(soundRoutine);

        if (writer != null)
        {
            writer.Close();
            writer = null;
        }

        Debug.Log("Stopped Blink Logging");
    }

    private IEnumerator SoundSequence()
    {
        // sound sequence to play beep every 2 seconds
        while (isLogging)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
            }
            yield return new WaitForSeconds(beepInterval);
        }
    }

    private void GetDataAndLogIntoFile()
    {
        string timeMs = (Time.time * 1000f).ToString("F0");

        // these return a float from 0.0 (eyes open) to 1.0 (eyes closed)
        float leftBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
        float rightBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.BrowLowererR);

        float lConf = leftEyeGaze != null ? leftEyeGaze.Confidence : 0f;
        float rConf = rightEyeGaze != null ? rightEyeGaze.Confidence : 0f;

        string line = $"{timeMs},{leftBlink},{rightBlink},{lConf},{rConf}";
        writer.WriteLine(line);
    }

    void OnDestroy()
    {
        if (writer != null) writer.Close();
    }
}
