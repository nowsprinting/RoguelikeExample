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
    public class PhysicsGeneratorTest
    {
        [SetUp]
        public void SetUp()
        {
            SceneManager.CreateScene(nameof(PhysicsGeneratorTest));

            var camera = new GameObject().AddComponent<Camera>();
            var transform = camera.transform;
            transform.position = new Vector3(8, 10, 8);
            transform.LookAt(Vector3.zero);
        }

        [UnityTest]
        public IEnumerator Generate_マップの通りPrefabが配置されること()
        {
            var map = new MapChip[,]
            {
                { MapChip.Wall, MapChip.Wall, MapChip.Wall, MapChip.Wall, MapChip.Wall, MapChip.Wall },
                { MapChip.Wall, MapChip.UpStair, MapChip.Corridor, MapChip.Room, MapChip.DownStair, MapChip.Wall },
                { MapChip.Wall, MapChip.Wall, MapChip.Wall, MapChip.Wall, MapChip.Wall, MapChip.Wall },
            };

            var actual = PhysicsGenerator.Generate(map);

            yield return ScreenshotHelper.CaptureScreenshot(); // TODO: ルックが安定したらImageAssertに変更してもいいかも

            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.transform.childCount, Is.EqualTo(map.Length));
        }
    }
}
