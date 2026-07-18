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
            Vector3[] peripheralAngles =
                SaccadeJumpSequence.CreateRandomizedPeripheralAngles();
            int peripheralIndex = 0;
            bool moveToPeripheral = false;

            while (shouldContinue())
            {
                Quaternion startRotation = targetPivot.localRotation;

                // alternate between the center and the next randomized peripheral direction
                Vector3 targetAngle = Vector3.zero;

                if (moveToPeripheral)
                {
                    targetAngle = peripheralAngles[peripheralIndex];
                    peripheralIndex++;

                    // reshuffle after every direction has been used once
                    if (peripheralIndex == peripheralAngles.Length)
                    {
                        peripheralAngles =
                            SaccadeJumpSequence.CreateRandomizedPeripheralAngles();
                        peripheralIndex = 0;
                    }
                }

                // prepare the destination rotation and reset the interpolation timer
                Quaternion targetRotation = Quaternion.Euler(targetAngle);
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
                moveToPeripheral = !moveToPeripheral;

                float dwellDuration = Mathf.Max(0.0f, jumpInterval - moveDuration);
                yield return new WaitForSeconds(dwellDuration);
            }
        }
    }
}
