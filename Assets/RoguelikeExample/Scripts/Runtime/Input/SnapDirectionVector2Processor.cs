// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.InputSystem;

namespace RoguelikeExample.Input
{
    /// <summary>
    /// Stickなどからの入力を、指定された分割数の方向にスナップするプロセッサ
    /// InputActionのProcessorsに設定して使用します
    ///
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Processors.html"/>
    /// </summary>
    public class SnapDirectionVector2Processor : InputProcessor<Vector2>
    {
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<SnapDirectionVector2Processor>();
        }

        [Tooltip("分割数")]
        public int divisor = 8;

        /// <summary>
        /// Value at which the lower bound deadzone starts.
        /// from <c>StickDeadzoneProcessor</c>
        /// </summary>
        /// <remarks>
        /// Values in the input at or below min will get dropped and values
        /// will be scaled to the range between min and max.
        /// </remarks>
        [Tooltip("この値未満のベクトルは無視する")]
        public float deadzoneMin = 0.125f;

        public override Vector2 Process(Vector2 value, InputControl control)
        {
            if (value.magnitude < deadzoneMin)
            {
                return Vector2.zero;
            }

            var angle = Mathf.Atan2(value.y, value.x);
            var anglePerDivisor = Mathf.PI * 2f / divisor;
            var snappedAngle = Mathf.Round(angle / anglePerDivisor) * anglePerDivisor;
            return new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle));
        }
    }
}
