// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities;
using RoguelikeExample.Entities.ScriptableObjects;
using RoguelikeExample.Input.CustomComposites;
using RoguelikeExample.Input.CustomProcessors;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// プレイヤーキャラクター操作・振る舞いのテスト
    ///
    /// 結合度高めのユニットテスト。
    /// <c>Unity.InputSystem.TestFramework</c>を使用して入力をシミュレートする例
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Testing.html"/>
    /// </summary>
    [TestFixture]
    public class PlayerCharacterControllerTest
    {
        /// <summary>
        /// 通常の移動と攻撃のテスト
        /// </summary>
        /// <remarks>
        /// - SetUpが異なるため高速移動のテスト <see cref="RunTest"/> と分けています
        /// - キーごとにテストメソッドが分かれている（パラメタライズドテストのパラメーターにしていない）のは、<c>InputControl</c>がstaticでないためです
        /// </remarks>
        [TestFixture, Timeout(5000)]
        public class MoveAndAttackTest
        {
            private readonly InputTestFixture _input = new InputTestFixture();

            private PlayerCharacterController _playerCharacterController;
            private EnemyManager _enemyManager;
            private Turn _turn;

            [SetUp]
            public void SetUp()
            {
                _input.Setup();
                // Note: プロダクトコードでInputSystemが初期化されるより前に `InputTestFixture.SetUp` を実行する必要がある
                // Note: `InputTestFixture` を継承する書きかたもあるが、SetUp/TearDownと競合するため選択していない

                InputSystem.RegisterBindingComposite<EightDirectionsComposite>();
                InputSystem.RegisterProcessor<SnapVector2Processor>();
                // Note: カスタムComposite, Interaction, Processorを使用しているプロジェクトでは、Setupの後に再Registerする

                var scene = SceneManager.CreateScene(TestContext.CurrentContext.Test.ClassName);
                SceneManager.SetActiveScene(scene);

                _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
                _enemyManager = new GameObject().AddComponent<EnemyManager>();
                _turn = new Turn();

                _enemyManager.Initialize(new RandomImpl(), _turn, _playerCharacterController);
                // Note: NewLevel()を呼ばなければ敵キャラクターは生成されない

                _playerCharacterController.actionAnimationMillis = 0; // 行動アニメーション時間を0に
                _playerCharacterController.Initialize(new RandomImpl(), _turn, _enemyManager);
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "01110", // 壁床床床壁
                        "01110", // 壁床床床壁
                        "01110", // 壁床床床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (0, 0) // 初期位置は仮。各テストケースで設定される
                );
            }

            [UnityTearDown]
            public IEnumerator TearDown()
            {
                _input.TearDown();

                yield return SceneManager.UnloadSceneAsync(TestContext.CurrentContext.Test.ClassName);
            }

            [Test]
            public async Task Hキー入力で左に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.hKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 2)));
            }

            [Test]
            public async Task Jキー入力で下に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.jKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 3)));
            }

            [Test]
            public async Task Kキー入力で上に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.kKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 1)));
            }

            [Test]
            public async Task Lキー入力で右に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 2)));
            }

            [Test]
            public async Task Yキー入力で左上に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.yKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 1)));
            }

            [Test]
            public async Task Uキー入力で右上に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.uKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task Bキー入力で左下に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.bKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 3)));
            }

            [Test]
            public async Task Nキー入力で右下に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.nKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 3)));
            }

            [Test]
            public async Task 壁に向かって移動_移動せずフェーズ遷移もしない()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.hKey);

                for (var i = 0; i < 10; i++)
                {
                    await UniTask.NextFrame();
                    Assert.That(_turn.State, Is.EqualTo(TurnState.PlayerIdol), "PlayerIdolフェーズから遷移しない");
                }

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 2)), "移動しない");
            }

            [Test]
            public async Task 移動するとターンが加算される()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.hKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 2)), "移動している");
                Assert.That(_turn.TurnCount, Is.EqualTo(2), "ターンが加算されている");
            }

            [Test]
            public async Task 連続移動は移動しただけターン加算される()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 1);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 押しっぱなし
                await WaitForNextPlayerIdol(_turn);
                await WaitForNextPlayerIdol(_turn); // 2単位連続で移動

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "移動している");
                Assert.That(_turn.TurnCount, Is.EqualTo(3), "ターンが加算されている");
            }

            [Test]
            public async Task スペースキーで攻撃_攻撃対象にダメージ()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 1);
                _playerCharacterController.Status = new PlayerStatus(1, 0, 3);
                var enemy = CreateEnemy(_turn, _enemyManager, _playerCharacterController, (1, 2), 10, 1);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.jKey); // 方向を変えるための空移動
                await UniTask.DelayFrame(2); // ターンは消費しない

                _input.PressAndRelease(keyboard.spaceKey); // 攻撃
                await WaitForNextPlayerIdol(_turn);

                Assert.That(enemy.Status.HitPoint, Is.EqualTo(10 - (3 - 1))); // HPが2減っている
            }

            [Test]
            public async Task スペースキーで攻撃_攻撃対象のヒットポイントが0になる_対象インスタンスは破棄され報酬を得る()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 1);
                _playerCharacterController.Status = new PlayerStatus(1, 0, 3);
                var enemy = CreateEnemy(_turn, _enemyManager, _playerCharacterController, (1, 2), 1, 1, 3, 5);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.jKey); // 方向を変えるための空移動
                await UniTask.DelayFrame(2); // ターンは消費しない

                _input.PressAndRelease(keyboard.spaceKey); // 攻撃
                await WaitForNextPlayerIdol(_turn);

                Assert.That(enemy.Status.HitPoint, Is.EqualTo(0), "敵HPは0"); // オーバーキルだが0になる
                Assert.That(enemy.Status.IsAlive, Is.False, "対象は破壊された");
                Assert.That((bool)enemy, Is.False, "対象インスタンスは破棄されている");
                Assert.That(_playerCharacterController.Status.Exp, Is.EqualTo(3), "経験値が加算される");
                Assert.That(_playerCharacterController.Status.Gold, Is.EqualTo(5), "通貨が加算される");
                await UniTask.NextFrame(); // オブジェクトの破棄を待つ
            }

            [Test]
            public async Task スペースキーで攻撃_空振りでもターンが加算される()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.PressAndRelease(keyboard.spaceKey); // 攻撃（空振り）
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_turn.TurnCount, Is.EqualTo(2));
            }

            [Test]
            public async Task ゲームパッド左スティックで移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var gamepad = InputSystem.AddDevice<Gamepad>();
                _input.Set(gamepad.leftStick, new Vector2(-0.9f, -0.1f)); // 誤差はSnapDirectionVector2Processorがスナップしてくれる
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 2)));
            }

            [Test]
            public async Task ゲームパッド左スティックで斜め移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var gamepad = InputSystem.AddDevice<Gamepad>();
                _input.Set(gamepad.leftStick, new Vector2(-0.9f, 0.9f)); // 誤差はSnapDirectionVector2Processorがスナップしてくれる
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 1)));
            }

            [Test]
            public async Task ゲームパッドSouthボタンで攻撃()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var gamepad = InputSystem.AddDevice<Gamepad>();
                _input.PressAndRelease(gamepad.buttonSouth); // 攻撃（空振り）
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_turn.TurnCount, Is.EqualTo(2));
            }
        }

        /// <summary>
        /// 高速移動のテスト
        /// </summary>
        /// <remarks>
        /// - SetUpが異なるため通常移動と攻撃のテスト <see cref="MoveAndAttackTest"/> と分けています
        /// </remarks>
        [TestFixture, Timeout(5000)]
        public class RunTest
        {
            private readonly InputTestFixture _input = new InputTestFixture();

            private PlayerCharacterController _playerCharacterController;
            private EnemyManager _enemyManager;
            private Turn _turn;

            [SetUp]
            public void SetUp()
            {
                _input.Setup();
                // Note: プロダクトコードでInputSystemが初期化されるより前に `InputTestFixture.SetUp` を実行する必要がある
                // Note: `InputTestFixture` を継承する書きかたもあるが、SetUp/TearDownと競合するため選択していない

                InputSystem.RegisterBindingComposite<EightDirectionsComposite>();
                InputSystem.RegisterProcessor<SnapVector2Processor>();
                // Note: カスタムComposite, Interaction, Processorを使用しているプロジェクトでは、Setupの後に再Registerする

                var scene = SceneManager.CreateScene(TestContext.CurrentContext.Test.ClassName);
                SceneManager.SetActiveScene(scene);

                _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
                _enemyManager = new GameObject().AddComponent<EnemyManager>();
                _turn = new Turn();

                _enemyManager.Initialize(new RandomImpl(), _turn, _playerCharacterController);
                // Note: NewLevel()を呼ばなければ敵キャラクターは生成されない

                _playerCharacterController.runAnimationMillis = 0; // 高速移動時アニメーション時間を0に
                _playerCharacterController.Initialize(new RandomImpl(), _turn, _enemyManager);
                // テストごとにマップが異なるため <c>_playerCharacterController.NewLevel</c> は個々のテストメソッドで実行する
            }

            [UnityTearDown]
            public IEnumerator TearDown()
            {
                _input.TearDown();

                yield return SceneManager.UnloadSceneAsync(TestContext.CurrentContext.Test.ClassName);
            }

            [Test]
            public async Task Controlキー同時押し_通路がない部屋_突き当りまで高速移動()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "01110", // 壁床床床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task Shiftキー同時押し_通路がない部屋_突き当りまで高速移動()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "01110", // 壁床床床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.shiftKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.shiftKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task Shiftと斜め移動キー同時押し_斜めには高速移動せず1単位だけ移動()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "01110", // 壁床床床壁
                        "01110", // 壁床床床壁
                        "01110", // 壁床床床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.nKey); // 右下
                _input.Press(keyboard.shiftKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.nKey); // 離す
                _input.Release(keyboard.shiftKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 2)));
            }

            [Test]
            public async Task 高速移動の途中に上り階段_階段で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "000000", // 壁壁壁壁壁壁
                        "011310", // 壁床床上床壁
                        "000000", // 壁壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.shiftKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.shiftKey);
                await WaitForOnStairs(_turn); // 階段ダイアログ表示を待つ

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "階段で停止");
                Assert.That(_turn.IsRun, Is.False, "Runモードは解除されている");
            }

            [Test]
            public async Task 高速移動の途中に下り階段_階段で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "000000", // 壁壁壁壁壁壁
                        "011410", // 壁床床下床壁
                        "000000", // 壁壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.shiftKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.shiftKey);
                await WaitForOnStairs(_turn); // 階段ダイアログ表示を待つ

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "階段で停止");
                Assert.That(_turn.IsRun, Is.False, "Runモードは解除されている");
            }

            [Test]
            public async Task 部屋で高速移動_左に通路_通路に出る手前で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "21110", // 通床床床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (3, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.hKey); // 左
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.hKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 1)));
            }

            [Test]
            public async Task 部屋で高速移動_右に通路_通路に出る手前で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "01112", // 壁床床床通
                        "00000", // 壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task 部屋で高速移動_上に通路_通路に出る手前で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "000200", // 壁壁壁通壁壁
                        "011110", // 壁床床床床壁
                        "000000", // 壁壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task 部屋で高速移動_下に通路_通路に出る手前で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "000000", // 壁壁壁壁壁壁
                        "011110", // 壁床床床床壁
                        "000200", // 壁壁壁通壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task 通路で高速移動_部屋に入ったところで停止() // Note: 部屋から通路に出る手前で止まるのと同じロジック
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "02220", // 壁通通通壁
                        "00020", // 壁壁壁通壁
                        "00010", // 壁壁壁床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 3)));
            }

            [Test]
            public async Task 通路で高速移動_分岐あり_分岐で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "02220", // 壁通通通壁
                        "00020", // 壁壁壁通壁
                        "00220", // 壁壁通通壁
                        "00020", // 壁壁壁通壁
                    }),
                    (1, 1)
                );

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 3)));
            }

            [Test]
            public async Task 高速移動の途中で敵に当たる_敵の手前で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "000000", // 壁壁壁壁壁壁
                        "011110", // 壁床床床床壁
                        "000000", // 壁壁壁壁壁壁
                    }),
                    (1, 1)
                );

                CreateEnemy(_turn, _enemyManager, _playerCharacterController, (4, 1));

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Ignore("敵の攻撃が未実装")]
            [Test]
            public async Task 高速移動の途中で敵から攻撃を受ける_その場で停止()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "000000", // 壁壁壁壁壁壁
                        "011110", // 壁床床床床壁
                        "000020", // 壁壁壁壁通壁
                        "000000", // 壁壁壁壁壁壁
                    }),
                    (1, 1)
                );

                CreateEnemy(_turn, _enemyManager, _playerCharacterController, (4, 2)); // (3, 1) は攻撃範囲
                // TODO: 移動しないが攻撃はするAIをセットしないとだめ

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 右
                _input.Press(keyboard.ctrlKey);
                await UniTask.DelayFrame(2);

                _input.Release(keyboard.lKey); // 離す
                _input.Release(keyboard.ctrlKey);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task ゲームパッドEastボタン同時押し_通路がない部屋_突き当りまで高速移動()
            {
                _playerCharacterController.NewLevel(
                    MapHelper.CreateFromDumpStrings(new[]
                    {
                        "00000", // 壁壁壁壁壁
                        "01110", // 壁床床床壁
                        "00000", // 壁壁壁壁壁
                    }),
                    (1, 1)
                );

                var gamepad = InputSystem.AddDevice<Gamepad>();
                _input.Set(gamepad.leftStick, Vector2.right); // 左スティックを右に
                _input.Press(gamepad.buttonEast);
                await UniTask.DelayFrame(2);

                _input.Set(gamepad.leftStick, Vector2.zero); // 離す
                _input.Release(gamepad.buttonEast);
                await WaitForNextPlayerIdol(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }
        }

        private static EnemyCharacterController CreateEnemy(
            Turn turn,
            EnemyManager enemyManager,
            PlayerCharacterController playerCharacterController,
            (int column, int row) location,
            int hitPoint = 1,
            int defence = 0,
            int rewardExp = 0,
            int rewardGold = 0)
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.maxHitPoint = hitPoint;
            enemyRace.defense = defence;
            enemyRace.rewardExp = rewardExp;
            enemyRace.rewardGold = rewardGold;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.transform.parent = enemyManager.transform;
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                null,
                location,
                new RandomImpl(),
                turn,
                enemyManager,
                playerCharacterController
            );

            return enemyCharacterController;
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
