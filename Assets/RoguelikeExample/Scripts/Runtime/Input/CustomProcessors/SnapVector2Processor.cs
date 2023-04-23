// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoguelikeExample.Input.CustomProcessors
{
    /// <summary>
    /// Stickなどからの入力を、指定された分割数の方向にスナップするカスタムプロセッサ
    /// InputActionのProcessorsに設定して使用します
    /// </summary>
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Processors.html"/>
    public class SnapVector2Processor : InputProcessor<Vector2>
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Initialize()
        {
            InputSystem.RegisterProcessor<SnapVector2Processor>();
        }

        /// <summary>
        /// 分割数（スナップする方向の数）
        /// </summary>
        [Tooltip("分割数（スナップする方向の数）")]
        public int divisor = 8;

        /// <summary>
        /// 最小値（この値未満のベクトルは無視します）
        /// </summary>
        /// <seealso cref="StickDeadzoneProcessor"/>
        [Tooltip("最小値（この値未満のベクトルは無視します）")]
        public float min = 0.125f;

        /// <summary>
        /// 入力値を <see cref="divisor"/> で指定された数の方向にスナップします
        /// </summary>
        /// <param name="value">Input value.</param>
        /// <param name="control">Ignored.</param>
        /// <returns></returns>
        public override Vector2 Process(Vector2 value, InputControl control)
        {
            if (value.magnitude < min)
            {
                return Vector2.zero;
            }

            var angle = Mathf.Atan2(value.y, value.x);
            var anglePerDivisor = Mathf.PI * 2f / divisor;
            var snappedAngle = Mathf.Round(angle / anglePerDivisor) * anglePerDivisor;
            return new Vector2(Mathf.Cos(snappedAngle), Mathf.Sin(snappedAngle));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"SnapVector2(divisor={divisor},min={min})";
        }
    }
}
