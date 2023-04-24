// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.AI;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities.ScriptableObjects;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// 敵キャラクターの振る舞いのテスト
    ///
    /// 結合度高めのユニットテスト。
    /// 移動については個々の敵AIのテストで検証し、ここではAIの出した移動先に移動できないケースを中心に検証する
    /// </summary>
    [TestFixture]
    public class EnemyCharacterControllerTest
    {
        private EnemyManager _enemyManager;
        private PlayerCharacterController _playerCharacterController;

        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(EnemyCharacterControllerTest));
            SceneManager.SetActiveScene(scene);

            _enemyManager = new GameObject().AddComponent<EnemyManager>();

            _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
            _playerCharacterController.actionAnimationMillis = 0; // 行動アニメーション時間を0に
            _playerCharacterController.Initialize(new RandomImpl(), _enemyManager);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.UnloadSceneAsync(nameof(EnemyCharacterControllerTest));
        }

        [Test]
        public void Initialize_RaceのdisplayCharacterがTextMeshで表示される()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.displayCharacter = "仇";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/RoguelikeExample/Prefabs/EnemyCharacter.prefab");
            var gameObject = Object.Instantiate(prefab);
            var enemyCharacterController = gameObject.GetComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "1", // 床
                }),
                (0, 0),
                new RandomImpl()
            );

            var textMesh = enemyCharacterController.GetComponent<TextMeshPro>();
            Assert.That(textMesh.text, Is.EqualTo("仇"));
        }

        [Test]
        public async Task ThinkAction_移動先座標にプレイヤーキャラクター_移動せず攻撃する()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.transform.parent = _enemyManager.transform;
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "0000", // 壁壁壁壁
                    "0110", // 壁床床壁
                    "0000", // 壁壁壁壁
                }),
                (1, 1),
                new RandomImpl(),
                _enemyManager,
                _playerCharacterController
            );

            _playerCharacterController.SetPositionFromMapLocation(2, 1); // 唯一の移動先を塞ぐ

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.NextLocation, Is.EqualTo((1, 1)));
            // TODO: 攻撃は未実装
        }

        [Test]
        public async Task ThinkAction_移動先座標に別の敵キャラクター_移動しない()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.aiType = AIType.BackAndForth;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.transform.parent = _enemyManager.transform;
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "0000", // 壁壁壁壁
                    "0110", // 壁床床壁
                    "0000", // 壁壁壁壁
                }),
                (1, 1),
                new RandomImpl(),
                _enemyManager,
                _playerCharacterController
            );

            var existEnemy = new GameObject().AddComponent<EnemyCharacterController>();
            existEnemy.transform.parent = _enemyManager.transform;
            existEnemy.SetPositionFromMapLocation(2, 1); // 唯一の移動先を塞ぐ

            await enemyCharacterController.DoAction();
            Assert.That(enemyCharacterController.NextLocation, Is.EqualTo((1, 1)));
        }

        [Ignore("壁に向かって移動しようとするAIが実装されたら追加予定")]
        [Test]
        public void ThinkAction_移動先座標が壁_移動しない()
        {
            // TODO: AIの実装によっては壁に向かって移動しようとするので、それを回避する実装のテスト
        }
    }
}
