using UnityEngine;
using System.Collections;
using GestureTestPhase = DataLoggers.TestPhase.TestPhase;

namespace DataLoggers.TestConfigurations
{
    internal sealed class CombinedTestSequence
    {
        private readonly MonoBehaviour coroutineOwner;
        private readonly Transform targetPivot;
        private readonly AudioSource metronomeAudio;

        private readonly float fixationDuration;
        private readonly float saccadeDuration;
        private readonly float blinkDuration;
        private readonly float saccadeJumpInterval;
        private readonly float blinkInitialDelay;
        private readonly float beepInterval;

        private Coroutine saccadeRoutine;
        private Coroutine metronomeRoutine;

        internal GestureTestPhase CurrentPhase { get; private set; } = GestureTestPhase.None;
        internal float PhaseStartTime { get; private set; }

        internal CombinedTestSequence(
            MonoBehaviour coroutineOwner,
            Transform targetPivot,
            AudioSource metronomeAudio,
            float fixationDuration,
            float saccadeDuration,
            float blinkDuration,
            float saccadeJumpInterval,
            float blinkInitialDelay,
            float beepInterval)
        {
            this.coroutineOwner = coroutineOwner;
            this.targetPivot = targetPivot;
            this.metronomeAudio = metronomeAudio;
            this.fixationDuration = fixationDuration;
            this.saccadeDuration = saccadeDuration;
            this.blinkDuration = blinkDuration;
            this.saccadeJumpInterval = saccadeJumpInterval;
            this.blinkInitialDelay = blinkInitialDelay;
            this.beepInterval = beepInterval;
        }

        internal IEnumerator Run()
        {
            yield return FixationPhase();
            yield return SaccadePhase();
            yield return BlinkPhase();
        }

        internal void Stop()
        {
            StopSaccadeSequence();
            StopMetronomeSequence();
            CurrentPhase = GestureTestPhase.None;
        }

        private IEnumerator FixationPhase()
        {
            StartPhase(GestureTestPhase.Fixation);
            yield return new WaitForSeconds(fixationDuration);
        }

        private IEnumerator SaccadePhase()
        {
            StartPhase(GestureTestPhase.Saccade);

            saccadeRoutine = coroutineOwner.StartCoroutine(
                SaccadeSequence.Run(
                    targetPivot,
                    saccadeJumpInterval,
                    () => CurrentPhase == GestureTestPhase.Saccade));

            yield return new WaitForSeconds(saccadeDuration);

            StopSaccadeSequence();
        }

        private IEnumerator BlinkPhase()
        {
            StartPhase(GestureTestPhase.Blink);

            metronomeRoutine = coroutineOwner.StartCoroutine(
                MetronomeSequence.Run(
                    metronomeAudio,
                    blinkInitialDelay,
                    beepInterval,
                    () => CurrentPhase == GestureTestPhase.Blink));

            yield return new WaitForSeconds(blinkDuration);

            StopMetronomeSequence();
        }

        private void StartPhase(GestureTestPhase phase)
        {
            CurrentPhase = phase;
            PhaseStartTime = Time.time;

            targetPivot.gameObject.SetActive(true);
            targetPivot.localEulerAngles = Vector3.zero;
        }

        private void StopSaccadeSequence()
        {
            if (saccadeRoutine == null) return;

            coroutineOwner.StopCoroutine(saccadeRoutine);
            saccadeRoutine = null;
        }

        private void StopMetronomeSequence()
        {
            if (metronomeRoutine == null) return;

            coroutineOwner.StopCoroutine(metronomeRoutine);
            metronomeRoutine = null;
        }
    }
}
