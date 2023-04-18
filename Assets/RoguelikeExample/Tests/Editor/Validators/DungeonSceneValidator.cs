// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

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
        private CameraController _cameraController;
        private Light _light;
        private DungeonManager _dungeonManager;
        private PlayerCharacterController _playerCharacterController;

        [OneTimeSetUp]
        public void SetUp()
        {
            EditorSceneManager.OpenScene("Assets/RoguelikeExample/Scenes/Dungeon.unity");

            _cameraController = Object.FindAnyObjectByType<CameraController>();
            Assume.That(_cameraController, Is.Not.Null);

            _light = Object.FindAnyObjectByType<Light>();
            Assume.That(_light, Is.Not.Null);

            _dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            Assume.That(_dungeonManager, Is.Not.Null);
            _dungeonManager.enabled = false; // ダンジョンと敵の生成は抑止

            _playerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
            Assume.That(_playerCharacterController, Is.Not.Null);
        }

        [Test]
        public void CameraControllerにPlayerCharacterへの参照がセットされていること()
        {
            Assert.That(_cameraController.trackedTarget.gameObject, Is.EqualTo(_playerCharacterController.gameObject));
            // Note: ここでRelativePositionの設定値まで検証することは推奨しません
        }

        [Test]
        public void DirectionalLightが存在すること()
        {
            Assert.That(_light.type, Is.EqualTo(LightType.Directional));
        }

        [Test]
        public void DungeonManagerにPlayerCharacterControllerへの参照がセットされていること()
        {
            Assert.That(_dungeonManager.playerCharacter, Is.EqualTo(_playerCharacterController));
        }

        [Test]
        public void PlayerCharacterControllerにDungeonManagerへの参照がセットされていること()
        {
            Assert.That(_playerCharacterController.dungeonManager, Is.EqualTo(_dungeonManager));
        }

        [Test]
        public void EnemyManagerは初期配置されていてはいけない()
        {
            var enemyManager = Object.FindAnyObjectByType<EnemyManager>();
            Assume.That(enemyManager, Is.Null);
        }
    }
}
