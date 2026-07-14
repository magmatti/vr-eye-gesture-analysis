using UnityEngine;
using System.IO;
using System;
using System.Collections;
using TMPro;

public abstract class BaseDataLogger : MonoBehaviour
{
    private const float RequiredDisplayFrequency = 90.0f;

    public GameObject infoCanvas;
    public GameObject alertCanvas;
    
    public TMP_Dropdown durationDropdown;
    public TextMeshProUGUI alertMessageText;

    protected float testStartTime;
    protected bool isLogging = false;
    protected string filePath;

    protected virtual void Start()
    {
        infoCanvas.SetActive(false);
        alertCanvas.SetActive(false);

        StartCoroutine(SetRequiredDisplayFrequency());
    }

    private IEnumerator SetRequiredDisplayFrequency()
    {
        while (OVRManager.display == null)
        {
            yield return null;
        }

        OVRDisplay display = OVRManager.display;

        while (!Mathf.Approximately(
            display.displayFrequency,
            RequiredDisplayFrequency))
        {
            display.displayFrequency = RequiredDisplayFrequency;
            yield return null;
        }

        infoCanvas.SetActive(true);
    }

    public virtual void RestartTest()
    {
        isLogging = false;
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
        filePath = Path
            .Combine(Application.persistentDataPath, $"{testName}Data_{timestamp}.csv");
        
        infoCanvas.SetActive(false);
        alertCanvas.SetActive(false);
        testStartTime = Time.time;
        isLogging = true;
    }

    protected void StartTestTimer() => StartCoroutine(TestTimer(GetSelectedDuration()));

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
