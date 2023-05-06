// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Dungeon;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// インゲームのシナリオテスト（統合テスト）
    ///
    /// <c>InputEventTrace</c>による操作再生のコードは、Input SystemパッケージのInput Recorderを参考にしています。
    /// またレコードデータはInput Recorderで記録したものです。
    /// 記録時は "Record Frames" のみonにしています。
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class InGameScenarioTest
    {
        private const string InputTracesPath = "Assets/RoguelikeExample/Tests/TestData/InputTraces";

        [TestCase("Keyboard_400.inputtrace", "400")]
        public async Task インゲームのシナリオテスト_InputTraceを再生_地下2階に到達すること(string path, string seed)
        {
            FocusGameView(); // キーボード・マウス操作ではGameビューにフォーカスを移さないと動作しない

            await SceneManager.LoadSceneAsync("Dungeon");

            var dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            dungeonManager.randomSeed = seed; // 乱数シードを固定

            using var eventTrace = new InputEventTrace();
            eventTrace.ReadFrom(Path.GetFullPath(Path.Combine(InputTracesPath, path)));

            var isFinished = false;

            using var replayController = eventTrace.Replay()
                .OnFinished(() => { isFinished = true; }) // 再生終了したらフラグを立てる
                .PlayAllFramesOneByOne(); // 記録されたフレームを再現しつつ再生

            while (!isFinished)
            {
                await UniTask.NextFrame();
            }

            Assert.That(dungeonManager.level, Is.EqualTo(2));
        }

        private static void FocusGameView()
        {
#if UNITY_EDITOR
            var assembly = Assembly.Load("UnityEditor.dll");
            var gameView = assembly.GetType("UnityEditor.GameView");
            EditorWindow.FocusWindowIfItsOpen(gameView);
#endif
        }
    }
}
