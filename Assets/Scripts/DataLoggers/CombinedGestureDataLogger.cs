using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DataLoggers.CSVWriter;
using DataLoggers.CSVWriter.Definitions;
using DataLoggers.DataPoints;
using DataLoggers.TestConfigurations;

public class CombinedGestureDataLogger : BaseDataLogger
{
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    public OVRFaceExpressions faceExpressions;

    public Transform targetPivot;
    public float saccadeJumpInterval = 1.75f;
    public float saccadeMoveDuration = 0.05f;

    public AudioSource metronomeAudio;
    public float blinkInitialDelay = 3.0f;
    public float beepInterval = 2.0f;

    public float fixationDuration = 10.0f;
    public float saccadeDuration = 10.0f;
    public float blinkDuration = 15.0f;

    private List<CombinedGestureDataPoint> dataBuffer;
    private CombinedTestSequence testSequence;

    private Coroutine testRoutine;

    protected override void Start()
    {
        base.Start();

        if (targetPivot != null) targetPivot.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isLogging) return;

        CaptureData();
    }

    public void StartTest()
    {
        StopTestExecution();

        dataBuffer = new List<CombinedGestureDataPoint>(5000);

        InitializeTest("CombinedGesture");

        testSequence = new CombinedTestSequence(
            this,
            targetPivot,
            metronomeAudio,
            fixationDuration,
            saccadeDuration,
            blinkDuration,
            saccadeJumpInterval,
            saccadeMoveDuration,
            blinkInitialDelay,
            beepInterval);

        testRoutine = StartCoroutine(RunCombinedTest());
    }

    private IEnumerator RunCombinedTest()
    {
        yield return testSequence.Run();
        EndTest();
    }

    public override void RestartTest()
    {
        StopTestExecution();
        dataBuffer.Clear();

        if (targetPivot != null)
        {
            targetPivot.localEulerAngles = Vector3.zero;
            targetPivot.gameObject.SetActive(false);
        }

        base.RestartTest();
    }

    private void CaptureData()
    {
        Vector3 targetAngles = targetPivot != null
            ? targetPivot.localEulerAngles
            : Vector3.zero;

        bool faceTrackingEnabled = faceExpressions != null
            && faceExpressions.FaceTrackingEnabled;

        dataBuffer.Add(new CombinedGestureDataPoint
        {
            TimeMs = (Time.time - testStartTime) * 1000.0f,
            Phase = testSequence.CurrentPhase,
            PhaseTimeMs = (Time.time - testSequence.PhaseStartTime) * 1000.0f,
            TargetX = NormalizeEulerAngle(targetAngles.x),
            TargetY = NormalizeEulerAngle(targetAngles.y),
            HRot = centerEyeAnchor.rotation,
            LLocRot = leftEyeGaze.transform.localRotation,
            LRot = leftEyeGaze.transform.rotation,
            RLocRot = rightEyeGaze.transform.localRotation,
            RRot = rightEyeGaze.transform.rotation,
            LeftBlinkWeight = faceTrackingEnabled
                ? faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL)
                : 0.0f,
            RightBlinkWeight = faceTrackingEnabled
                ? faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR)
                : 0.0f,
            LConf = leftEyeGaze.Confidence,
            RConf = rightEyeGaze.Confidence
        });
    }

    private float NormalizeEulerAngle(float angle)
    {
        if (angle > 180.0f) angle -= 360.0f;

        return angle;
    }

    protected override void EndTest()
    {
        isLogging = false;
        testRoutine = null;

        StopTestSequence();

        if (targetPivot != null)
        {
            targetPivot.localEulerAngles = Vector3.zero;
            targetPivot.gameObject.SetActive(false);
        }

        WriteBufferToFile();
        ShowCompletionAlert("Combined Gesture");
    }

    private void StopTestExecution()
    {
        if (testRoutine != null)
        {
            StopCoroutine(testRoutine);
            testRoutine = null;
        }

        StopTestSequence();
    }

    private void StopTestSequence()
    {
        if (testSequence == null) return;

        testSequence.Stop();
        testSequence = null;
    }

    private void WriteBufferToFile()
    {
        CsvFile.Write(
            filePath,
            dataBuffer,
            CombinedGestureCsvDefinition.Definition);

        dataBuffer.Clear();
    }
}
