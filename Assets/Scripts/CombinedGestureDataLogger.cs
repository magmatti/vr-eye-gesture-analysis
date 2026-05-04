using UnityEngine;
using System;
using System.IO;
using System.Collections;
using TMPro;
using System.Globalization;

public class CombinedGestureDataLogger : MonoBehaviour
{

    private enum TestPhase
    {
        None,
        Fixation,
        Saccade,
        Blink
    }

    [Header("Eye Tracking Components")]
    public OVREyeGaze leftEyeGaze;
    public OVREyeGaze rightEyeGaze;
    public Transform centerEyeAnchor;

    [Header("Blink Tracking Component")]
    public OVRFaceExpressions faceExpressions;

    [Header("Target Settings")]
    public Transform targetPivot;

    [Header("Saccade Settings")]
    public float saccadeJumpInterval = 2.0f;

    private readonly Vector3[] saccadeJumpAngles = new Vector3[]
    {
        Vector3.zero,
        new Vector3(0, 15, 0),
        Vector3.zero,
        new Vector3(0, -15, 0),
        Vector3.zero,
        new Vector3(-10, 0, 0),
        Vector3.zero,
        new Vector3(10, 0, 0)
    };

    [Header("Blink Beep Settings")]
    public AudioSource metronomeAudio;
    public float blinkInitialDelay = 3.0f;
    public float beepInterval = 2.0f;

    [Header("UI Panels")]
    public GameObject infoCanvas;
    public GameObject alertCanvas;

    [Header("UI Controls")]
    public TextMeshProUGUI alertMessageText;

    [Header("Fixed Test Durations")]
    public float fixationDuration = 10.0f;
    public float saccadeDuration = 10.0f;
    public float blinkDuration = 15.0f;

    private StreamWriter writer;
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
        // wait a little because OVRManager.display can be null at the very beginning.
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
        filePath = Path.Combine(Application.persistentDataPath, $"CombinedGestureData_{timestamp}.csv");

        writer = new StreamWriter(filePath, false);

        writer.WriteLine(
            "Time_ms,Phase,PhaseTime_ms," +
            "TargetRotX,TargetRotY," +
            "HeadRotX,HeadRotY,HeadRotZ,HeadRotW," +
            "LeftLocalRotX,LeftLocalRotY,LeftLocalRotZ,LeftLocalRotW," +
            "LeftWorldRotX,LeftWorldRotY,LeftWorldRotZ,LeftWorldRotW," +
            "RightLocalRotX,RightLocalRotY,RightLocalRotZ,RightLocalRotW," +
            "RightWorldRotX,RightWorldRotY,RightWorldRotZ,RightWorldRotW," +
            "LeftBlinkWeight,RightBlinkWeight," +
            "LeftConfidence,RightConfidence"
        );

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
            writer.Close();
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
                targetPivot.localEulerAngles = saccadeJumpAngles[index];
            }

            index = (index + 1) % saccadeJumpAngles.Length;

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

        float leftConfidence = leftEyeGaze != null ? leftEyeGaze.Confidence : 0.0f;
        float rightConfidence = rightEyeGaze != null ? rightEyeGaze.Confidence : 0.0f;

        string line =
            $"{totalTimeMs.ToString("F0", CultureInfo.InvariantCulture)}," +
            $"{currentPhase}," +
            $"{phaseTimeMs.ToString("F0", CultureInfo.InvariantCulture)}," +

            $"{targetX.ToString("F2", CultureInfo.InvariantCulture)}," +
            $"{targetY.ToString("F2", CultureInfo.InvariantCulture)}," +

            $"{hRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{hRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{hRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{hRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +

            $"{lLocalRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{lLocalRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{lLocalRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{lLocalRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +

            $"{lWorldRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{lWorldRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{lWorldRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{lWorldRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +

            $"{rLocalRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rLocalRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rLocalRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rLocalRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +

            $"{rWorldRot.x.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rWorldRot.y.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rWorldRot.z.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rWorldRot.w.ToString("F5", CultureInfo.InvariantCulture)}," +

            $"{leftBlink.ToString("F5", CultureInfo.InvariantCulture)}," +
            $"{rightBlink.ToString("F5", CultureInfo.InvariantCulture)}," +

            $"{leftConfidence.ToString("F2", CultureInfo.InvariantCulture)}," +
            $"{rightConfidence.ToString("F2", CultureInfo.InvariantCulture)}";

        writer.WriteLine(line);
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
            writer.Close();
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
            writer.Close();
            writer = null;
        }
    }
}
