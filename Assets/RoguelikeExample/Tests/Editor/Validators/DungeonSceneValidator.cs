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
    /// 初期設定の確認ではなく、変更時のミスを早期に検出することが目的です。
    /// コンポーネントの設定値の検証まで書いてしまうと、Sceneを変更するたびにテストも修正しなければならなくなります。
    /// 他に影響のある致命的な設定ミスや、逆に気づきにくいミスを検出するに留めるのが理想です。
    /// </summary>
    [TestFixture]
    public class DungeonSceneValidator
    {
        private const string PlayerCharacterObjectName = "PlayerCharacter";

        [SetUp]
        public void SetUp()
        {
            EditorSceneManager.OpenScene("Assets/RoguelikeExample/Scenes/Dungeon.unity");

            var dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            dungeonManager.enabled = false; // ダンジョンと敵の生成は抑止
        }

        [Test]
        public void CameraControllerにPlayerCharacterへの参照がセットされていること()
        {
            var cameraController = Object.FindAnyObjectByType<CameraController>();
            Assume.That(cameraController, Is.Not.Null);

            var playerCharacter = GameObject.Find(PlayerCharacterObjectName);
            Assume.That(playerCharacter, Is.Not.Null);

            Assert.That(cameraController.trackedTarget.gameObject, Is.EqualTo(playerCharacter));
            // Note: ここで例えば、RelativePositionの設定値まで検証することは推奨しません
        }

        [Test]
        public void DirectionalLightが存在すること()
        {
            var directionalLight = Object.FindAnyObjectByType<Light>();
            Assume.That(directionalLight, Is.Not.Null);

            Assert.That(directionalLight.type, Is.EqualTo(LightType.Directional));
        }

        [Test]
        public void DungeonManagerが存在すること()
        {
            var dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            Assert.That(dungeonManager, Is.Not.Null);
        }

        [Test]
        public void PlayerCharacterが存在すること()
        {
            var playerCharacter = GameObject.Find(PlayerCharacterObjectName);
            Assume.That(playerCharacter, Is.Not.Null);

            var playerCharacterController = playerCharacter.GetComponent<PlayerCharacterController>();
            Assert.That(playerCharacterController, Is.Not.Null, "PlayerCharacterControllerがアタッチされていること");
        }
    }
}
