using UnityEngine;

namespace DataLoggers.TestConfigurations
{
    internal static class SaccadeJumpSequence
    {
        private static readonly Vector3[] PeripheralAngles =
        {
            new Vector3(0, 15, 0),
            new Vector3(0, -15, 0),
            new Vector3(-10, 0, 0),
            new Vector3(10, 0, 0)
        };

        internal static Vector3[] CreateRandomizedPeripheralAngles()
        {
            Vector3[] angles = (Vector3[])PeripheralAngles.Clone();

            for (int index = angles.Length - 1; index > 0; index--)
            {
                int randomIndex = Random.Range(0, index + 1);
                (angles[index], angles[randomIndex]) =
                    (angles[randomIndex], angles[index]);
            }

            return angles;
        }
    }
}
