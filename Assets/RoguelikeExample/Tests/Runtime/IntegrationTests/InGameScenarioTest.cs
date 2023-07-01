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
using UnityEngine.TestTools;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
#endif

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// インゲームのシナリオテスト（統合テスト）
    ///
    /// <c>InputEventTrace</c>による操作再生のコードは、Input Systemパッケージの *Input Recorder* サンプルを参考にしています。
    /// またレコードデータは *Input Recorder* で記録したものです。
    /// 記録時は *Record Frames* のみonにしています。
    /// </summary>
    /// <remarks>
    /// Linuxでの再生時、以下のエラーが発生するため、実行対象から除外しています。詳細は未調査だが、おそらく接続されているデバイスの差異。
    /// `Unhandled log message: '[Exception] ArgumentException: State format KEYS from event does not match state format MOUS of device Mouse:/Mouse Parameter name: eventPtr'. Use UnityEngine.TestTools.LogAssert.Expect`
    /// </remarks>
    [TestFixture]
    [Category("Integration")]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor)]
    public class InGameScenarioTest
    {
        private const string InputTracesPath = "Assets/RoguelikeExample/Tests/TestData/InputTraces";

        [TestCase("Keyboard_400.inputtrace", "400")]
        public async Task インゲームのシナリオテスト_InputTraceを再生_地下2階に到達すること(string path, string seed)
        {
#if UNITY_EDITOR
            // キーボード・マウス操作ではGameビューにフォーカスを移さないと動作しない
            var assembly = Assembly.Load("UnityEditor.dll");
            var gameView = assembly.GetType("UnityEditor.GameView");
            EditorWindow.FocusWindowIfItsOpen(gameView);
#endif
            // フレームレートをキャプチャ環境に合わせる（Updateで入力を処理しているとき必要）
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            await SceneManager.LoadSceneAsync("Dungeon");

            var dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            dungeonManager.randomSeed = seed; // 擬似乱数シードを固定

            using var eventTrace = new InputEventTrace();
            eventTrace.ReadFrom(Path.GetFullPath(Path.Combine(InputTracesPath, path)));

            var isFinished = false;

            using var replayController = eventTrace.Replay()
                .OnFinished(() => { isFinished = true; }) // 再生終了したらフラグを立てる
                .PlayAllEventsAccordingToTimestamps(); // タイムスタンプ基準で再生
            // Note: スペックの異なるマシンで再生する場合、PlayAllEventsAccordingToTimestampsを使用するほうが安定します。
            // 同一マシンでキャプチャ・プレイバックするのであれば、フレームに忠実に再生するPlayAllFramesOneByOneでも再生できます。

            while (!isFinished)
            {
                await UniTask.NextFrame();
            }

            Assert.That(dungeonManager.level, Is.EqualTo(2));
        }
    }
}
