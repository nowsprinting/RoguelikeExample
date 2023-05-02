// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Input.CustomComposites;
using RoguelikeExample.Input.CustomProcessors;
using RoguelikeExample.Random;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// インゲームのモンキーテスト（統合テスト）
    ///
    /// 一定時間でたらめな操作をします。
    /// テスト失敗と判断されるのは次の2パターン
    /// - ログにエラー（プロダクトコードに仕込んだ UnityEngine.Assertions.Assert を含む）が出力されたとき
    /// - 一定時間プレイヤーキャラクターが移動しないとき
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class InGameMonkeyTest
    {
        private readonly InputTestFixture _input = new InputTestFixture();

        [SetUp]
        public async Task SetUp()
        {
            _input.Setup();
            // Note: プロダクトコードでInputSystemが初期化されるより前に `InputTestFixture.SetUp` を実行する必要がある
            // Note: `InputTestFixture` を継承する書きかたもあるが、SetUp/TearDownと競合するため選択していない

            InputSystem.RegisterBindingComposite<EightDirectionsComposite>();
            InputSystem.RegisterProcessor<SnapVector2Processor>();
            // Note: カスタムComposite, Interaction, Processorを使用しているプロジェクトでは、Setupの後に再Registerする

            await SceneManager.LoadSceneAsync("Dungeon");

            var dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            dungeonManager.level = 3; // すぐ地上に出ないように初期レベルを変更
        }

        [TearDown]
        public void TearDown()
        {
            _input.TearDown();
        }

        [Test]
        [Timeout(200000)] // タイムアウトを3分強に設定（デフォルトは180,000ms）
        public async Task インゲームのモンキーテスト()
        {
            var random = new RandomImpl();
            Debug.Log($"Using {random}"); // 擬似乱数発生器のシード値を出力（再現可能にするため）
            // Note: DungeonManagerも同様にシード値を出力しています

            var playerCharacterController = Object.FindAnyObjectByType<PlayerCharacterController>();
            var lastLocation = playerCharacterController.MapLocation();
            var dontMoveCount = 0;

            var keyboard = InputSystem.AddDevice<Keyboard>();
            var keys = new[]
            {
                keyboard.hKey, keyboard.jKey, keyboard.kKey, keyboard.lKey, keyboard.yKey, keyboard.uKey,
                keyboard.bKey, keyboard.nKey, keyboard.spaceKey, keyboard.escapeKey
            };

            _input.Press(keyboard.ctrlKey); // 単なるレバガチャにならないよう、常に高速移動

            var expireTime = Time.time + 180.0f; // タイムアウト少し手前まで動作
            while (Time.time < expireTime)
            {
                var key = keys[random.Next(keys.Length)]; // 操作するキーを抽選
                _input.Press(key); // 押す
                await UniTask.DelayFrame(random.Next(10));

                _input.Release(key); // 離す
                await UniTask.DelayFrame(random.Next(10));

                var nowLocation = playerCharacterController.MapLocation();
                if (nowLocation == lastLocation)
                {
                    if (++dontMoveCount > 80)
                    {
                        Assert.Fail("一定時間プレイヤーキャラクターが移動していない");
                    }
                }
                else
                {
                    lastLocation = nowLocation;
                    dontMoveCount = 0;
                }
            }
        }
    }
}
