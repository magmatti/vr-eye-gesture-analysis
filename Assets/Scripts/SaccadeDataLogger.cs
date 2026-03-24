using UnityEngine;
using System.IO;
using System;
using System.Collections;
using Unity.VisualScripting;

public class SaccadeDataLogger : MonoBehaviour
{
    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    [Header("Saccade Target Settings")]
    public Transform targetPivot;

    [Header("Dot Jump Iterval (in seconds)")]
    public float jumpInterval = 2.0f;

    private Vector3[] jumpAngles = new Vector3[]
    {
        Vector3.zero, 
        new Vector3(0, 15, 0), 
        Vector3.zero, 
        new Vector3(0, -15, 0),
        Vector3.zero,
        new Vector3(-10, 0, 0),
        Vector3.zero,
        new Vector3(10, 0, 0)
    };

    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;
    private Coroutine saccadeRoutine;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"SaccadeData_{timestamp}.csv");
    }

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (!isLogging) StartSaccadeTest();
            else StopSaccadeTest();
        }

        if (isLogging && leftEyeGaze != null && rightEyeGaze != null)
        {
            GetDataAndLogIntoFile();
        }
    }

    private void StartSaccadeTest()
    {
        isLogging = true;
        writer = new StreamWriter(filePath, true);

        writer.WriteLine("Time_ms,TargetRotX,TargetRotY,LeftRotX,LeftRotY,LeftRotZ,LeftRotW,LeftConfidence,RightRotX,RightRotY,RightRotZ,RightRotW,RightConfidence");
        Debug.Log($"Started Saccade Logging to: {filePath}");

        saccadeRoutine = StartCoroutine(SpawnDotsInNewPositions());
    }

    private void StopSaccadeTest()
    {
        isLogging = false;
        if (saccadeRoutine != null) StopCoroutine(saccadeRoutine);

        if (writer != null)
        {
            writer.Close();
            writer = null;
        }

        targetPivot.localEulerAngles = Vector3.zero;
        Debug.Log("Stopped saccade logging");
    }

    private IEnumerator SpawnDotsInNewPositions()
    {
        // setting new positions for dots, looping through jumpAngles array
        int index = 0;
        while (isLogging)
        {
            targetPivot.localEulerAngles = jumpAngles[index];
            index = (index + 1) % jumpAngles.Length;
            yield return new WaitForSeconds(jumpInterval);
        }
    }

    private void GetDataAndLogIntoFile()
    {
        string timeMs = (Time.time * 1000f).ToString("F0");
        
        // current target location data to log with eye data for reference
        float targetX = targetPivot.localEulerAngles.x;
        float targetY = targetPivot.localEulerAngles.y;

        Quaternion lRot = leftEyeGaze.transform.rotation;
        Quaternion rRot = rightEyeGaze.transform.rotation;

        float lConf = leftEyeGaze.Confidence;
        float rConf = rightEyeGaze.Confidence;

        string line = $"{timeMs},{targetX},{targetY},{lRot.x},{lRot.y},{lRot.z},{lRot.w},{lConf},{rRot.x},{rRot.y},{rRot.z},{rRot.w},{rConf}";
        writer.WriteLine(line);
    }

    void OnDestroy()
    {
        if (writer != null) writer.Close();
    }
}
