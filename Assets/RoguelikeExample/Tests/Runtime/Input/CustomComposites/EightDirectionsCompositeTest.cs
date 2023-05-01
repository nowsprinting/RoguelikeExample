// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using UnityEngine;

namespace RoguelikeExample.Input.CustomComposites
{
    public class EightDirectionsCompositeTest
    {
        private static readonly Vector2 s_zero = Vector2.zero;
        private static readonly Vector2 s_left = Vector2.left;
        private static readonly Vector2 s_downLeft = new Vector2(-1f, -1f);
        private static readonly Vector2 s_down = Vector2.down;
        private static readonly Vector2 s_downRight = new Vector2(1f, -1f);
        private static readonly Vector2 s_right = Vector2.right;
        private static readonly Vector2 s_upRight = new Vector2(1f, 1f);
        private static readonly Vector2 s_up = Vector2.up;
        private static readonly Vector2 s_upLeft = new Vector2(-1f, 1f);

        private static IEnumerable s_testCases()
        {
            yield return new object[] { false, false, false, false, false, false, false, false, s_zero };
            yield return new object[] { true, false, false, false, false, false, false, false, s_up };
            yield return new object[] { false, true, false, false, false, false, false, false, s_down };
            yield return new object[] { false, false, true, false, false, false, false, false, s_left };
            yield return new object[] { false, false, false, true, false, false, false, false, s_right };
            yield return new object[] { false, false, false, false, true, false, false, false, s_upLeft };
            yield return new object[] { false, false, false, false, false, true, false, false, s_upRight };
            yield return new object[] { false, false, false, false, false, false, true, false, s_downLeft };
            yield return new object[] { false, false, false, false, false, false, false, true, s_downRight };
        }

        [TestCaseSource(nameof(s_testCases))]
        public void CompositeVector(bool up, bool down, bool left, bool right,
            bool upLeft, bool upRight, bool downLeft, bool downRight, Vector2 expected)
        {
            var actual = EightDirectionsComposite.CompositeVector(up, down, left, right,
                upLeft, upRight, downLeft, downRight);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
