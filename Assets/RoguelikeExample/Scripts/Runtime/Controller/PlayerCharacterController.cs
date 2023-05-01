// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities;
using RoguelikeExample.Input;
using RoguelikeExample.Random;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// プレイヤーキャラクターのコントローラー
    /// </summary>
    public sealed class PlayerCharacterController : CharacterController
    {
        [SerializeField, Tooltip("行動アニメーション時間")]
        internal int actionAnimationMillis = 100;

        [SerializeField, Tooltip("高速移動時アニメーション時間")]
        internal int runAnimationMillis = 10;

        public PlayerStatus Status { get; internal set; } = new PlayerStatus(10, 1, 3);

        // インゲーム開始時に <c>DungeonManager</c> から設定されるもの（テストでは省略されることもある）
        private EnemyManager _enemyManager;

        private PlayerInputActions _inputActions;
        private Direction _direction;

        /// <summary>
        /// インゲーム開始時に <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="random">このキャラクターが消費する擬似乱数発生器インスタンス</param>
        /// <param name="turn">行動ターンのステートマシン（DungeonManagerが生成したインスタンス）</param>
        /// <param name="enemyManager">敵キャラクター管理</param>
        public void Initialize(IRandom random, Turn turn, EnemyManager enemyManager = null)
        {
            _enemyManager = enemyManager;
            _random = random;
            _turn = turn;
            _turn.OnPhaseTransition += HandlePhaseTransition;
        }

        /// <summary>
        /// 新しいレベルに移動したときに <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="map">当該レベルのマップ</param>
        /// <param name="startLocation">キャラクターの初期位置</param>
        public void NewLevel(MapChip[,] map, (int colum, int row) startLocation)
        {
            _map = map;
            SetPositionFromMapLocation(startLocation.colum, startLocation.row);
        }

        /// <summary>
        /// 行動アニメーション時間（高速移動中かどうかを考慮した値）
        /// </summary>
        public int AnimationMillis() => _turn.IsRun ? runAnimationMillis : actionAnimationMillis;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Attack.performed += AttackOperation;
        }

        private void OnEnable()
        {
            _inputActions?.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Disable();
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();

            if (_turn != null)
            {
                _turn.OnPhaseTransition -= HandlePhaseTransition;
            }
        }

        private void HandlePhaseTransition(object sender, EventArgs _)
        {
            var turn = (Turn)sender;
            switch (turn.State)
            {
                case TurnState.PlayerRun:
                    ThinkToRun(turn);
                    break;
                case TurnState.PlayerAction:
                    DoAction(turn).Forget();
                    break;
            }
        }

        private void Update()
        {
            if (_turn.State != TurnState.PlayerIdol)
            {
                return; // PlayerIdol以外は入力を受け付けない
            }

            // Move (hjklyubn, stick)
            var direction = DirectionExtensions.FromVector2(_inputActions.Player.Move.ReadValue<Vector2>());
            if (direction != Direction.None)
            {
                MoveOperation(direction);
            }
        }

        private void MoveOperation(Direction direction)
        {
            _direction = direction; // 移動できないときも方向だけは反映するので、early returnしない

            var location = MapLocation();
            (int column, int row) dest = (location.column + direction.X(), location.row + direction.Y());

            if (_map.IsWall(dest.column, dest.row))
            {
                return; // 移動先が壁
            }

            if (_enemyManager.ExistEnemy(dest) != null)
            {
                return; // 移動先に敵がいる場合は移動しない（攻撃はspaceキー）
            }

            NextLocation = dest;

            _turn.IsRun = _inputActions.Player.Run.ReadValue<float>() > 0 && !direction.IsDiagonal(); // 次ターンから高速移動
            _turn.NextPhase().Forget();
        }

        private void AttackOperation(InputAction.CallbackContext context)
        {
            if (_turn.State != TurnState.PlayerIdol)
            {
                return; // PlayerIdol以外は入力を受け付けない
            }

            _turn.NextPhase().Forget(); // 空振りでもフェーズを送る
        }

        private void ThinkToRun(Turn turn)
        {
            Assert.IsTrue(turn.IsRun);

            var location = MapLocation();
            if (IsStopLocation(_map, location))
            {
                turn.CanselRun(); // 現在地が停止条件を満たすとき、高速移動をキャンセルしてプレイヤー操作に戻る
                return;
            }

            var newDirection = MovableDirection(_map, location, _direction);
            if (newDirection == Direction.None)
            {
                _turn.CanselRun(); // 移動先がないとき、高速移動をキャンセルしてプレイヤー操作に戻る
                return;
            }
            else if (_map.IsCorridor(location.column, location.row))
            {
                _direction = newDirection; // 通路では無制限に方向転換
            }
            else if (_map.IsRoom(location.column, location.row))
            {
                if (newDirection != _direction)
                {
                    _turn.CanselRun(); // 部屋では方向転換しないで、高速移動をキャンセルしてプレイヤー操作に戻る（暫定）
                    return;
                }
                // TODO: 部屋でも、方向転換した先に通路があるなら1回だけ方向転換させていいのでは
            }

            (int column, int row) dest = (location.column + _direction.X(), location.row + _direction.Y());

            if (_enemyManager.ExistEnemy(dest) != null)
            {
                _turn.CanselRun(); // 移動先が敵キャラクターのとき、高速移動をキャンセルしてプレイヤー操作に戻る
                return;
            }

            NextLocation = dest;
            _turn.NextPhase().Forget();
        }

        private static bool IsStopLocation(MapChip[,] map, (int column, int row) location)
        {
            if (map.IsRoom(location.column, location.row) && (
                    map.IsCorridor(location.column - 1, location.row) ||
                    map.IsCorridor(location.column + 1, location.row) ||
                    map.IsCorridor(location.column, location.row - 1) ||
                    map.IsCorridor(location.column, location.row + 1)))
            {
                return true; // 部屋かつ通路に隣接しているとき（通路から部屋に入ったときもこの条件に一致する）
            }

            if (map.IsCorridor(location.column, location.row) && (CorridorWays(map, location) >= 3))
            {
                return true; // 通路の分岐点（三叉路以上）のとき
            }

            return false;
        }

        private static int CorridorWays(MapChip[,] map, (int column, int row) location)
        {
            var ways = 0;
            if (map.IsCorridor(location.column - 1, location.row)) ways++;
            if (map.IsCorridor(location.column + 1, location.row)) ways++;
            if (map.IsCorridor(location.column, location.row - 1)) ways++;
            if (map.IsCorridor(location.column, location.row + 1)) ways++;
            return ways;
        }

        private static Direction MovableDirection(MapChip[,] map, (int column, int row) location, Direction direction)
        {
            // (int column, int row) forward = (location.column + direction.X(), location.row + direction.Y());
            if (!map.IsWall(location.column + direction.X(), location.row + direction.Y()))
            {
                return direction; // 前方に移動可能な場合はそのまま
            }

            var left = direction.TurnLeft();
            if (!map.IsWall(location.column + left.X(), location.row + left.Y()))
            {
                return left; // 左に方向転換可能
            }

            var right = direction.TurnRight();
            if (!map.IsWall(location.column + right.X(), location.row + right.Y()))
            {
                return right; // 右に方向転換可能
            }

            return Direction.None; // 前方、左、右に移動できない
        }

        private async UniTask DoAction(Turn turn)
        {
            if (NextLocation != MapLocation())
            {
                await MoveToNextLocation(AnimationMillis());
            }
            else
            {
                await AttackAction();
            }

            if (_map.IsStairs(MapLocation().column, MapLocation().row))
            {
                turn.OnStairs();
            }
            else
            {
                turn.NextPhase().Forget();
            }
        }

        private async UniTask AttackAction()
        {
            var location = MapLocation();
            var dest = (location.column + _direction.X(), location.row + _direction.Y());
            var target = _enemyManager != null ? _enemyManager.ExistEnemy(dest) : null; // nullでも空振りするため、early returnしない

            var damage = -1;
            if (target)
            {
                damage = target.Status.Attacked(Status.Attack);
            }

            await UniTask.Delay(actionAnimationMillis); // TODO: 攻撃演出

            if (target && !target.Status.IsAlive())
            {
                Status.AddExp(target.Status.RewardExp);
                Status.AddGold(target.Status.RewardGold);

                // TODO: 破壊演出

                Destroy(target.gameObject);
            }
        }
    }
}
