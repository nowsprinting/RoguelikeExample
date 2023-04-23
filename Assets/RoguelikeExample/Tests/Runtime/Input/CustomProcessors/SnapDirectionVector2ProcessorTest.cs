// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;

namespace RoguelikeExample.Input.CustomProcessors
{
    public class SnapDirectionVector2ProcessorTest
    {
        private static readonly Vector2 s_left = Vector2.left;
        private static readonly Vector2 s_downLeft = new Vector2(-1f, -1f).normalized;
        private static readonly Vector2 s_down = Vector2.down;
        private static readonly Vector2 s_downRight = new Vector2(1f, -1f).normalized;
        private static readonly Vector2 s_right = Vector2.right;
        private static readonly Vector2 s_upRight = new Vector2(1f, 1f).normalized;
        private static readonly Vector2 s_up = Vector2.up;
        private static readonly Vector2 s_upLeft = new Vector2(-1f, 1f).normalized;

        private static IEnumerable s_testCases()
        {
            yield return new object[] { s_left, s_left };
            yield return new object[] { s_downLeft, s_downLeft };
            yield return new object[] { s_down, s_down };
            yield return new object[] { s_downRight, s_downRight };
            yield return new object[] { s_right, s_right };
            yield return new object[] { s_upRight, s_upRight };
            yield return new object[] { s_up, s_up };
            yield return new object[] { s_upLeft, s_upLeft };

            // Around left
            yield return new object[] { new Vector2(-0.9f, 0.3f), s_left };
            yield return new object[] { new Vector2(-0.9f, -0.3f), s_left };

            // Around down-left
            yield return new object[] { new Vector2(-0.8f, -0.4f), s_downLeft };
            yield return new object[] { new Vector2(-0.4f, -0.8f), s_downLeft };

            // Around up-left
            yield return new object[] { new Vector2(-0.8f, 0.4f), s_upLeft };
            yield return new object[] { new Vector2(-0.4f, 0.8f), s_upLeft };
        }

        [TestCaseSource(nameof(s_testCases))]
        public void Process_8divisor_Snapped8directions(Vector2 src, Vector2 snapped)
        {
            var sut = new SnapVector2Processor { divisor = 8 };
            var actual = sut.Process(src, null);
            Assert.That(actual, Is.EqualTo(snapped).Using(Vector2EqualityComparer.Instance));
        }

        [Test]
        public void Process_underMinVector_returnZero()
        {
            var sut = new SnapVector2Processor { min = 0.125f };
            var actual = sut.Process(new Vector2(0.05f, 0.05f), null);
            Assert.That(actual, Is.EqualTo(Vector2.zero));
        }
    }
}
