using UnityEngine;
using System.IO;
using System;
using System.Collections;
using TMPro;

public abstract class BaseDataLogger : MonoBehaviour
{
    [Header("Base UI Panels")]
    public GameObject infoCanvas;
    public GameObject alertCanvas;
    
    [Header("Base UI Controls")]
    public TMP_Dropdown durationDropdown;
    public TextMeshProUGUI alertMessageText;

    protected float testStartTime;
    protected bool isLogging = false;
    protected string filePath;

    protected virtual void Start()
    {
        infoCanvas.SetActive(true);
        alertCanvas.SetActive(false);

        // --- 90 Hz Hardware Override ---
        if (OVRManager.display != null)
        {
            OVRManager.display.displayFrequency = 90.0f;
            Debug.Log($"[Eye Tracking] Display frequency forced to: {OVRManager.display.displayFrequency} Hz");
        }
        else
        {
            Debug.LogWarning("[Eye Tracking] OVRManager.display is null. Cannot force 90 Hz.");
        }
    }

    public void RestartTest()
    {
        alertCanvas.SetActive(false);
        infoCanvas.SetActive(true);
    }

    protected float GetSelectedDuration()
    {
        if (durationDropdown.value == 1) return 20f;
        if (durationDropdown.value == 2) return 30f;
        return 10f;
    }

    protected void InitializeTest(string testName)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path.Combine(Application.persistentDataPath, $"{testName}Data_{timestamp}.csv");
        
        infoCanvas.SetActive(false);
        testStartTime = Time.time;
        isLogging = true;
        
        StartCoroutine(TestTimer(GetSelectedDuration()));
    }

    private IEnumerator TestTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndTest();
    }

    protected void ShowCompletionAlert(string testName)
    {
        alertCanvas.SetActive(true);
        alertMessageText.text = $"{testName} Test Complete!\nData saved to:\n{filePath}\n";
    }

    protected abstract void EndTest();
}