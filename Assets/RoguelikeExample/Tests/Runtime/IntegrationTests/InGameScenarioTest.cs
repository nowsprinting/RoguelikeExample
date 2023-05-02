// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Dungeon;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// インゲームのシナリオテスト（統合テスト）
    ///
    /// <c>InputEventTrace</c>による操作再生のコードは、Input SystemパッケージのInput Recorderを参考にしています。
    /// またレコードデータはInput Recorderで記録したものです。
    /// 記録時は "Record Frames" のみonにしています。
    /// Gamepadで記録したデータは安定して再生できていますが、Keyboardで記録したデータは不安定です。
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class InGameScenarioTest
    {
        private const string InputTracesPath = "Assets/RoguelikeExample/Tests/TestData/InputTraces";

        [TestCase("400", "Gamepad_400.inputtrace")]
        public async Task インゲームのシナリオテスト_InputTraceを再生_B2に到達すること(string seed, string path)
        {
            await SceneManager.LoadSceneAsync("Dungeon");

            var dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            dungeonManager.randomSeed = seed; // 乱数シードを固定

            using var eventTrace = new InputEventTrace();
            eventTrace.ReadFrom(Path.GetFullPath(Path.Combine(InputTracesPath, path)));

            var isFinished = false;

            using var replayController = eventTrace.Replay()
                .OnFinished(() => { isFinished = true; })
                .PlayAllFramesOneByOne();

            while (!isFinished)
            {
                await UniTask.NextFrame();
            }

            Assert.That(dungeonManager.level, Is.EqualTo(2));
        }
    }
}
