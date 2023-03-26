// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using RoguelikeExample.Dungeon.Generator;
using RoguelikeExample.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample
{
    [TestFixture]
    public class DisplayCharacterTest
    {
        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(DisplayCharacterTest));
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
            SceneManager.UnloadSceneAsync(nameof(DisplayCharacterTest));
        }

        [UnityTest]
        public IEnumerator TextMeshProを使用したGameObjectを表示できること()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PlayerCharacter.prefab");
            Object.Instantiate(prefab);

            yield return ScreenshotHelper.CaptureScreenshot();
        }
    }
}
