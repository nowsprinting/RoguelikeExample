// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RoguelikeExample.Dungeon
{
    /// <summary>
    /// 敵キャラクター管理クラスのテスト
    /// 主に敵インスタンス生成ロジックのテスト
    /// </summary>
    [TestFixture, Timeout(5000)]
    public class EnemyManagerTest
    {
        private PlayerCharacterController _playerCharacterController;
        private EnemyManager _enemyManager;
        private Turn _turn;

        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(TestContext.CurrentContext.Test.ClassName);
            SceneManager.SetActiveScene(scene);

            _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
            _enemyManager = new GameObject().AddComponent<EnemyManager>();
            _turn = new Turn();

            _enemyManager.Initialize(new RandomImpl(), _turn, _playerCharacterController);
            _enemyManager.popTrialCount = 1000; // 試行回数を大幅に増やす
            // Note: NewLevel()を呼ぶまでは敵キャラクターは生成されない

            _playerCharacterController.Initialize(new RandomImpl(), _turn, _enemyManager);
            _playerCharacterController.NewLevel(
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "000000000000000000000000000000000000000000000000000000000001",
                }),
                (59, 0) // 敵配置に影響しないところに配置
            );
        }

        [TearDown]
        public async Task TearDown()
        {
            await Task.Delay(200); // オブジェクトの破棄を待つ
            await SceneManager.UnloadSceneAsync(TestContext.CurrentContext.Test.ClassName);
        }

        [Test]
        public void NewLevel_指定レベルの敵インスタンスが生成される([NUnit.Framework.Range(1, 1)] int level)
        {
            _enemyManager.maxInstantiateEnemiesPercentageOfFloor = 1.0f; // 床1つに対して1体を必ず生成
            _enemyManager.NewLevel(
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "1", // 床のみ（必ず配置される）
                })
            );

            var createdEnemies = _enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(1), "1体だけ生成");
            Assert.That(createdEnemies[0].Status.Race.lowestSpawnLevel, Is.LessThanOrEqualTo(level), "種族のレベル下限");
            Assert.That(createdEnemies[0].Status.Race.highestSpawnLevel, Is.GreaterThanOrEqualTo(level), "種族のレベル上限");
            Assert.That(createdEnemies[0].Status.Level, Is.EqualTo(level), "敵インスタンスのレベル");
        }

        [Test]
        public void NewLevel_指定レベルにRaceのSOが存在しないとき_敵インスタンスは生成されない()
        {
            const int InvalidLevel = -1;

            _enemyManager.maxInstantiateEnemiesPercentageOfFloor = 1.0f; // 床1つに対して1体を必ず生成
            _enemyManager.NewLevel(
                InvalidLevel,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "1", // 床のみ（必ず配置される）
                })
            );

            var createdEnemies = _enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(0));
        }

        [Test]
        public void NewLevel_配置可能な座標に敵インスタンスが配置される()
        {
            var existEnemy = new GameObject().AddComponent<EnemyCharacterController>();
            existEnemy.SetPositionFromMapLocation(1, 0);
            existEnemy.transform.parent = _enemyManager.transform;

            var existEnemy2 = new GameObject().AddComponent<EnemyCharacterController>();
            existEnemy2.SetPositionFromMapLocation(2, 0);
            existEnemy2.transform.parent = _enemyManager.transform;

            _enemyManager.maxInstantiateEnemiesPercentageOfFloor = 0.25f; // 床4つに対して1体だけ生成
            _enemyManager.NewLevel(
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "0123", // 壁、部屋、通路、上り階段（このrowの床にはキャラクターを置くため配置されない）
                    "0124", // 壁、部屋、通路、下り階段
                })
            );

            Object.DestroyImmediate(existEnemy.gameObject); // 事前に配置した敵インスタンスは破棄
            Object.DestroyImmediate(existEnemy2.gameObject); // 事前に配置した敵インスタンスは破棄

            var createdEnemies = _enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(1), "1体だけ生成");
            Assert.That(createdEnemies[0].MapLocation().column, Is.InRange(1, 2), "(1,1)か(2,1)にのみ配置される");
            Assert.That(createdEnemies[0].MapLocation().row, Is.EqualTo(1));
        }

        [Test]
        public async Task RefillEnemies_EnemyPopupで敵インスタンスが補充される_1ターンに1体のみ補充される()
        {
            _enemyManager.maxInstantiateEnemiesPercentageOfFloor = 1.0f; // 床1つに対して最大4体生成
            _enemyManager.NewLevel(
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "1111", // 床床床床
                })
            );

            foreach (var enemyCharacterController in _enemyManager.GetComponentsInChildren<EnemyCharacterController>())
            {
                Object.DestroyImmediate(enemyCharacterController.gameObject); // NewLevelで配置された敵インスタンスは破棄
            }

            Assume.That(_enemyManager.GetComponentsInChildren<EnemyCharacterController>(), Is.Empty, "敵キャラは0");

            await WaitForNextPlayerIdol(_turn); // EnemyPopupまで進める

            var createdEnemies = _enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(1));
        }

        [Test]
        public async Task RefillEnemies_EnemyPopupで敵インスタンスが1体ずつ補充される_上限までしか補充されない()
        {
            _enemyManager.maxInstantiateEnemiesPercentageOfFloor = 0.5f; // 床1つに対して最大2体生成
            _enemyManager.NewLevel(
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "000000", // 壁壁壁壁壁壁
                    "011110", // 壁床床床床壁
                    "000000", // 壁壁壁壁壁壁
                    // Note: AIが動くので整合性の取れているマップが必要
                })
            );

            foreach (var enemyCharacterController in _enemyManager.GetComponentsInChildren<EnemyCharacterController>())
            {
                Object.DestroyImmediate(enemyCharacterController.gameObject); // NewLevelで配置された敵インスタンスは破棄
            }

            Assume.That(_enemyManager.GetComponentsInChildren<EnemyCharacterController>(), Is.Empty, "敵キャラは0");

            for (var i = 0; i < 10; i++)
            {
                await WaitForNextPlayerIdol(_turn); // EnemyPopupを10回
            }

            var createdEnemies = _enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(2));
        }

        [Test]
        public void ExistEnemy_指定座標に敵キャラクターは存在しない_nullを返す()
        {
            var enemyManager = new GameObject().AddComponent<EnemyManager>();

            var actual = enemyManager.ExistEnemy((0, 0));
            Assert.That(actual, Is.Null);
        }

        [Test]
        public void ExistEnemy_指定座標に敵キャラクターが存在する_インスタンスを返す()
        {
            var enemyManager = new GameObject().AddComponent<EnemyManager>();
            var existEnemy = new GameObject().AddComponent<EnemyCharacterController>();
            existEnemy.SetPositionFromMapLocation(0, 0);
            existEnemy.transform.parent = enemyManager.transform;

            var actual = enemyManager.ExistEnemy((0, 0));
            Assert.That(actual, Is.EqualTo(existEnemy));
        }

        private static async UniTask WaitForNextPlayerIdol(Turn turn)
        {
            if (turn.State == TurnState.PlayerIdol)
            {
                await turn.NextPhase(); // Idolを強制スキップ
            }

            // まず、プレイヤーフェイズを抜けるまで待つ
            while (turn.State <= TurnState.PlayerAction)
            {
                await UniTask.NextFrame();
            }

            // 次のIdolまで待つ
            while (turn.State != TurnState.PlayerIdol)
            {
                await UniTask.NextFrame();
            }
        }
    }
}
