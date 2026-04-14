using UnityEngine;
using System.IO;
using System;
using System.Collections;
using TMPro;
using System.Globalization;

public class BlinkDataLogger : MonoBehaviour
{
    [Header("Tracking Components")]
    public OVRFaceExpressions faceExpressions;
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    [Header("Beep Sound Settings")]
    public AudioSource metronomeAudio;
    public float beepInterval = 2.0f;
    public float initialDelay = 3.0f; 

    [Header("UI Panels")]
    public GameObject infoCanvas;
    public GameObject alertCanvas;
    
    [Header("UI Controls")]
    public TMP_Dropdown durationDropdown;
    public TextMeshProUGUI alertMessageText;

    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;
    private Coroutine metronomeRoutine;

    void Start()
    {
        infoCanvas.SetActive(true);
        alertCanvas.SetActive(false);
    }

    void Update()
    {
        if (isLogging && faceExpressions != null && faceExpressions.FaceTrackingEnabled)
        {
            LogData();
        }
    }

    public void StartTest()
    {
        float testDuration = 10f; 
        if (durationDropdown.value == 1) testDuration = 20f;
        else if (durationDropdown.value == 2) testDuration = 30f;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"BlinkData_{timestamp}.csv");
        
        writer = new StreamWriter(filePath, false);
        writer.WriteLine("Time_ms,LeftBlinkWeight,RightBlinkWeight,LeftConfidence,RightConfidence");

        infoCanvas.SetActive(false);
        
        isLogging = true;
        StartCoroutine(TestTimer(testDuration));
        metronomeRoutine = StartCoroutine(MetronomeSequence());
    }

    public void RestartTest()
    {
        alertCanvas.SetActive(false);
        infoCanvas.SetActive(true);
    }

    private IEnumerator TestTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndTest();
    }

    private IEnumerator MetronomeSequence()
    {
        // initial beep delay
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

    private void EndTest()
    {
        isLogging = false;
        if (metronomeRoutine != null) StopCoroutine(metronomeRoutine);

        if (writer != null)
        {
            writer.Close();
            writer = null;
        }

        alertCanvas.SetActive(true);
        alertMessageText.text = $"Blink Test Complete!\nData saved to:\n{filePath}\n";
    }

    private void LogData()
    {
        string timeMs = (Time.time * 1000f).ToString("F0", CultureInfo.InvariantCulture);

        float leftBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
        float rightBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);

        float lConf = leftEyeGaze != null ? leftEyeGaze.Confidence : 0f;
        float rConf = rightEyeGaze != null ? rightEyeGaze.Confidence : 0f;

        string line = $"{timeMs}," +
              $"{leftBlink.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rightBlink.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lConf.ToString("F2", CultureInfo.InvariantCulture)}," +
              $"{rConf.ToString("F2", CultureInfo.InvariantCulture)}";

        writer.WriteLine(line);
    }

    void OnDestroy()
    {
        if (writer != null) writer.Close();
    }
}
