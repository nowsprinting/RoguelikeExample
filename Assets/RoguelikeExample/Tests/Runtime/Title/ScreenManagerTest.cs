// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RoguelikeExample.Title
{
    /// <summary>
    /// アウトゲームの画面遷移のテスト
    ///
    /// <c>Transit</c> 直接メソッドを呼んで検証するのではなく、ボタン操作をシミュレートして実際に画面遷移することを検証しています
    /// </summary>
    [TestFixture]
    public class ScreenManagerTest
    {
        [SetUp]
        public async Task SetUp()
        {
            await SceneManager.LoadSceneAsync("Title");
        }

        [TestCase("Start", "StageSelect")]
        [TestCase("Ranking", "Ranking")]
        [TestCase("Option", "Option")]
        [TestCase("Credit", "Credit")]
        [TestCase("Exit", "Exit")]
        public async Task Transit_ボタンをクリック_指定画面に遷移(string buttonName, string dstScreenName)
        {
            var srcScreen = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.name == "Title");
            Assume.That(srcScreen, Is.Not.Null);

            var button = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.name == buttonName);
            Assume.That(button, Is.Not.Null);

            var eventData = new PointerEventData(EventSystem.current);
            button.OnPointerClick(eventData); // クリックイベントを発生させる
            await UniTask.NextFrame();

            var dstScreen = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                .FirstOrDefault(x => x.name == dstScreenName);
            Assert.That(dstScreen, Is.Not.Null, $"{dstScreenName}画面が表示されている"); // Findで取得できる時点でactive
            Assert.That(srcScreen.activeInHierarchy, Is.False, "遷移元は非活性化している");
        }
    }
}
