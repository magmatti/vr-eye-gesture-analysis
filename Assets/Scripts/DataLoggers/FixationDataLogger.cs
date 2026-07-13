using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Time_ms,HeadRotX,HeadRotY,HeadRotZ,HeadRotW," +
                             "LeftLocalRotX,LeftLocalRotY,LeftLocalRotZ,LeftLocalRotW," +
                             "LeftWorldRotX,LeftWorldRotY,LeftWorldRotZ,LeftWorldRotW," +
                             "LeftConfidence,RightLocalRotX,RightLocalRotY," +
                             "RightLocalRotZ,RightLocalRotW,RightWorldRotX," +
                             "RightWorldRotY,RightWorldRotZ,RightWorldRotW,RightConfidence");

            StringBuilder sb = new StringBuilder(512);
            foreach (var p in dataBuffer)
            {
                sb.Clear();
                sb.Append(p.TimeMs.ToString("F0", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.HRot.x.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.HRot.y.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.HRot.z.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.HRot.w.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LLocRot.x.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LLocRot.y.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LLocRot.z.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LLocRot.w.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LRot.x.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LRot.y.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LRot.z.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LRot.w.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.LConf.ToString("F2", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RLocRot.x.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RLocRot.y.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RLocRot.z.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RLocRot.w.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RRot.x.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RRot.y.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RRot.z.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RRot.w.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.RConf.ToString("F2", CultureInfo.InvariantCulture));
                writer.WriteLine(sb.ToString());
            }
        }
        dataBuffer.Clear();
    }
}
