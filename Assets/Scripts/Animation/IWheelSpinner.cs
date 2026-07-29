using System.Collections;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Animation
{
    public interface IWheelSpinner
    {
        bool IsSpinning { get; }

        IEnumerator SpinCoroutine(
            RectTransform rotator,
            int sliceCount,
            int resultIndex,
            float duration,
            int fullRotations
        );
    }
}
