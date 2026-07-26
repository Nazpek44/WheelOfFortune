using System.Collections;
using UnityEngine;

namespace VertigoDemo.WheelOfFortune.Animation
{
    public interface IWheelSpinner
    {
        bool IsSpinning { get; }

        IEnumerator Spin(
            RectTransform rotator,
            int sliceCount,
            int resultIndex,
            float duration,
            int fullRotations
        );
    }
}