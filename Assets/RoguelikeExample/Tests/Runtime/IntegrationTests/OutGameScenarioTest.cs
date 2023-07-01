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
    /// Test Generation機能で *Simple Test* を選択して生成されるコードをベースにしています。
    /// 再生完了後、インゲームに遷移していることを検証するアサーションを追加しています。
    /// </summary>
    /// <remarks>
    /// なお、再生時にGameビューからボタンが見切れているとテストは失敗します。
    /// サンプルでは<c>CanvasScaler</c>によって常に全てのボタンが表示されるようになっています。
    /// </remarks>
    [TestFixture]
    [Category("Integration")]
    public class OutGameScenarioTest : RecordedTestSuite
    {
        [UnityTest]
        [RecordedTest("RoguelikeExample/Tests/TestData/AutomatedQA/recording-all.json")]
        public IEnumerator アウトゲームのシナリオテスト_記録ファイルを再生_各画面を一通り遷移してインゲームに遷移()
        {
            yield return RecordedTesting.TestPlayToEnd();

            var activeScene = SceneManager.GetActiveScene();
            Assert.That(activeScene.name, Is.EqualTo("Dungeon"));
        }
    }
}
