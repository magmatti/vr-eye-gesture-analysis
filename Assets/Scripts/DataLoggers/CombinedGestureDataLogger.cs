using UnityEngine;
using System.IO;
using System.Collections;
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
    public float saccadeJumpInterval = 2.0f;

    public AudioSource metronomeAudio;
    public float blinkInitialDelay = 3.0f;
    public float beepInterval = 2.0f;

    public float fixationDuration = 10.0f;
    public float saccadeDuration = 10.0f;
    public float blinkDuration = 15.0f;

    private CsvWriter<CombinedGestureDataPoint> writer;
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

        LogData();
    }

    private void OnDestroy() => DisposeWriter();

    public void StartTest()
    {
        StopTestExecution();
        DisposeWriter();

        InitializeTest("CombinedGesture");

        writer = new CsvWriter<CombinedGestureDataPoint>(
            new StreamWriter(filePath, false),
            CombinedGestureCsvDefinition.Definition);

        testSequence = new CombinedTestSequence(
            this,
            targetPivot,
            metronomeAudio,
            fixationDuration,
            saccadeDuration,
            blinkDuration,
            saccadeJumpInterval,
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
        DisposeWriter();

        if (targetPivot != null)
        {
            targetPivot.localEulerAngles = Vector3.zero;
            targetPivot.gameObject.SetActive(false);
        }

        base.RestartTest();
    }

    private void LogData()
    {
        if (writer == null) return;

        float totalTimeMs = (Time.time - testStartTime) * 1000.0f;
        float phaseTimeMs = (Time.time - testSequence.PhaseStartTime) * 1000.0f;

        float targetX = 0.0f;
        float targetY = 0.0f;

        if (targetPivot != null)
        {
            Vector3 targetAngles = targetPivot.localEulerAngles;

            targetX = NormalizeEulerAngle(targetAngles.x);
            targetY = NormalizeEulerAngle(targetAngles.y);
        }

        Quaternion hRot = centerEyeAnchor.rotation;

        Quaternion lLocalRot = leftEyeGaze.transform.localRotation;
        Quaternion rLocalRot = rightEyeGaze.transform.localRotation;

        Quaternion lWorldRot = leftEyeGaze.transform.rotation;
        Quaternion rWorldRot = rightEyeGaze.transform.rotation;

        float leftBlink = 0.0f;
        float rightBlink = 0.0f;

        if (faceExpressions != null && faceExpressions.FaceTrackingEnabled)
        {
            leftBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedL);
            rightBlink = faceExpressions.GetWeight(OVRFaceExpressions.FaceExpression.EyesClosedR);
        }

        float leftConfidence = leftEyeGaze.Confidence;
        float rightConfidence = rightEyeGaze.Confidence;

        writer.WriteRecord(new CombinedGestureDataPoint
        {
            TimeMs = totalTimeMs,
            Phase = testSequence.CurrentPhase,
            PhaseTimeMs = phaseTimeMs,
            TargetX = targetX,
            TargetY = targetY,
            HRot = hRot,
            LLocRot = lLocalRot,
            LRot = lWorldRot,
            RLocRot = rLocalRot,
            RRot = rWorldRot,
            LeftBlinkWeight = leftBlink,
            RightBlinkWeight = rightBlink,
            LConf = leftConfidence,
            RConf = rightConfidence
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
        DisposeWriter();

        if (targetPivot != null)
        {
            targetPivot.localEulerAngles = Vector3.zero;
            targetPivot.gameObject.SetActive(false);
        }

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

    private void DisposeWriter()
    {
        if (writer != null)
        {
            writer.Dispose();
            writer = null;
        }
    }
}
