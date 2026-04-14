using UnityEngine;
using System.IO;
using System;
using System.Collections;
using TMPro;
using System.Globalization;

public class SaccadeDataLogger : MonoBehaviour
{
    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    [Header("Saccade Target Settings")]
    public Transform targetPivot; 
    public float jumpInterval = 2.0f; 
    
    private Vector3[] jumpAngles = new Vector3[] {
        Vector3.zero, new Vector3(0, 15, 0), Vector3.zero, new Vector3(0, -15, 0),
        Vector3.zero, new Vector3(-10, 0, 0), Vector3.zero, new Vector3(10, 0, 0)
    };

    [Header("UI Panels")]
    public GameObject infoCanvas;
    public GameObject alertCanvas;
    
    [Header("UI Controls")]
    public TMP_Dropdown durationDropdown;
    public TextMeshProUGUI alertMessageText;

    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;
    private Coroutine saccadeRoutine;

    void Start()
    {
        infoCanvas.SetActive(true);
        alertCanvas.SetActive(false);
        targetPivot.gameObject.SetActive(false);
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
        float testDuration = 10f; 
        if (durationDropdown.value == 1) testDuration = 20f;
        else if (durationDropdown.value == 2) testDuration = 30f;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"SaccadeData_{timestamp}.csv");
        
        writer = new StreamWriter(filePath, false);
        writer.WriteLine("Time_ms,TargetRotX,TargetRotY,LeftRotX,LeftRotY,LeftRotZ,LeftRotW,LeftConfidence,RightRotX,RightRotY,RightRotZ,RightRotW,RightConfidence");

        infoCanvas.SetActive(false);
        targetPivot.gameObject.SetActive(true);
        targetPivot.localEulerAngles = Vector3.zero;
        
        isLogging = true;
        StartCoroutine(TestTimer(testDuration));
        saccadeRoutine = StartCoroutine(SaccadeSequence());
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

    private IEnumerator SaccadeSequence()
    {
        int index = 0;
        while (isLogging)
        {
            targetPivot.localEulerAngles = jumpAngles[index];
            index = (index + 1) % jumpAngles.Length;
            yield return new WaitForSeconds(jumpInterval);
        }
    }

    private void EndTest()
    {
        isLogging = false;
        if (saccadeRoutine != null) StopCoroutine(saccadeRoutine);

        if (writer != null)
        {
            writer.Close();
            writer = null;
        }

        targetPivot.gameObject.SetActive(false);
        alertCanvas.SetActive(true);
        alertMessageText.text = $"Saccade Test Complete!\nData saved to:\n{filePath}\n";
    }

    private void LogData()
    {
        string timeMs = (Time.time * 1000f).ToString("F0", CultureInfo.InvariantCulture);
        float targetX = targetPivot.localEulerAngles.x;
        float targetY = targetPivot.localEulerAngles.y;

        Quaternion hRot = centerEyeAnchor.rotation;
        Quaternion lLocRot = leftEyeGaze.transform.localRotation;
        Quaternion rLocRot = rightEyeGaze.transform.localRotation;
        Quaternion lRot = leftEyeGaze.transform.rotation;
        Quaternion rRot = rightEyeGaze.transform.rotation;

        float lConf = leftEyeGaze.Confidence;
        float rConf = rightEyeGaze.Confidence;

        string line = $"{timeMs}," +
              $"{targetX.ToString("F2", CultureInfo.InvariantCulture)}," +
              $"{targetY.ToString("F2", CultureInfo.InvariantCulture)}," +
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
