// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey;
using UnityEngine.SceneManagement;

namespace RoguelikeExample.IntegrationTests
{
    /// <summary>
    /// アウトゲームのモンキーテスト（統合テスト）
    ///
    /// 一定時間でたらめな操作をします。
    /// テスト失敗と判断されるのは次の2パターン
    /// - ログにエラー（プロダクトコードに仕込んだ UnityEngine.Assertions.Assert を含む）が出力されたとき
    /// - 一定時間操作できるコンポーネントがないとき
    /// </summary>
    [TestFixture]
    [Category("Integration")]
    public class OutGameMonkeyTest
    {
        [SetUp]
        public async Task SetUp()
        {
            await SceneManager.LoadSceneAsync("Title");
        }

        [Test]
        public async Task アウトゲームのモンキーテスト()
        {
            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMinutes(1), // 1分間動作
                DelayMillis = 200, // 操作間隔は200ms
                SecondsToErrorForNoInteractiveComponent = 5, // 5秒間操作できるコンポーネントがないときエラー扱い
            };

            using var cancellationTokenSource = new CancellationTokenSource();
            await Monkey.Run(config, cancellationTokenSource.Token);
        }
    }
}
