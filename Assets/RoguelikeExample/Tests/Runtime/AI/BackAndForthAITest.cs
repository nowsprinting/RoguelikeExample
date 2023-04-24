// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Dungeon;
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
        private EnemyManager _enemyManager;
        private PlayerCharacterController _playerCharacterController;

        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(BackAndForthAITest));
            SceneManager.SetActiveScene(scene);

            _enemyManager = new GameObject().AddComponent<EnemyManager>();

            _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
            _playerCharacterController.actionAnimationMillis = 0; // 行動アニメーション時間を0に
            _playerCharacterController.Initialize(new RandomImpl(), _enemyManager);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.UnloadSceneAsync(nameof(BackAndForthAITest));
        }

        [Test]
        public async Task 縦の通路を反復移動する()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "000", // 壁壁壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "000", // 壁壁壁
                }),
                (1, 1),
                new RandomImpl(),
                _enemyManager,
                _playerCharacterController
            );

            _playerCharacterController.SetPositionFromMapLocation(-1, -1); // 接敵しない座標

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 2)), "down");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 3)), "down");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 2)), "up (reversed)");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 1)), "up");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 2)), "down (reversed)");
        }

        [Test]
        public async Task 横の通路を反復移動する()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "00000", // 壁壁壁壁壁
                    "01110", // 壁床床床壁
                    "00000", // 壁壁壁壁壁
                }),
                (1, 1),
                new RandomImpl(),
                _enemyManager,
                _playerCharacterController
            );

            _playerCharacterController.SetPositionFromMapLocation(-1, -1); // プレイキャラクターは接敵しない座標

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((2, 1)), "right");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((3, 1)), "right");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((2, 1)), "left (reversed)");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 1)), "left");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((2, 1)), "right (reversed)");
        }

        [Test]
        public async Task 接敵したら攻撃を続ける()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "000", // 壁壁壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "010", // 壁床壁
                    "000", // 壁壁壁
                }),
                (1, 1),
                new RandomImpl(),
                _enemyManager,
                _playerCharacterController
            );

            _playerCharacterController.SetPositionFromMapLocation(0, 4); // プレイキャラクターは左下の壁の中にいる

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 2)), "down");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 3)), "down");

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 3)), "attack (not move)");
            // Note: 敵の攻撃は未実装なので移動しないことで判断

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 3)), "attack (not move)");
            // Note: 敵の攻撃は未実装なので移動しないことで判断

            _playerCharacterController.SetPositionFromMapLocation(-1, -1); // 接敵を解消

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.MapLocation(), Is.EqualTo((1, 2)), "up (restart move)");
        }
    }
}
