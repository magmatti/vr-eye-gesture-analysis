using UnityEngine;
using System;
using System.IO;
using System.Collections;
using TMPro;
using DataLoggers.CSVWriter;
using DataLoggers.CSVWriter.Definitions;
using DataLoggers.DataPoints;
using DataLoggers.TestPhase;
using DataLoggers.TestConfigurations;

public class CombinedGestureDataLogger : MonoBehaviour
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

    public GameObject infoCanvas;
    public GameObject alertCanvas;

    public TextMeshProUGUI alertMessageText;

    public float fixationDuration = 10.0f;
    public float saccadeDuration = 10.0f;
    public float blinkDuration = 15.0f;

    private CsvWriter<CombinedGestureDataPoint> writer;
    private string filePath;

    private bool isLogging = false;
    private float testStartTime;
    private float phaseStartTime;

    private TestPhase currentPhase = TestPhase.None;

    private Coroutine testRoutine;
    private Coroutine saccadeRoutine;
    private Coroutine metronomeRoutine;

    private void Start()
    {
        if (infoCanvas != null) infoCanvas.SetActive(true);
        if (alertCanvas != null) alertCanvas.SetActive(false);
        if (targetPivot != null) targetPivot.gameObject.SetActive(false);

        StartCoroutine(Force90HzRoutine());
    }

    private IEnumerator Force90HzRoutine()
    {
        // wait a little because OVRManager.display can be null at the beginning
        float timeout = 3.0f;
        float elapsed = 0.0f;

        while (elapsed < timeout)
        {
            if (OVRManager.display != null)
            {
                OVRManager.display.displayFrequency = 90.0f;

                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 90;

                Debug.Log($"[Combined Test] Display frequency forced to: {OVRManager.display.displayFrequency} Hz");
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("[Combined Test] OVRManager.display is null. Cannot force 90 Hz.");
    }

    private void Update()
    {
        if (!isLogging) return;

        if (leftEyeGaze == null || rightEyeGaze == null || centerEyeAnchor == null)
        {
            return;
        }

        LogData();
    }

    public void StartTest()
    {
        if (testRoutine != null)
        {
            StopCoroutine(testRoutine);
        }

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        filePath = Path
            .Combine(Application.persistentDataPath, $"CombinedGestureData_{timestamp}.csv");

        writer = new CsvWriter<CombinedGestureDataPoint>(
            new StreamWriter(filePath, false),
            CombinedGestureCsvDefinition.Definition);

        if (infoCanvas != null) infoCanvas.SetActive(false);
        if (alertCanvas != null) alertCanvas.SetActive(false);

        if (targetPivot != null)
        {
            targetPivot.gameObject.SetActive(true);
            targetPivot.localEulerAngles = Vector3.zero;
        }

        testStartTime = Time.time;
        isLogging = true;

        testRoutine = StartCoroutine(CombinedTestSequence());
    }

    public void RestartTest()
    {
        StopAllActiveRoutines();

        isLogging = false;
        currentPhase = TestPhase.None;

        if (writer != null)
        {
            writer.Dispose();
            writer = null;
        }

        if (targetPivot != null)
        {
            targetPivot.localEulerAngles = Vector3.zero;
            targetPivot.gameObject.SetActive(false);
        }

        if (alertCanvas != null) alertCanvas.SetActive(false);
        if (infoCanvas != null) infoCanvas.SetActive(true);
    }

    private IEnumerator CombinedTestSequence()
    {
        // -------------------------
        // 1. Fixation phase - 10 sec
        // -------------------------
        SetPhase(TestPhase.Fixation);

        if (targetPivot != null)
        {
            targetPivot.gameObject.SetActive(true);
            targetPivot.localEulerAngles = Vector3.zero;
        }

        yield return new WaitForSeconds(fixationDuration);

        // -------------------------
        // 2. Saccade phase - 20 sec
        // -------------------------
        SetPhase(TestPhase.Saccade);

        if (targetPivot != null)
        {
            targetPivot.gameObject.SetActive(true);
            targetPivot.localEulerAngles = Vector3.zero;
        }

        saccadeRoutine = StartCoroutine(SaccadeSequence());

        yield return new WaitForSeconds(saccadeDuration);

        if (saccadeRoutine != null)
        {
            StopCoroutine(saccadeRoutine);
            saccadeRoutine = null;
        }

        // -------------------------
        // 3. Blink phase - 15 sec
        // -------------------------
        SetPhase(TestPhase.Blink);

        if (targetPivot != null)
        {
            targetPivot.gameObject.SetActive(true);
            targetPivot.localEulerAngles = Vector3.zero;
        }

        metronomeRoutine = StartCoroutine(MetronomeSequence());

        yield return new WaitForSeconds(blinkDuration);

        EndTest();
    }

    private void SetPhase(TestPhase newPhase)
    {
        currentPhase = newPhase;
        phaseStartTime = Time.time;

        Debug.Log($"[Combined Test] Started phase: {currentPhase}");
    }

    private IEnumerator SaccadeSequence()
    {
        int index = 0;

        while (currentPhase == TestPhase.Saccade && isLogging)
        {
            if (targetPivot != null)
            {
                targetPivot.localEulerAngles = SaccadeJumpSequence.Angles[index];
            }

            index = (index + 1) % SaccadeJumpSequence.Angles.Count;

            yield return new WaitForSeconds(saccadeJumpInterval);
        }
    }

    private IEnumerator MetronomeSequence()
    {
        yield return new WaitForSeconds(blinkInitialDelay);

        while (currentPhase == TestPhase.Blink && isLogging)
        {
            if (metronomeAudio != null && metronomeAudio.clip != null)
            {
                metronomeAudio.Play();
            }

            yield return new WaitForSeconds(beepInterval);
        }
    }

    private void LogData()
    {
        if (writer == null) return;

        float totalTimeMs = (Time.time - testStartTime) * 1000.0f;
        float phaseTimeMs = (Time.time - phaseStartTime) * 1000.0f;

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
            Phase = currentPhase,
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
        if (angle > 180.0f)
        {
            angle -= 360.0f;
        }

        return angle;
    }

    private void EndTest()
    {
        isLogging = false;
        currentPhase = TestPhase.None;

        StopAllActiveRoutines();

        if (writer != null)
        {
            writer.Dispose();
            writer = null;
        }

        if (targetPivot != null)
        {
            targetPivot.localEulerAngles = Vector3.zero;
            targetPivot.gameObject.SetActive(false);
        }

        if (alertCanvas != null) alertCanvas.SetActive(true);

        if (alertMessageText != null)
        {
            alertMessageText.text = $"Combined Gesture Test Complete!\nData saved to:\n{filePath}\n";
        }

        Debug.Log($"[Combined Test] Data saved to: {filePath}");
    }

    private void StopAllActiveRoutines()
    {
        if (saccadeRoutine != null)
        {
            StopCoroutine(saccadeRoutine);
            saccadeRoutine = null;
        }

        if (metronomeRoutine != null)
        {
            StopCoroutine(metronomeRoutine);
            metronomeRoutine = null;
        }
    }

    private void OnDestroy()
    {
        if (writer != null)
        {
            writer.Dispose();
            writer = null;
        }
    }
}
