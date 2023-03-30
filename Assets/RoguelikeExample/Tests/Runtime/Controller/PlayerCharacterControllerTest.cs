// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using NUnit.Framework;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// プレイヤーキャラクター操作のテスト
    ///
    /// - 使用しているScene（Dungeon.unity）は、Scenes in Buildに登録されているもの
    /// </summary>
    [TestFixture]
    public class PlayerCharacterControllerTest
    {
        private PlayerCharacterController _playerCharacterController;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Dungeon", LoadSceneMode.Single);

            _playerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
            Assume.That(_playerCharacterController, Is.Not.Null);

            // マップ生成
            var map = MapHelper.CreateFromDumpStrings(new[]
            {
                "0000", //　壁壁壁壁
                "0110", // 壁床床壁
                "0110", //　壁床床壁
                "0000", //　壁壁壁壁
            });
            _playerCharacterController.DungeonMap = new MapUtil(map);
            _playerCharacterController.SetMapPosition(1, 1);
        }

        [UnityTest]
        public IEnumerator Hを入力_左が壁でなければ移動する()
        {
            yield return null;
            // TODO: 移動完了時間をどう待つか
        }
    }
}
