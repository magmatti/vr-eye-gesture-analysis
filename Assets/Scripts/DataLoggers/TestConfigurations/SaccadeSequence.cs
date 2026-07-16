using UnityEngine;
using System;
using System.Collections;

namespace DataLoggers.TestConfigurations
{
    internal static class SaccadeSequence
    {
        internal static IEnumerator Run(
            Transform targetPivot,
            float jumpInterval,
            float moveDuration,
            Func<bool> shouldContinue)
        {
            int index = 0;

            while (shouldContinue())
            {
                Quaternion startRotation = targetPivot.localRotation;
                Quaternion targetRotation =
                    Quaternion.Euler(SaccadeJumpSequence.Angles[index]);
                float elapsedTime = 0.0f;

                while (elapsedTime < moveDuration && shouldContinue())
                {
                    elapsedTime += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsedTime / moveDuration);
                    targetPivot.localRotation =
                        Quaternion.Slerp(startRotation, targetRotation, progress);

                    yield return null;
                }

                if (!shouldContinue()) yield break;

                targetPivot.localRotation = targetRotation;
                index = (index + 1) % SaccadeJumpSequence.Angles.Count;

                float dwellDuration = Mathf.Max(0.0f, jumpInterval - moveDuration);
                yield return new WaitForSeconds(dwellDuration);
            }
        }
    }
}
