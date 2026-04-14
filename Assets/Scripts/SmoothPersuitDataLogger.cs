using UnityEngine;
using System.IO;
using System;
using System.Collections;
using TMPro;
using System.Globalization;

public class SmoothPursuitDataLogger : MonoBehaviour
{
    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    [Header("Pursuit Target Settings")]
    public Transform targetPivot;
    [Tooltip("How far left and right the dot sweeps (in degrees)")]
    public float maxAngle = 20f; 
    [Tooltip("How fast the dot oscillates")]
    public float movementSpeed = 2f; 

    [Header("UI Panels")]
    public GameObject infoCanvas;
    public GameObject alertCanvas;
    
    [Header("UI Controls")]
    public TMP_Dropdown durationDropdown;
    public TextMeshProUGUI alertMessageText;

    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;
    private float testStartTime;

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
            float timeActive = Time.time - testStartTime;
            float currentAngle = Mathf.Sin(timeActive * movementSpeed) * maxAngle;
            targetPivot.localEulerAngles = new Vector3(0, currentAngle, 0);

            LogData(currentAngle);
        }
    }

    public void StartTest()
    {
        float testDuration = 10f; 
        if (durationDropdown.value == 1) testDuration = 20f;
        else if (durationDropdown.value == 2) testDuration = 30f;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"SmoothPursuitData_{timestamp}.csv");
        
        writer = new StreamWriter(filePath, false);
        writer.WriteLine("Time_ms,TargetRotY,LeftRotX,LeftRotY,LeftRotZ,LeftRotW,LeftConfidence,RightRotX,RightRotY,RightRotZ,RightRotW,RightConfidence");

        infoCanvas.SetActive(false);
        targetPivot.gameObject.SetActive(true);
        targetPivot.localEulerAngles = Vector3.zero;
        
        testStartTime = Time.time;
        isLogging = true;
        StartCoroutine(TestTimer(testDuration));
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

        targetPivot.gameObject.SetActive(false);
        alertCanvas.SetActive(true);
        alertMessageText.text = $"Smooth Pursuit Test Complete!\nData saved to:\n{filePath}\n";
    }

    private void LogData(float targetY)
    {
        string timeMs = (Time.time * 1000f).ToString("F0", CultureInfo.InvariantCulture);

        Quaternion lRot = leftEyeGaze.transform.rotation;
        Quaternion rRot = rightEyeGaze.transform.rotation;

        float lConf = leftEyeGaze.Confidence;
        float rConf = rightEyeGaze.Confidence;

        string line = $"{timeMs}," +
              $"{targetY.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +
              $"{lConf.ToString("F2", CultureInfo.InvariantCulture)}," +
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
