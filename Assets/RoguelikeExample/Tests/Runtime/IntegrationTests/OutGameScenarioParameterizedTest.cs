// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GeneratedAutomationTests;
using NUnit.Framework;
using Unity.RecordedPlayback;
using Unity.RecordedTesting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// Automated QAパッケージの *Simple Test*（JSONファイルを読むタイプ）をパラメタライズドテスト対応にしたサンプル
    ///
    /// 実装のポイント
    /// - Async Testにすることでパラメタライズドテストを可能に
    /// - RecordedTestSuite基底クラスおよびRecordedTest属性は使用せず、テストメソッド内でSceneロードを行なうことで、擬似乱数シード値を固定可能に
    /// - Test Report に名前付きでレポートを残すようにReportingManager.CurrentTestNameを設定
    /// </summary>
    /// <remarks>
    /// なお、再生時にGameビューからボタンが見切れているとテストは失敗します。
    /// サンプルでは<c>CanvasScaler</c>によって常に全てのボタンが表示されるようになっています。
    /// </remarks>
    [TestFixture]
    [Category("Integration")]
    public class OutGameScenarioParameterizedTest : AutomatedTestSuite
    {
        [UnitySetUp]
        public override IEnumerator Setup()
        {
            yield return base.Setup();

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // Automated QAのTest Reportに名前付きでレポートを残す
            ReportingManager.CurrentTestName = $"{TestContext.CurrentContext.Test.FullName}";
            ReportingManager.InitializeDataForNewTest();
        }

        [TestCase("recording-credit.json", "Credit")]
        [TestCase("recording-credit-back.json", "Title")]
        [TestCase("recording-exit.json", "Exit")]
        [TestCase("recording-exit-back.json", "Title")]
        [TestCase("recording-option.json", "Option")]
        [TestCase("recording-option-back.json", "Title")]
        [TestCase("recording-ranking.json", "Ranking")]
        [TestCase("recording-ranking-back.json", "Title")]
        [TestCase("recording-start.json", "StageSelect")]
        [TestCase("recording-start-back.json", "Title")]
        public async Task アウトゲームのシナリオテスト_AQA記録ファイルを再生(string recordingJsonPath, string expectedScreenName)
        {
            const string DataPath = "Assets/RoguelikeExample/Tests/TestData/AutomatedQA";

            await SceneManager.LoadSceneAsync("Title");
            // Note: ランダム要素のあるSceneの場合、ここで擬似乱数シード値を固定できる

            await Playback(Path.GetFullPath(Path.Combine(DataPath, recordingJsonPath)));

            // Verify
            var expectedScreen = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.name.Equals(expectedScreenName));
            Assert.That(expectedScreen, Is.Not.Null);
        }

        private static async Task Playback(string recordingJsonPath)
        {
            // RecordedTestSuite.Setup に実装されている初期化をSceneロード後に実行
            RecordedPlaybackController.Instance.Reset();
            RecordedPlaybackPersistentData.SetRecordingMode(RecordingMode.Playback);
            RecordedPlaybackPersistentData.SetRecordingData(File.ReadAllText(Path.GetFullPath(recordingJsonPath)));
            RecordedPlaybackController.Instance.Begin();
            while (!RecordedPlaybackController.Exists() || !RecordedPlaybackController.Instance.IsInitialized())
            {
                await UniTask.NextFrame();
            }

            // Playback
            await RecordedTesting.TestPlayToEnd();
        }
    }
}
