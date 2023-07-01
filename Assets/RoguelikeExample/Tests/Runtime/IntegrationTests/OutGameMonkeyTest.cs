// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using TestHelper.Monkey;
using TestHelper.Monkey.Random;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2023_1_OR_NEWER
using UnityEngine;
#else
using Cysharp.Threading.Tasks;
#endif

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
        [Test]
        public async Task アウトゲームのモンキーテスト()
        {
            var random = new RandomImpl(); // 擬似乱数生成器
            Debug.Log($"Using {random}"); // シード値を出力（再現可能にするため）

            await SceneManager.LoadSceneAsync("Title");
            // Note: ランダム要素のあるSceneの場合、擬似乱数シードにrandom.Next()を使用すると再現に必要なシード値が1つで済んで便利です

            var config = new MonkeyConfig
            {
                Lifetime = TimeSpan.FromMinutes(1), // 1分間動作
                DelayMillis = 200, // 操作間隔は200ms
                SecondsToErrorForNoInteractiveComponent = 5, // 5秒間操作できるコンポーネントがないときエラー扱い
                Random = random, // 擬似乱数生成器を指定
            };

            await Monkey.Run(config);
        }
    }
}
