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
            camera.transform.position = new Vector3(0, 20, -10);
            camera.transform.LookAt(Vector3.zero);

            var light = new GameObject("Directional Light").AddComponent<Light>();
            light.transform.rotation = Quaternion.Euler(new Vector3(50, -30, 0));
            light.type = LightType.Directional;
            light.color = Color.white;
        }

        [TearDown]
        public void TearDown()
        {
            SceneManager.UnloadSceneAsync(nameof(PhysicsGeneratorTest));
        }

        [UnityTest]
        public IEnumerator Generate_マップの通りPrefabが配置されること()
        {
            var map = MapHelper.CreateFromDumpStrings(new[]
            {
                "013", // 壁、部屋、上り階段
                "024", // 壁、通路、下り階段
            });

            var actual = PhysicsGenerator.Generate(map);

            yield return ScreenshotHelper.CaptureScreenshot(); // TODO: ルックが安定したらImageAssertに変更してもいいかも

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.transform.childCount, Is.EqualTo(map.Length));
        }
    }
}
