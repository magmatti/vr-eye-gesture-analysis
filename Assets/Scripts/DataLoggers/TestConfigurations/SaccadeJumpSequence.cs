using System.Collections.ObjectModel;
using UnityEngine;

namespace DataLoggers.TestConfigurations
{
    internal static class SaccadeJumpSequence
    {
        internal static ReadOnlyCollection<Vector3> Angles { get; } =
            new ReadOnlyCollection<Vector3>(new[]
            {
                Vector3.zero,
                new Vector3(0, 15, 0),
                Vector3.zero,
                new Vector3(0, -15, 0),
                Vector3.zero,
                new Vector3(-10, 0, 0),
                Vector3.zero,
                new Vector3(10, 0, 0)
            });
    }
}
