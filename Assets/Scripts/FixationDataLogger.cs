using UnityEngine;
using System.IO;
using System;
using System.Collections;
using TMPro;
using System.Globalization;

public class FixationDataLogger : MonoBehaviour
{
    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    [Header("Test Environment")]
    [Tooltip("The red dot the user stares at")]
    public GameObject fixationTarget;

    [Header("UI Panels")]
    public GameObject infoCanvas;
    public GameObject alertCanvas;
    
    [Header("UI Controls")]
    public TMP_Dropdown durationDropdown;
    public TextMeshProUGUI alertMessageText;

    private float testStartTime;
    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;

    void Start()
    {
        infoCanvas.SetActive(true);
        alertCanvas.SetActive(false);
        fixationTarget.SetActive(false);
    }

    void Update()
    {
        if (isLogging && leftEyeGaze != null && rightEyeGaze != null)
        {
            LogData();
        }
    }

    public void StartTest()
    {
        float testDurationInSeconds = 10f; 
        if (durationDropdown.value == 1) testDurationInSeconds = 20f;
        else if (durationDropdown.value == 2) testDurationInSeconds = 30f;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"FixationData_{timestamp}.csv");
        
        writer = new StreamWriter(filePath, false);
        writer.WriteLine("Time_ms,HeadRotX,HeadRotY,HeadRotZ,HeadRotW," +
                 "LeftLocalRotX,LeftLocalRotY,LeftLocalRotZ,LeftLocalRotW," +
                 "LeftWorldRotX,LeftWorldRotY,LeftWorldRotZ,LeftWorldRotW," +
                 "LeftConfidence,RightLocalRotX,RightLocalRotY," +
                 "RightLocalRotZ,RightLocalRotW,RightWorldRotX," +
                 "RightWorldRotY,RightWorldRotZ,RightWorldRotW," +
                 "RightConfidence");

        infoCanvas.SetActive(false);
        fixationTarget.SetActive(true);
        
        testStartTime = Time.time;
        isLogging = true;
        StartCoroutine(TestTimer(testDurationInSeconds));
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

    private void EndTest()
    {
        isLogging = false;

        if (writer != null)
        {
            writer.Close();
            writer = null;
        }

        fixationTarget.SetActive(false);
        alertCanvas.SetActive(true);

        alertMessageText.text = $"Test Complete!\nData saved to:\n{filePath}\n";
    }

    private void LogData()
    {
        string timeMs = ((Time.time - testStartTime) * 1000f)
            .ToString("F0", CultureInfo.InvariantCulture);

        Quaternion hRot = centerEyeAnchor.rotation;
        Quaternion lLocRot = leftEyeGaze.transform.localRotation;
        Quaternion rLocRot = rightEyeGaze.transform.localRotation;
        Quaternion lRot = leftEyeGaze.transform.rotation;
        Quaternion rRot = rightEyeGaze.transform.rotation;

        float lConf = leftEyeGaze.Confidence;
        float rConf = rightEyeGaze.Confidence;

        string line = $"{timeMs}," +
              $"{hRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{hRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{hRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{hRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lLocRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lLocRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lLocRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lLocRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lConf.ToString("F2", CultureInfo.InvariantCulture)}," +
              $"{rLocRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rLocRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rLocRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rLocRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{rConf.ToString("F2", CultureInfo.InvariantCulture)}";
        
        writer.WriteLine(line);
    }

    void OnDestroy()
    {
        if (writer != null) writer.Close();
    }
}
