// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// プレイヤーキャラクター操作のテスト
    /// <c>Unity.InputSystem.TestFramework</c>を使用して入力をシミュレートする例
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Testing.html"/>
    ///
    /// - 使用しているScene（Sandbox.unity）は、Scenes in Buildに登録されていないもの
    /// - Play Modeテストだが、In Editorでのみ実行する
    /// </summary>
    [TestFixture]
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    public class PlayerCharacterControllerTest
    {
        private PlayerCharacterController _playerCharacterController;
        private readonly InputTestFixture _input = new InputTestFixture();

        private const int WaitAfterOperationMillis = 150;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            _input.Setup();
            // Note: プロダクトコードでInputSystemが初期化されるより前に `InputTestFixture.SetUp` を実行する必要がある
            // Note: `InputTestFixture` はSetUp/TearDownと競合するため使用していない

#if UNITY_EDITOR
            // Scenes in Buildに登録されていないSceneかつIn Editorでのみ実行するテストのため、EditorSceneManagerでロード
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/RoguelikeExample/Scenes/Sandbox.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#endif

            _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
            _playerCharacterController._map = MapHelper.CreateFromDumpStrings(new[]
            {
                "00000", //　壁壁壁壁壁
                "01110", // 壁床床床壁
                "01110", //　壁床床床壁
                "01110", //　壁床床壁
                "00000", //　壁壁壁壁壁
            });
        }

        [TearDown]
        public void TearDown()
        {
            _input.TearDown();
        }

        [TestCase(2, 2, 1, 2)]
        [TestCase(1, 2, 1, 2)] // 壁があるので移動しない
        public async Task Hキー入力で左に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.hKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 2, 3)]
        [TestCase(2, 3, 2, 3)] // 壁があるので移動しない
        public async Task Jキー入力で下に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.jKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 2, 1)]
        [TestCase(2, 1, 2, 1)] // 壁があるので移動しない
        public async Task Kキー入力で上に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.kKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 3, 2)]
        [TestCase(3, 2, 3, 2)] // 壁があるので移動しない
        public async Task Lキー入力で右に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.lKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 1, 1)]
        [TestCase(1, 2, 1, 2)] // 壁があるので移動しない
        [TestCase(2, 1, 2, 1)] // 壁があるので移動しない
        public async Task Yキー入力で左上に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.yKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 3, 1)]
        [TestCase(3, 2, 3, 2)] // 壁があるので移動しない
        [TestCase(2, 1, 2, 1)] // 壁があるので移動しない
        public async Task Uキー入力で右上に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.uKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 1, 3)]
        [TestCase(1, 2, 1, 2)] // 壁があるので移動しない
        [TestCase(2, 3, 2, 3)] // 壁があるので移動しない
        public async Task Bキー入力で左下に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.bKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 3, 3)]
        [TestCase(3, 2, 3, 2)] // 壁があるので移動しない
        [TestCase(2, 3, 2, 3)] // 壁があるので移動しない
        public async Task Nキー入力で右下に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.nKey);
            await Task.Delay(WaitAfterOperationMillis);

            Assert.That(_playerCharacterController.GetMapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }
    }
}
