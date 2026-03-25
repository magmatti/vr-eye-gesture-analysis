using UnityEngine;
using System.IO;
using System;

public class FixationDataLogger : MonoBehaviour
{
    
    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;

    private bool isLogging = false;
    private StreamWriter writer;
    private string filePath;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"FixationData_{timestamp}.csv");
    }

    void Update()
    {
        // button "A" on quest's controller will start or stop logging
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            ToggleLogging();
        }

        if (isLogging && leftEyeGaze != null && rightEyeGaze != null)
        {
            GetDataAndLogIntoFile();
        }

        // press button "B" on quest's controller to go back to menu
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuScene");
        }
    }

    private void ToggleLogging()
    {
        isLogging = !isLogging;

        if (isLogging)
        {
            writer = new StreamWriter(filePath, true);
            writer.WriteLine("Time_ms,LeftRotX,LeftRotY,LeftRotZ,LeftRotW,LeftConfidence,RightRotX,RightRotY,RightRotZ,RightRotW,RightConfidence");
            Debug.Log($"Started Logging to: {filePath}");
        }
        else
        {
            if (writer != null)
            {
                writer.Close();
                writer = null;
            }
            Debug.Log("Stopped Logging");
        }
    }

    private void GetDataAndLogIntoFile()
    {
        string timeMs = (Time.time * 1000f).ToString("F0");

        Quaternion lRot = leftEyeGaze.transform.rotation;
        Quaternion rRot = rightEyeGaze.transform.rotation;

        float lConf = leftEyeGaze.Confidence;
        float rConf = rightEyeGaze.Confidence;

        string line = $"{timeMs},{lRot.x},{lRot.y},{lRot.z},{lRot.w},{lConf},{rRot.x},{rRot.y},{rRot.z},{rRot.w},{rConf}";
        writer.WriteLine(line);
    }

    void OnDestroy()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}
