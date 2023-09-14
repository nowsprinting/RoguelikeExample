// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Controller;
using RoguelikeExample.Input.CustomComposites;
using RoguelikeExample.Input.CustomProcessors;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RoguelikeExample.Dungeon
{
    [TestFixture, Timeout(5000)]
    [Category("IgnoreCI")] // CI環境ではfpsが低いため、このテストはスキップする
    public class DungeonManagerTest
    {
        private readonly InputTestFixture _input = new InputTestFixture();

        private DungeonManager _dungeonManager;
        private PlayerCharacterController _playerCharacterController;
        private Turn _turn;

        [SetUp]
        public async Task SetUp()
        {
            _input.Setup();
            // Note: プロダクトコードでInputSystemが初期化されるより前に `InputTestFixture.SetUp` を実行する必要がある
            // Note: `InputTestFixture` を継承する書きかたもあるが、SetUp/TearDownと競合するため選択していない

            InputSystem.RegisterBindingComposite<EightDirectionsComposite>();
            InputSystem.RegisterProcessor<SnapVector2Processor>();
            // Note: カスタムComposite, Interaction, Processorを使用しているプロジェクトでは、Setupの後に再Registerする

            await SceneManager.LoadSceneAsync("Dungeon");

            _dungeonManager = Object.FindAnyObjectByType<DungeonManager>();
            _playerCharacterController = _dungeonManager.playerCharacterController;
            _turn = _dungeonManager.Turn;
        }

        [TearDown]
        public async Task TearDown()
        {
            _input.TearDown();
            await UniTask.DelayFrame(10); // オブジェクトの破棄を待つ
        }

        [Test]
        public async Task OpenOnStairsDialog_Escapeキーでキャンセル_レベル移動しない()
        {
            var beforeLevel = _dungeonManager.level;
            var beforeLocation = _playerCharacterController.MapLocation();

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.PressAndRelease(keyboard.spaceKey); // 攻撃（空振り）
            await WaitForOnStairs(_turn);

            _input.PressAndRelease(keyboard.escapeKey); // 選択ダイアログをキャンセル
            await WaitForNextPlayerIdol(_turn);

            Assert.That(_dungeonManager.level, Is.EqualTo(beforeLevel), "レベル移動していない");
            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo(beforeLocation), "PC座標は移動していない");
        }

        [Test]
        public async Task OpenOnStairsDialog_Noを選択_レベル移動しない()
        {
            var beforeLevel = _dungeonManager.level;
            var beforeLocation = _playerCharacterController.MapLocation();

            var gamepad = InputSystem.AddDevice<Gamepad>();
            _input.PressAndRelease(gamepad.buttonSouth); // 攻撃（空振り）
            await WaitForOnStairs(_turn);

            _input.Set(gamepad.leftStick, Vector2.down); // Noを選択
            await UniTask.NextFrame();

            _input.PressAndRelease(gamepad.buttonSouth); // 決定
            await WaitForNextPlayerIdol(_turn);

            Assert.That(_dungeonManager.level, Is.EqualTo(beforeLevel), "レベル移動していない");
            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo(beforeLocation), "PC座標は移動していない");
        }

        [Test]
        public async Task OpenOnStairsDialog_上り階段でYesを選択_レベルを1つ戻る()
        {
            _dungeonManager.level = 2;

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.PressAndRelease(keyboard.spaceKey); // 攻撃（空振り）
            await WaitForOnStairs(_turn);

            _input.PressAndRelease(keyboard.spaceKey); // Yesを選択
            await WaitForNextPlayerIdol(_turn);

            Assert.That(_dungeonManager.level, Is.EqualTo(1), "レベルを1つ戻っている");
            Assert.That(_playerCharacterController.OnDownStairs, Is.True, "下り階段にいる");
        }

        [Test]
        public async Task OpenOnStairsDialog_下り階段でYesを選択_レベルを1つ進む()
        {
            _dungeonManager.level = 2;

            // 一度レベル2→1に移動して、下り階段にいる状態にする
            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.PressAndRelease(keyboard.spaceKey); // 攻撃（空振り）
            await WaitForOnStairs(_turn);
            _input.PressAndRelease(keyboard.spaceKey); // Yesを選択
            await WaitForNextPlayerIdol(_turn);
            Assume.That(_dungeonManager.level, Is.EqualTo(1), "レベルを1つ戻っている");
            Assume.That(_playerCharacterController.OnDownStairs, Is.True, "下り階段にいる");

            _input.PressAndRelease(keyboard.spaceKey); // 攻撃（空振り）
            await WaitForOnStairs(_turn);

            _input.PressAndRelease(keyboard.spaceKey); // Yesを選択
            await WaitForNextPlayerIdol(_turn);

            Assert.That(_dungeonManager.level, Is.EqualTo(2), "レベルを1つ進んでいる");
            Assert.That(_playerCharacterController.OnUpStairs, Is.True, "上り階段にいる");
        }

        [Test]
        [Ignore("Result画面が未実装なのでignore")]
        public async Task OpenOnStairsDialog_レベル1の上り階段でYesを選択_Result画面に遷移()
        {
            // TODO: 未実装
        }

        private static async UniTask WaitForNextPlayerIdol(Turn turn)
        {
            // まず、プレイヤーフェイズを抜けるまで待つ
            while (turn.State <= TurnState.PlayerAction)
            {
                await UniTask.NextFrame();
            }

            // 次のIdolまで待つ
            while (turn.State != TurnState.PlayerIdol)
            {
                await UniTask.NextFrame();
            }
        }

        private static async UniTask WaitForOnStairs(Turn turn)
        {
            // まず、プレイヤーフェイズを抜けるまで待つ
            while (turn.State <= TurnState.PlayerAction)
            {
                await UniTask.NextFrame();
            }

            // OnStairsまで待つ
            while (turn.State != TurnState.OnStairs)
            {
                await UniTask.NextFrame();
            }
        }
    }
}
