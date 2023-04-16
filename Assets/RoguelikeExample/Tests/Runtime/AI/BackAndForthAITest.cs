// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Entities.ScriptableObjects;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.AI
{
    /// <summary>
    /// 敵AIの振る舞いをテスト
    ///
    /// ロジックのユニットテストは割愛し、<c>EnemyCharacterController</c>の振る舞いを隔離されたサンドボックスで検証する。
    /// 現状は毎ターンの移動座標を検証しているが、数ターン放置した後の座標やヒットポイントの減少での検証に切り替えたい。
    /// </summary>
    [TestFixture]
    public class BackAndForthAITest
    {
        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(BackAndForthAITest));
            SceneManager.SetActiveScene(scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.UnloadSceneAsync(nameof(BackAndForthAITest));
        }

        [Test]
        public void 縦の通路を反復移動する()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                new RandomImpl(),
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "000", // 壁壁壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "000", // 壁壁壁
                }),
                (1, 1)
            );

            var pcLocation = (-1, -1); // プレイキャラクターは接敵しない座標

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 2)), "down");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 3)), "down");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 2)), "up (reversed)");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 1)), "up");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 2)), "down (reversed)");
        }

        [Test]
        public void 横の通路を反復移動する()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                new RandomImpl(),
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "00000", // 壁壁壁壁壁
                    "01110", // 壁床床床壁
                    "00000", // 壁壁壁壁壁
                }),
                (1, 1)
            );

            var pcLocation = (-1, -1); // プレイキャラクターは接敵しない座標

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((2, 1)), "right");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((3, 1)), "right");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((2, 1)), "left (reversed)");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 1)), "left");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((2, 1)), "right (reversed)");
        }

        [Test]
        public void 接敵したら攻撃を続ける()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                new RandomImpl(),
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "000", // 壁壁壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "000", // 壁壁壁
                }),
                (1, 1)
            );

            var pcLocation = (0, 4); // プレイキャラクターは左下の壁の中にいる

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 2)), "down");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 3)), "down");

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 3)), "attack (not move)");
            // TODO: 攻撃は未実装なので移動しないことで判断

            enemyCharacterController.DoAction(pcLocation);
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 3)), "attack (not move)");

            enemyCharacterController.DoAction((-1, -1)); // 接敵を解消
            Assert.That(enemyCharacterController.GetMapLocation(), Is.EqualTo((1, 2)), "up (restart move)");
        }
    }
}
