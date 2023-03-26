// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Dungeon.Generator
{
    /// <summary>
    /// ダンジョン3Dオブジェクト生成のテスト
    ///
    /// - マップ通りに5種類Prefabをロードして配置できていることが確認できればいい
    /// - 個々のPrefabの内容は別のテストで確認する前提で、配置はスクリーンショットを目視確認
    /// - スクリーンショット撮影のためクリーンなSceneとCameraをセットアップしている
    /// </summary>
    [TestFixture]
    public class PhysicsGeneratorTest
    {
        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(PhysicsGeneratorTest));
            SceneManager.SetActiveScene(scene);

            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            var cameraTransform = camera.transform;
            cameraTransform.position = new Vector3(8, 10, 8);
            cameraTransform.LookAt(Vector3.zero);
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadSceneAsync(nameof(PhysicsGeneratorTest));
        }

        [UnityTest]
        public IEnumerator Generate_マップの通りPrefabが配置されること()
        {
            var map = new MapChip[,]
            {
                { MapChip.Wall, MapChip.Room, MapChip.UpStair, }, // 壁、部屋（床）、上り階段
                { MapChip.Wall, MapChip.Corridor, MapChip.DownStair, }, // 壁、通路（床）、下り階段
            };

            var actual = PhysicsGenerator.Generate(map);

            yield return ScreenshotHelper.CaptureScreenshot(); // TODO: ルックが安定したらImageAssertに変更してもいいかも

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.transform.childCount, Is.EqualTo(map.Length));
        }
    }
}
