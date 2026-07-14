using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataLoggers.CSVWriter;
using DataLoggers.CSVWriter.Definitions;
using DataLoggers.DataPoints;
using DataLoggers.TestConfigurations;

public class SaccadeDataLogger : BaseDataLogger
{
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    public Transform targetPivot;
    public float jumpInterval = 2.0f;
    
    private Coroutine saccadeRoutine;

    private List<SaccadeDataPoint> dataBuffer;

    protected override void Start()
    {
        base.Start();
        targetPivot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isLogging && leftEyeGaze != null && rightEyeGaze != null)
        {
            CaptureData();
        }
    }

    public void StartTest()
    {
        dataBuffer = new List<SaccadeDataPoint>(3000); 
        targetPivot.gameObject.SetActive(true);
        targetPivot.localEulerAngles = Vector3.zero;
        
        InitializeTest("Saccade");
        StartTestTimer();
        saccadeRoutine = StartCoroutine(SaccadeSequence());
    }

    private IEnumerator SaccadeSequence()
    {
        int index = 0;
        while (isLogging)
        {
            targetPivot.localEulerAngles = SaccadeJumpSequence.Angles[index];
            index = (index + 1) % SaccadeJumpSequence.Angles.Count;
            yield return new WaitForSeconds(jumpInterval);
        }
    }

    private void CaptureData()
    {
        dataBuffer.Add(new SaccadeDataPoint
        {
            TimeMs = (Time.time - testStartTime) * 1000f,
            TargetX = targetPivot.localEulerAngles.x,
            TargetY = targetPivot.localEulerAngles.y,
            HRot = centerEyeAnchor.rotation,
            LLocRot = leftEyeGaze.transform.localRotation,
            RLocRot = rightEyeGaze.transform.localRotation,
            LRot = leftEyeGaze.transform.rotation,
            RRot = rightEyeGaze.transform.rotation,
            LConf = leftEyeGaze.Confidence,
            RConf = rightEyeGaze.Confidence
        });
    }

    protected override void EndTest()
    {
        isLogging = false;
        if (saccadeRoutine != null) StopCoroutine(saccadeRoutine);
        targetPivot.gameObject.SetActive(false);

        WriteBufferToFile();
        ShowCompletionAlert("Saccade");
    }

    private void WriteBufferToFile()
    {
        CsvFile.Write(filePath, dataBuffer, SaccadeCsvDefinition.Definition);
        dataBuffer.Clear();
    }
}
