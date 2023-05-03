// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using GeneratedAutomationTests;
using NUnit.Framework;
using Unity.RecordedTesting;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// アウトゲームのシナリオテスト（統合テスト）
    ///
    /// Automated QAパッケージのRecorded Playback機能を使用して、あらかじめ記録した操作を再生しています。
    /// 再生完了後、インゲームに遷移していることを検証しています。
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class OutGameScenarioTest : RecordedTestSuite
    {
        [UnityTest]
        [RecordedTest("RoguelikeExample/Tests/TestData/AutomatedQA/recording.json")]
        public IEnumerator アウトゲームのシナリオテスト_AQA記録ファイルを再生_タイトル画面を一通り遷移してインゲームに遷移()
        {
            yield return RecordedTesting.TestPlayToEnd();

            var activeScene = SceneManager.GetActiveScene();
            Assert.That(activeScene.name, Is.EqualTo("Dungeon"));
        }
    }
}
