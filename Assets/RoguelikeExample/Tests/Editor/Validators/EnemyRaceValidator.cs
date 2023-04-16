// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using RoguelikeExample.Entities.ScriptableObjects;
using UnityEditor;

namespace RoguelikeExample.Editor.Validators
{
    /// <summary>
    /// 敵データのScriptableObjectに設定の漏れがないかを検証します
    /// </summary>
    public class EnemyRaceValidator
    {
        // EnemyRace型のScriptableObjectを列挙
        private static IEnumerable<string> Paths => AssetDatabase
            .FindAssets("t:EnemyRace", new[] { "Assets/RoguelikeExample" })
            .Select(AssetDatabase.GUIDToAssetPath);

        // EnemyRace型のフィールドを列挙
        private static IEnumerable<FieldInfo> Fields =>
            typeof(EnemyRace).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        [Test]
        public void フィールドに設定漏れがないこと(
            [ValueSource(nameof(Paths))] string path,
            [ValueSource(nameof(Fields))] FieldInfo field)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            var value = field.GetValue(obj);

            switch (value)
            {
                case string s:
                    Assert.That(s, Is.Not.Empty);
                    break;
                case int i:
                    Assert.That(i, Is.GreaterThan(0));
                    break;
                case Array array:
                    Assert.That(array, Has.Length.GreaterThanOrEqualTo(0));
                    break;
                case AI.AIType ai:
                    Assert.That((int)ai, Is.GreaterThanOrEqualTo(0));
                    break;
                default:
                    Assert.Fail($"Unsupported field type: {field.Name}");
                    break;
            }
        }
    }
}
