using UnityEngine;
using System;
using System.Collections;

namespace DataLoggers.TestConfigurations
{
    internal static class MetronomeSequence
    {
        internal static IEnumerator Run(
            AudioSource audioSource,
            float initialDelay,
            float beepInterval,
            Func<bool> shouldContinue)
        {
            yield return new WaitForSeconds(initialDelay);

            while (shouldContinue())
            {
                if (audioSource != null && audioSource.clip != null)
                {
                    audioSource.Play();
                }

                yield return new WaitForSeconds(beepInterval);
            }
        }
    }
}
