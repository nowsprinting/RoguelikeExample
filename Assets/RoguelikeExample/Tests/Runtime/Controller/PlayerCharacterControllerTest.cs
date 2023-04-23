// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities;
using RoguelikeExample.Entities.ScriptableObjects;
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
        /// - SetUpが異なるため高速移動のテスト <see cref="Run"/> と分けています
        /// - キーごとにテストメソッドが分かれている（パラメタライズドテストのパラメーターにしていない）のは、<c>InputControl</c>がstaticでないためです
        /// </remarks>
        [TestFixture, Timeout(5000)]
        public class MoveAndAttack
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

                var scene = SceneManager.CreateScene(nameof(PlayerCharacterControllerTest));
                SceneManager.SetActiveScene(scene);

                _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
                _enemyManager = new GameObject().AddComponent<EnemyManager>();
                _turn = new Turn();

                _enemyManager.Initialize(new RandomImpl(), _playerCharacterController);
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

                yield return SceneManager.UnloadSceneAsync(nameof(PlayerCharacterControllerTest));
            }

            [Test]
            public async Task Hキー入力で左に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.hKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 2)));
            }

            [Test]
            public async Task Jキー入力で下に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.jKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 3)));
            }

            [Test]
            public async Task Kキー入力で上に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.kKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 1)));
            }

            [Test]
            public async Task Lキー入力で右に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 2)));
            }

            [Test]
            public async Task Yキー入力で左上に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.yKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 1)));
            }

            [Test]
            public async Task Uキー入力で右上に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.uKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            }

            [Test]
            public async Task Bキー入力で左下に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.bKey);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 3)));
            }

            [Test]
            public async Task Nキー入力で右下に移動()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.nKey);
                await WaitForNextPlayerPhase(_turn);

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
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 2)), "移動している");
                Assert.That(_turn.TurnCount, Is.EqualTo(2), "ターンが加算されている");
            }

            [Test]
            public async Task 連続移動は移動しただけターン加算される()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 1);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.lKey); // 押しっぱなし
                await WaitForNextPlayerPhase(_turn);
                await WaitForNextPlayerPhase(_turn); // 2単位連続で移動

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
                Assert.That(_turn.TurnCount, Is.EqualTo(3));
            }

            [Test]
            public async Task スペースキーで攻撃_空振りでもターンが加算される()
            {
                _playerCharacterController.SetPositionFromMapLocation(2, 2);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.PressAndRelease(keyboard.spaceKey); // 攻撃（空振り）
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_turn.TurnCount, Is.EqualTo(2));
            }

            [Test]
            public async Task スペースキーで攻撃_攻撃対象にダメージ()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 1);
                _playerCharacterController.Status = new PlayerStatus(1, 0, 3);
                var enemy = CreateEnemy((1, 2), 10, 1,
                    enemyManager: _enemyManager, playerCharacterController: _playerCharacterController);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.jKey); // 方向を変えるための空移動
                await UniTask.DelayFrame(2); // ターンは消費しない

                _input.PressAndRelease(keyboard.spaceKey); // 攻撃
                await WaitForNextPlayerPhase(_turn);

                Assert.That(enemy.Status.HitPoint, Is.EqualTo(10 - (3 - 1))); // HPが2減っている
            }

            [Test]
            public async Task スペースキーで攻撃_攻撃対象のヒットポイントが0になる_対象インスタンスは破棄され報酬を得る()
            {
                _playerCharacterController.SetPositionFromMapLocation(1, 1);
                _playerCharacterController.Status = new PlayerStatus(1, 0, 3);
                var enemy = CreateEnemy((1, 2), 1, 1, 3, 5,
                    enemyManager: _enemyManager, playerCharacterController: _playerCharacterController);

                var keyboard = InputSystem.AddDevice<Keyboard>();
                _input.Press(keyboard.jKey); // 方向を変えるための空移動
                await UniTask.DelayFrame(2); // ターンは消費しない

                _input.PressAndRelease(keyboard.spaceKey); // 攻撃
                await WaitForNextPlayerPhase(_turn);

                Assert.That(enemy.Status.HitPoint, Is.EqualTo(0), "敵HPは0"); // オーバーキルだが0になる
                Assert.That(enemy.Status.IsAlive, Is.False, "対象は破壊された");
                Assert.That((bool)enemy, Is.False, "対象インスタンスは破棄されている");
                Assert.That(_playerCharacterController.Status.Exp, Is.EqualTo(3), "経験値が加算される");
                Assert.That(_playerCharacterController.Status.Gold, Is.EqualTo(5), "通貨が加算される");
            }
        }

        /// <summary>
        /// 高速移動のテスト
        /// </summary>
        /// <remarks>
        /// - SetUpが異なるため通常移動と攻撃のテスト <see cref="MoveAndAttack"/> と分けています
        /// </remarks>
        [TestFixture, Timeout(5000)]
        public class Run
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

                var scene = SceneManager.CreateScene(nameof(PlayerCharacterControllerTest));
                SceneManager.SetActiveScene(scene);

                _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
                _enemyManager = new GameObject().AddComponent<EnemyManager>();
                _turn = new Turn();

                _enemyManager.Initialize(new RandomImpl(), _playerCharacterController);
                // Note: NewLevel()を呼ばなければ敵キャラクターは生成されない

                _playerCharacterController.runAnimationMillis = 0; // 高速移動時アニメーション時間を0に
                _playerCharacterController.Initialize(new RandomImpl(), _turn, _enemyManager);
                // テストごとにマップが異なるため <c>_playerCharacterController.NewLevel</c> は個々のテストメソッドで実行する
            }

            [UnityTearDown]
            public IEnumerator TearDown()
            {
                _input.TearDown();

                yield return SceneManager.UnloadSceneAsync(nameof(PlayerCharacterControllerTest));
            }

            [Test]
            public async Task Controlキー同時押し_通路がない_突き当りまで高速移動()
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
                await WaitForNextPlayerPhase(_turn);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "2単位移動している");
                Assert.That(_turn.State, Is.EqualTo(TurnState.PlayerIdol), "Runを抜けてPlayerIdolフェーズ");
            }

            [Test]
            public async Task Shiftキー同時押し_通路がない_突き当りまで高速移動()
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
                await WaitForNextPlayerPhase(_turn);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "2単位移動している");
                Assert.That(_turn.State, Is.EqualTo(TurnState.PlayerIdol), "Runを抜けてPlayerIdolフェーズ");
            }

            [Test]
            public async Task Shiftと斜め移動キー同時に押し_斜めには高速移動せず1単位だけ移動()
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
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 2)), "1単位だけ移動している");
                Assert.That(_turn.State, Is.EqualTo(TurnState.PlayerIdol), "Runを抜けてPlayerIdolフェーズ");
            }

            [Test]
            public async Task 高速移動の途中に上り階段がある_階段で停止()
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
                await WaitForNextPlayerPhase(_turn);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "階段まで移動している");
                Assert.That(_turn.State, Is.EqualTo(TurnState.PlayerIdol), "Runを抜けてPlayerIdolフェーズ");
            }

            [Test]
            public async Task 高速移動の途中に下り階段がある_階段で停止()
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
                await WaitForNextPlayerPhase(_turn);
                await WaitForNextPlayerPhase(_turn);

                Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)), "階段まで移動している");
                Assert.That(_turn.State, Is.EqualTo(TurnState.PlayerIdol), "Runを抜けてPlayerIdolフェーズ");
            }

            [Ignore("未実装")]
            [Test]
            public async Task 高速移動の途中で敵に当たる_敵の手前で停止()
            {
                Assert.Fail();
            }

            [Ignore("敵の攻撃が未実装")]
            [Test]
            public async Task 高速移動の途中で敵から攻撃を受ける_その場で停止()
            {
                Assert.Fail();
            }

            [Ignore("未実装")]
            [Test]
            public async Task 通路で高速移動_部屋に入る手前で停止()
            {
                Assert.Fail();
            }

            [Ignore("未実装")]
            [Test]
            public async Task 通路で高速移動_分岐あり_分岐で停止()
            {
                Assert.Fail();
            }

            [Ignore("未実装")]
            [Test]
            public async Task 部屋で高速移動_通路に出る手前で停止()
            {
                // TODO: 指定方向の通路を探して、そっちに向かう

                Assert.Fail();
            }
        }

        private static EnemyCharacterController CreateEnemy(
            (int column, int row) location,
            int hitPoint = 1,
            int defence = 0,
            int rewardExp = 0,
            int rewardGold = 0,
            EnemyManager enemyManager = null,
            PlayerCharacterController playerCharacterController = null)
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
                new MapChip[,] { { new() } }, // TODO: mapはnullでもいいのでは？
                location,
                new RandomImpl(),
                enemyManager,
                playerCharacterController
            );

            return enemyCharacterController;
        }

        private static async UniTask WaitForNextPlayerPhase(Turn turn)
        {
            bool IsPlayerPhase() => (turn.State <= TurnState.PlayerAction);

            // まず、プレイヤーフェイズを抜けるまで待つ
            while (IsPlayerPhase())
            {
                await UniTask.NextFrame();
            }

            // 次のプレイヤーフェイズまで待つ
            while (!IsPlayerPhase())
            {
                await UniTask.NextFrame();
            }
        }
    }
}
