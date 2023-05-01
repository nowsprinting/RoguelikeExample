// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.ComponentModel;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem.Editor;
#endif

namespace RoguelikeExample.Input.CustomComposites
{
    /// <summary>
    /// 8方向のキー入力を扱うカスタムコンポジット
    /// </summary>
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/ActionBindings.html"/>
    [DisplayStringFormat("{left}/{down}/{up}/{right}/{upLeft}/{upRight}/{downLeft}/{downRight}")]
    [DisplayName("Up/Down/Left/Right/UpLeft/UpRight/DownLeft/DownRight Composite")]
    public class EightDirectionsComposite : InputBindingComposite<Vector2>
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Initialize()
        {
            InputSystem.RegisterBindingComposite<EightDirectionsComposite>();
        }

        [InputControl(layout = "Axis")]
        public int up = 0;

        [InputControl(layout = "Axis")]
        public int down = 0;

        [InputControl(layout = "Axis")]
        public int left = 0;

        [InputControl(layout = "Axis")]
        public int right = 0;

        [InputControl(layout = "Axis")]
        public int upLeft = 0;

        [InputControl(layout = "Axis")]
        public int upRight = 0;

        [InputControl(layout = "Axis")]
        public int downLeft = 0;

        [InputControl(layout = "Axis")]
        public int downRight = 0;

        public bool normalize;

        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            var compositeVector = CompositeVector(
                context.ReadValueAsButton(up),
                context.ReadValueAsButton(down),
                context.ReadValueAsButton(left),
                context.ReadValueAsButton(right),
                context.ReadValueAsButton(upLeft),
                context.ReadValueAsButton(upRight),
                context.ReadValueAsButton(downLeft),
                context.ReadValueAsButton(downRight)
            );
            return normalize ? compositeVector.normalized : compositeVector;
        }

        internal static Vector2 CompositeVector(bool up, bool down, bool left, bool right,
            bool upLeft, bool upRight, bool downLeft, bool downRight)
        {
            var upValue = (up || upRight || upLeft) ? 1.0f : 0.0f;
            var downValue = (down || downLeft || downRight) ? 1.0f : 0.0f;
            var leftValue = (left || upLeft || downLeft) ? 1.0f : 0.0f;
            var rightValue = (right || upRight || downRight) ? 1.0f : 0.0f;

            return new Vector2(rightValue - leftValue, upValue - downValue);
        }

        /// <inheritdoc />
        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            var value = ReadValue(ref context);
            return value.magnitude;
        }
    }

#if UNITY_EDITOR
    public class EightDirectionsCompositeEditor : InputParameterEditor<EightDirectionsComposite>
    {
        public override void OnGUI()
        {
            target.normalize = EditorGUILayout.Toggle(_normalizeLabel, target.normalize);
        }

        private readonly GUIContent _normalizeLabel = new GUIContent("Normalize Vector");
    }
#endif
}
