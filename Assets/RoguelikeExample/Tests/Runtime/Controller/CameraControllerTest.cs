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
    /// </summary>
    [TestFixture]
    public class CameraControllerTest
    {
        private readonly QuaternionEqualityComparer _rotateComparer = new QuaternionEqualityComparer(0.00001f);

        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(CameraControllerTest));
            SceneManager.SetActiveScene(scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.UnloadSceneAsync(nameof(CameraControllerTest));
        }

        [UnityTest]
        public IEnumerator TrackTargetの移動に追随してpositionとrotationが設定されること()
        {
            var target = new GameObject();
            var camera = new GameObject().AddComponent<CameraController>();
            camera.trackedTarget = target.transform;
            camera.relativePosition = new Vector3(0, 10, -10);

            // move
            target.transform.position = new Vector3(1, 2, 3);
            yield return null;

            Assert.That(camera.transform.position, Is.EqualTo(new Vector3(1, 12, -7)));
            Assert.That(camera.transform.rotation, Is.EqualTo(Quaternion.Euler(45, 0, 0)).Using(_rotateComparer));
        }
    }
}
