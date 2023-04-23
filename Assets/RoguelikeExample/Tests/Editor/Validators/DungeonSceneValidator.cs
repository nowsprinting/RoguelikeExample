// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Dungeon;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoguelikeExample.Editor.Validators
{
    /// <summary>
    /// Dungeon.unityに必要なGameObjectの設定を検証する
    ///
    /// 開発時の確認ではなく、変更時のミスを早期に検出することが目的です。
    /// コンポーネントの設定値の検証まで書いてしまうと、Sceneを変更するたびにテストも修正しなければならなくなります。
    /// 他に影響のある致命的な設定ミスや、逆に気づきにくいミスを検出するに留めるのが理想です。
    /// </summary>
    [TestFixture]
    public class DungeonSceneValidator
    {
        private DungeonManager _dungeonManager;
        private EnemyManager _enemyManager;
        private PlayerCharacterController _playerCharacterController;

        [OneTimeSetUp]
        public void SetUp()
        {
            EditorSceneManager.OpenScene("Assets/RoguelikeExample/Scenes/Dungeon.unity");

            _dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            Assume.That(_dungeonManager, Is.Not.Null);
            _dungeonManager.enabled = false; // ダンジョンと敵の生成は抑止

            _enemyManager = Object.FindAnyObjectByType<EnemyManager>();
            Assume.That(_enemyManager, Is.Not.Null);

            _playerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
            Assume.That(_playerCharacterController, Is.Not.Null);
        }

        [Test]
        public void CameraControllerにPlayerCharacterへの参照がセットされていること()
        {
            var cameraController = Object.FindAnyObjectByType<CameraController>();

            Assert.That(cameraController, Is.Not.Null);
            Assert.That(cameraController.trackedTarget.gameObject, Is.EqualTo(_playerCharacterController.gameObject));
            // Note: ここでRelativePositionの設定値まで検証することは推奨しません
        }

        [Test]
        public void DirectionalLightが存在すること()
        {
            var light = Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.type == LightType.Directional);

            Assert.That(light, Is.Not.Null);
        }

        [Test]
        public void DungeonManagerにEnemyManagerへの参照がセットされていること()
        {
            Assert.That(_dungeonManager.enemyManager, Is.EqualTo(_enemyManager));
        }

        [Test]
        public void DungeonManagerにPlayerCharacterControllerへの参照がセットされていること()
        {
            Assert.That(_dungeonManager.playerCharacterController, Is.EqualTo(_playerCharacterController));
        }
    }
}
