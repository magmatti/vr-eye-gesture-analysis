using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public class SmoothPursuitDataLogger : BaseDataLogger
{
    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    [Header("Pursuit Target Settings")]
    public Transform targetPivot;
    public float maxAngle = 20f; 
    public float movementSpeed = 2f; 

    private struct PursuitDataPoint
    {
        public float TimeMs, TargetY;
        public Quaternion HRot, LLocRot, RLocRot, LRot, RRot;
        public float LConf, RConf;
    }

    private List<PursuitDataPoint> dataBuffer;

    protected override void Start()
    {
        base.Start();
        targetPivot.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isLogging && leftEyeGaze != null && rightEyeGaze != null)
        {
            float timeActive = Time.time - testStartTime;
            float currentAngle = Mathf.Sin(timeActive * movementSpeed) * maxAngle;
            targetPivot.localEulerAngles = new Vector3(0, currentAngle, 0);

            CaptureData(currentAngle);
        }
    }

    public void StartTest()
    {
        dataBuffer = new List<PursuitDataPoint>(3000);
        targetPivot.gameObject.SetActive(true);
        targetPivot.localEulerAngles = Vector3.zero;
        
        InitializeTest("SmoothPursuit");
    }

    private void CaptureData(float currentTargetY)
    {
        dataBuffer.Add(new PursuitDataPoint
        {
            TimeMs = (Time.time - testStartTime) * 1000f,
            TargetY = currentTargetY,
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
        targetPivot.gameObject.SetActive(false);
        WriteBufferToFile();
        ShowCompletionAlert("Smooth Pursuit");
    }

    private void WriteBufferToFile()
    {
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Time_ms,TargetRotY,HeadRotX,HeadRotY,HeadRotZ," +
                             "HeadRotW,LeftLocalRotX,LeftLocalRotY,LeftLocalRotZ," +
                             "LeftLocalRotW,LeftWorldRotX,LeftWorldRotY," +
                             "LeftWorldRotZ,LeftWorldRotW,LeftConfidence," +
                             "RightLocalRotX,RightLocalRotY,RightLocalRotZ," +
                             "RightLocalRotW,RightWorldRotX,RightWorldRotY," +
                             "RightWorldRotZ,RightWorldRotW,RightConfidence");

            StringBuilder sb = new StringBuilder(512);
            foreach (var p in dataBuffer)
            {
                sb.Clear();
                sb.Append(p.TimeMs.ToString("F0", CultureInfo.InvariantCulture)).Append(",")
                  .Append(p.TargetY.ToString("F5", CultureInfo.InvariantCulture)).Append(",")
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
