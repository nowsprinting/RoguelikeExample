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
    /// プレイヤーキャラクター操作・振る舞いのテスト（結合度高め）
    ///
    /// <c>Unity.InputSystem.TestFramework</c>を使用して入力をシミュレートする例
    /// <see href="https://docs.unity3d.com/Packages/com.unity.inputsystem@1.5/manual/Testing.html"/>
    /// </summary>
    [TestFixture]
    public class PlayerCharacterControllerTest
    {
        private readonly InputTestFixture _input = new InputTestFixture();
        private PlayerCharacterController _playerCharacterController;
        private EnemyManager _enemyManager;

        [SetUp]
        public void SetUp()
        {
            _input.Setup();
            // Note: プロダクトコードでInputSystemが初期化されるより前に `InputTestFixture.SetUp` を実行する必要がある
            // Note: `InputTestFixture` を継承する方法は、SetUp/TearDownと競合するため使用していない

            var scene = SceneManager.CreateScene(nameof(PlayerCharacterControllerTest));
            SceneManager.SetActiveScene(scene);

            _enemyManager = new GameObject().AddComponent<EnemyManager>();

            _playerCharacterController = new GameObject().AddComponent<PlayerCharacterController>();
            _playerCharacterController._actionAnimationMillis = 0; // 行動アニメーション時間を0に
            _playerCharacterController.NewLevel(
                MapHelper.CreateFromDumpStrings(new[]
                {
                    "00000", // 壁壁壁壁壁
                    "01110", // 壁床床床壁
                    "01110", // 壁床床床壁
                    "01110", // 壁床床床壁
                    "00000", // 壁壁壁壁壁
                }),
                (0, 0), // 初期位置は仮。各テストケースで設定される
                _enemyManager
            );
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _input.TearDown();

            yield return SceneManager.UnloadSceneAsync(nameof(PlayerCharacterControllerTest));
        }

        [TestCase(2, 2, 1, 2)]
        [TestCase(1, 2, 1, 2)] // 壁があるので移動しない
        public async Task Hキー入力で左に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.hKey);
            await UniTask.DelayFrame(2); // PCと敵の行動で（アニメーション0でも）2フレーム待つ

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 2, 3)]
        [TestCase(2, 3, 2, 3)] // 壁があるので移動しない
        public async Task Jキー入力で下に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.jKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 2, 1)]
        [TestCase(2, 1, 2, 1)] // 壁があるので移動しない
        public async Task Kキー入力で上に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.kKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 3, 2)]
        [TestCase(3, 2, 3, 2)] // 壁があるので移動しない
        public async Task Lキー入力で右に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.lKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 1, 1)]
        [TestCase(1, 2, 1, 2)] // 壁があるので移動しない
        [TestCase(2, 1, 2, 1)] // 壁があるので移動しない
        public async Task Yキー入力で左上に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.yKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 3, 1)]
        [TestCase(3, 2, 3, 2)] // 壁があるので移動しない
        [TestCase(2, 1, 2, 1)] // 壁があるので移動しない
        public async Task Uキー入力で右上に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.uKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 1, 3)]
        [TestCase(1, 2, 1, 2)] // 壁があるので移動しない
        [TestCase(2, 3, 2, 3)] // 壁があるので移動しない
        public async Task Bキー入力で左下に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.bKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [TestCase(2, 2, 3, 3)]
        [TestCase(3, 2, 3, 2)] // 壁があるので移動しない
        [TestCase(2, 3, 2, 3)] // 壁があるので移動しない
        public async Task Nキー入力で右下に移動(int startColumn, int startRow, int expectedColumn, int expectedRow)
        {
            _playerCharacterController.SetPositionFromMapLocation(startColumn, startRow);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.nKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((expectedColumn, expectedRow)));
        }

        [Test]
        public async Task 壁に向かって移動しようとしたときはターン加算されない()
        {
            _playerCharacterController.SetPositionFromMapLocation(1, 1);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.hKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((1, 1)));
            Assert.That(_playerCharacterController.Status.Turn, Is.EqualTo(0));
        }

        [Test]
        public async Task 移動するとターン加算される()
        {
            _playerCharacterController.SetPositionFromMapLocation(1, 1);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.lKey);
            await UniTask.DelayFrame(2);
            _input.Release(keyboard.lKey);
            await UniTask.DelayFrame(2);

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((2, 1)));
            Assert.That(_playerCharacterController.Status.Turn, Is.EqualTo(1));
        }

        [Test]
        public async Task 連続移動は移動しただけターン加算される()
        {
            _playerCharacterController.SetPositionFromMapLocation(1, 1);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.lKey); // 押しっぱなし
            await UniTask.DelayFrame(4); // 2単位移動するので2x2=4フレーム待つ

            Assert.That(_playerCharacterController.MapLocation(), Is.EqualTo((3, 1)));
            Assert.That(_playerCharacterController.Status.Turn, Is.EqualTo(2));
        }

        [Test]
        public async Task スペースキーで攻撃_攻撃対象にダメージ()
        {
            _playerCharacterController.SetPositionFromMapLocation(1, 1);
            _playerCharacterController.Status = new PlayerStatus(1, 0, 3);
            var enemy = CreateEnemy(10, 1, (1, 2));

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.jKey); // 方向を変えるための空移動
            await UniTask.DelayFrame(2);

            _input.PressAndRelease(keyboard.spaceKey); // 攻撃
            await UniTask.DelayFrame(2);

            Assert.That(enemy.Status.HitPoint, Is.EqualTo(10 - (3 - 1))); // HPが2減っている
        }

        [Test]
        public async Task スペースキーで攻撃_攻撃対象のヒットポイントが0_敵インスタンスは破棄され報酬を得る()
        {
            _playerCharacterController.SetPositionFromMapLocation(1, 1);
            _playerCharacterController.Status = new PlayerStatus(1, 0, 3);
            var enemy = CreateEnemy(1, 1, (1, 2), 3, 5);

            var keyboard = InputSystem.AddDevice<Keyboard>();
            _input.Press(keyboard.jKey); // 方向を変えるための空移動
            await UniTask.DelayFrame(2);

            _input.PressAndRelease(keyboard.spaceKey); // 攻撃
            await UniTask.DelayFrame(2);
            await UniTask.DelayFrame(2); // 破壊演出分

            Assert.That(enemy.Status.HitPoint, Is.EqualTo(0), "敵HPは0（オーバーキルでも0）");
            Assert.That(enemy.Status.IsAlive, Is.False, "敵は破壊された");
            Assert.That((bool)enemy, Is.False, "敵インスタンスは破棄されている");
            Assert.That(_playerCharacterController.Status.Exp, Is.EqualTo(3), "経験値");
            Assert.That(_playerCharacterController.Status.Gold, Is.EqualTo(5), "Gold");
        }

        private EnemyCharacterController CreateEnemy(int hitPoint, int defence, (int column, int row) location,
            int rewardExp = 0, int rewardGold = 0)
        {
            var enemyRace = ScriptableObject.CreateInstance<EnemyRace>();
            enemyRace.maxHitPoint = hitPoint;
            enemyRace.defense = defence;
            enemyRace.rewardExp = rewardExp;
            enemyRace.rewardGold = rewardGold;

            var enemyCharacterController = new GameObject().AddComponent<EnemyCharacterController>();
            enemyCharacterController.transform.parent = _enemyManager.transform;
            enemyCharacterController.Initialize(
                enemyRace,
                1,
                new RandomImpl(),
                new MapChip[,] { { new() } },
                location
            );

            return enemyCharacterController;
        }
    }
}
