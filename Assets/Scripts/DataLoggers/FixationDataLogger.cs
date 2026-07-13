using UnityEngine;
using System.Collections.Generic;
using DataLoggers.CSVWriter;
using DataLoggers.CSVWriter.Definitions;
using DataLoggers.DataPoints;

public class FixationDataLogger : BaseDataLogger
{
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    public GameObject fixationTarget;

    private List<FixationDataPoint> dataBuffer;

    protected override void Start()
    {
        base.Start();
        fixationTarget.SetActive(false);
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
        dataBuffer = new List<FixationDataPoint>(3000); 
        fixationTarget.SetActive(true);
        
        InitializeTest("Fixation");
    }

    private void CaptureData()
    {
        dataBuffer.Add(new FixationDataPoint
        {
            TimeMs = (Time.time - testStartTime) * 1000f,
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
        fixationTarget.SetActive(false);
        WriteBufferToFile();
        ShowCompletionAlert("Fixation");
    }

    private void WriteBufferToFile()
    {
        CsvFile.Write(filePath, dataBuffer, FixationCsvDefinition.Definition);
        dataBuffer.Clear();
    }
}
