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
            Func<bool> shouldContinue)
        {
            int index = 0;

            while (shouldContinue())
            {
                targetPivot.localEulerAngles = SaccadeJumpSequence.Angles[index];
                index = (index + 1) % SaccadeJumpSequence.Angles.Count;

                yield return new WaitForSeconds(jumpInterval);
            }
        }
    }
}
