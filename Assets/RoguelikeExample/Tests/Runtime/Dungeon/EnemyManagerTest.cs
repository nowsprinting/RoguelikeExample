// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Dungeon
{
    public class EnemyManagerTest
    {
        private const int RepeatCount = 1;
        // Note: 開発時は繰り返し回数を増やして多数試行、以降はリグレッションテストとして1回だけ実行

        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(EnemyManagerTest));
            SceneManager.SetActiveScene(scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.UnloadSceneAsync(nameof(EnemyManagerTest));
        }

        [Ignore("未実装")] // TODO: 未実装のためignore
        [Test]
        [Repeat(RepeatCount)]
        public void Initialize_指定レベルの敵インスタンスが生成される([NUnit.Framework.Range(1, 1)] int level)
        {
            var enemyManager = new GameObject().AddComponent<EnemyManager>();
            enemyManager.Initialize(
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "1", // 床のみ（必ず配置される）
                }),
                1,
                1,
                new RandomImpl()
            );

            var createdEnemies = enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(1), "maxInstantiateEnemiesだけ生成");
            Assert.That(createdEnemies[0].Status.Race.lowestSpawnLevel, Is.LessThanOrEqualTo(level), "出現レベル下限");
            Assert.That(createdEnemies[0].Status.Race.highestSpawnLevel, Is.GreaterThanOrEqualTo(level), "出現レベル上限");
            Assert.That(createdEnemies[0].Status.Level, Is.EqualTo(level), "敵インスタンスのレベル");
        }

        [Ignore("未実装")] // TODO: 未実装のためignore
        [Test]
        [Repeat(RepeatCount)]
        // [Retry(2)] // 抽選結果によっては配置されないときもあるため
        public void Initialize_配置可能な座標に配置される()
        {
            var enemyManager = new GameObject().AddComponent<EnemyManager>();
            enemyManager.Initialize(
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "0123", // 壁、部屋、通路、上り階段（このrowの床にはキャラクターを置くため配置されない）
                    "0124", // 壁、部屋、通路、下り階段
                }),
                1,
                1,
                new RandomImpl()
            );

            var existEnemy = new GameObject().AddComponent<EnemyCharacterController>();
            existEnemy.SetPositionFromMapLocation(1, 0);
            existEnemy.transform.parent = enemyManager.transform;

            var playerCharacter = new GameObject().AddComponent<PlayerCharacterController>();
            playerCharacter.SetPositionFromMapLocation(2, 0);

            var createdEnemies = enemyManager.GetComponentsInChildren<EnemyCharacterController>();
            Assert.That(createdEnemies, Has.Length.EqualTo(1), "maxInstantiateEnemiesだけ生成");
            Assert.That(createdEnemies[0].MapLocation().column, Is.InRange(1, 2), "(1,1)か(2,1)にのみ配置される");
            Assert.That(createdEnemies[0].MapLocation().row, Is.EqualTo(1));
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
    }
}
