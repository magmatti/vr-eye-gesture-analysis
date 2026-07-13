using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
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
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Time_ms,TargetRotX,TargetRotY,HeadRotX,HeadRotY," +
                             "HeadRotZ,HeadRotW,LeftLocalRotX,LeftLocalRotY," +
                             "LeftLocalRotZ,LeftLocalRotW,LeftWorldRotX," +
                             "LeftWorldRotY,LeftWorldRotZ,LeftWorldRotW," +
                             "LeftConfidence,RightLocalRotX,RightLocalRotY," +
                             "RightLocalRotZ,RightLocalRotW,RightWorldRotX," +
                             "RightWorldRotY,RightWorldRotZ,RightWorldRotW,RightConfidence");

            StringBuilder sb = new StringBuilder(512); 
            foreach (var p in dataBuffer)
            {
                sb.Clear();
                sb.Append(p.TimeMs.ToString("F0", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.TargetX.ToString("F2", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.TargetY.ToString("F2", CultureInfo.InvariantCulture)).Append(",")
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
