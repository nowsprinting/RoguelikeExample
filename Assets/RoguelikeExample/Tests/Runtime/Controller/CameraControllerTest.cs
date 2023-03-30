// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// プレイヤーキャラクターを追尾するカメラのテスト
    ///
    /// - 使用しているScene（Dungeon.unity）は、Scenes in Buildに登録されているもの
    /// </summary>
    [TestFixture]
    public class CameraControllerTest
    {
        private readonly QuaternionEqualityComparer _rotateComparer = new QuaternionEqualityComparer(0.00001f);
        private GameObject _camera;
        private GameObject _target;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Dungeon", LoadSceneMode.Single);

            _camera = GameObject.Find("Main Camera");
            Assume.That(_camera, Is.Not.Null);

            _target = GameObject.Find("PlayerCharacter");
            Assume.That(_target, Is.Not.Null);
        }

        /// <summary>
        /// Sceneの設定を検証するテスト
        ///
        /// 設定漏れや意図せず書き換わってしまうことを防止できる反面、インスペクタで意図的な変更を行なう都度、テストに反映する必要がある。
        /// 有効なテストかどうかはケースバイケースだが、本例では、本来無くてよいテストです。
        /// </summary>
        [Test]
        public void SceneにあるCameraController設定の検証()
        {
            var cameraController = _camera.GetComponent<CameraController>();
            Assert.That(cameraController.trackedTarget.gameObject, Is.EqualTo(_target));
            Assert.That(cameraController.relativePosition, Is.EqualTo(new Vector3(0, 10, -5)));
        }

        [UnityTest]
        public IEnumerator TrackTargetの移動に追随すること()
        {
            var cameraController = _camera.GetComponent<CameraController>();
            cameraController.trackedTarget = _target.transform;
            cameraController.relativePosition = new Vector3(0, 10, -10);

            // move
            _target.transform.position = new Vector3(1, 2, 3);
            yield return null;

            Assert.That(_camera.transform.position, Is.EqualTo(new Vector3(1, 12, -7)));
            Assert.That(_camera.transform.rotation, Is.EqualTo(Quaternion.Euler(45, 0, 0)).Using(_rotateComparer));
        }
    }
}
