// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using RoguelikeExample.Entities.ScriptableObjects;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Controller
{
    [TestFixture]
    public class EnemyCharacterControllerTest
    {
        [SetUp]
        public void SetUp()
        {
            var scene = SceneManager.CreateScene(nameof(EnemyCharacterControllerTest));
            SceneManager.SetActiveScene(scene);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return SceneManager.UnloadSceneAsync(nameof(EnemyCharacterControllerTest));
        }

        [Test]
        public void Initialize_RaceのdisplayCharacterがTextMeshで表示される()
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.displayCharacter = "仇";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/RoguelikeExample/Prefabs/EnemyCharacter.prefab");
            var gameObject = Object.Instantiate(prefab);
            var enemyCharacterController = gameObject.GetComponent<EnemyCharacterController>();
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                new RandomImpl(),
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "1", // 床
                }),
                (0, 0)
            );

            var textMesh = enemyCharacterController.GetComponent<TextMeshPro>();
            Assert.That(textMesh.text, Is.EqualTo("仇"));
        }
    }
}
