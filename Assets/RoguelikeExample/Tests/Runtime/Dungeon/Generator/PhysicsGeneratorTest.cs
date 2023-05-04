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
    /// ダンジョン3Dオブジェクト配置のテスト
    ///
    /// 渡されたマップ通りに5種類のPrefabをロードして配置できていることを確認しています。
    /// 個々のPrefabの内容は別のテストで確認する前提で、配置はスクリーンショットを目視確認しています。
    /// </summary>
    [TestFixture]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [Category("IgnoreCI")] // スクリーンショット撮影を含むためバッチモードでは動作しない
    public class PhysicsGeneratorTest
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
#if UNITY_EDITOR
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/RoguelikeExample/Tests/TestData/Scenes/DungeonGeneratorSandbox.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            // Note: カメラとライトが必要なのでCreateSceneでなくサンドボックスを使用しています。
            //       Scenes in BuildにないSceneなので、EditorSceneManagerでロードしています
#endif
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

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.transform.childCount, Is.EqualTo(map.Length));

            yield return ScreenshotHelper.CaptureScreenshot(); // TODO: ルックが安定したらImageAssertに変更
        }
    }
}
