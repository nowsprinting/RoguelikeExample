// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Reflection;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace RoguelikeExample.Editor
{
    /// <summary>
    /// テスト開始時にGameビューを開くコールバック実装.
    ///
    ///　一部のIntegration testでは、Gameビューが表示されないと動作が不安定なため、バッチモードであってもGameビューを強制的に開きます。
    /// なお、グラフィックボード・ビデオカードを搭載していないマシン、もしくは`-nographics`オプションを指定しているときは動作しません。
    ///
    /// 詳しくは <see href="https://www.nowsprinting.com/entry/2023/06/14/003659"/> を参照。
    /// </summary>
    /// <remarks>
    /// 解像度は指定していないので、バッチモードではVGA（640x480）で開きます。
    /// 解像度を指定する方法については <see href="https://www.nowsprinting.com/entry/2023/06/25/071828"/> を参照。
    /// </remarks>
    public class OpenGameViewAtRunStarted : ICallbacks
    {
        [InitializeOnLoadMethod]
        private static void SetupTestCallbacks()
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new OpenGameViewAtRunStarted());
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            var assembly = Assembly.Load("UnityEditor.dll");
            var viewClass = Application.isBatchMode
                ? "UnityEditor.GameView"
                : "UnityEditor.PlayModeView";
            var gameView = assembly.GetType(viewClass);
            EditorWindow.GetWindow(gameView, false, null, true);
        }

        public void RunFinished(ITestResultAdaptor result) { }

        public void TestStarted(ITestAdaptor test) { }

        public void TestFinished(ITestResultAdaptor result) { }
    }
}
