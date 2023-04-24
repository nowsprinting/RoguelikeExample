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
        /// <param name="enemyManager">敵キャラクター管理</param>
        public void Initialize(IRandom random, EnemyManager enemyManager = null)
        {
            _enemyManager = enemyManager;
            _random = random;
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
        public int AnimationMillis() => Turn.GetInstance().IsRun ? runAnimationMillis : actionAnimationMillis;

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Attack.performed += AttackOperation;
            _inputActions.Enable();
            Turn.OnPhaseTransition += HandlePhaseTransition;
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
            Turn.OnPhaseTransition -= HandlePhaseTransition;
        }

        private void HandlePhaseTransition(object sender, EventArgs _)
        {
            switch (((Turn)sender).State)
            {
                case TurnState.PlayerRun:
                    ThinkToRun();
                    break;
                case TurnState.PlayerAction:
                    DoAction().Forget();
                    break;
            }
        }

        private void Update()
        {
            if (Turn.GetInstance().State != TurnState.PlayerIdol)
            {
                return; // PlayerIdol以外は入力を受け付けない
            }

            // hjkl, arrow, stick
            var move = _inputActions.Player.Move.ReadValue<Vector2>().normalized;
            if (move == Vector2.up)
            {
                MoveOperation(Direction.Up);
                return;
            }

            if (move == Vector2.down)
            {
                MoveOperation(Direction.Down);
                return;
            }

            if (move == Vector2.right)
            {
                MoveOperation(Direction.Right);
                return;
            }

            if (move == Vector2.left)
            {
                MoveOperation(Direction.Left);
                return;
            }

            // yubnは反時計回り45°回転して斜め移動として扱う
            var diagonalMove = _inputActions.Player.DiagonalMove.ReadValue<Vector2>().normalized;
            if (diagonalMove == Vector2.up)
            {
                MoveOperation(Direction.UpLeft);
                return;
            }

            if (diagonalMove == Vector2.down)
            {
                MoveOperation(Direction.DownRight);
                return;
            }

            if (diagonalMove == Vector2.right)
            {
                MoveOperation(Direction.UpRight);
                return;
            }

            if (diagonalMove == Vector2.left)
            {
                MoveOperation(Direction.DownLeft);
                return;
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

            var turn = Turn.GetInstance();
            turn.IsRun = _inputActions.Player.Run.ReadValue<float>() > 0 && !direction.IsDiagonal(); // 次ターンから高速移動
            turn.NextPhase().Forget();
        }

        private void AttackOperation(InputAction.CallbackContext context)
        {
            var turn = Turn.GetInstance();
            if (turn.State != TurnState.PlayerIdol)
            {
                return; // PlayerIdol以外は入力を受け付けない
            }

            turn.NextPhase().Forget(); // 空振りでもフェーズを送る
        }

        private void ThinkToRun()
        {
            var turn = Turn.GetInstance();
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
                turn.CanselRun(); // 移動先がないとき、高速移動をキャンセルしてプレイヤー操作に戻る
                return;
            }
            else if (_map.IsCorridor(location.column, location.row))
            {
                _direction = newDirection; // 通路では無制限に方向転換
            }
            // TODO: 部屋でも、方向転換した先に通路があるなら1回だけ方向転換させていいのでは

            (int column, int row) dest = (location.column + _direction.X(), location.row + _direction.Y());

            if (_enemyManager.ExistEnemy(dest) != null)
            {
                turn.CanselRun(); // 移動先が敵キャラクターのとき、高速移動をキャンセルしてプレイヤー操作に戻る
                return;
            }

            NextLocation = dest;
            turn.NextPhase().Forget();
        }

        private static bool IsStopLocation(MapChip[,] map, (int column, int row) location)
        {
            if (map.IsUpStair(location.column, location.row) || map.IsDownStair(location.column, location.row))
            {
                return true; // 階段
            }

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

        private async UniTask DoAction()
        {
            if (NextLocation != MapLocation())
            {
                await MoveToNextLocation(AnimationMillis());
            }
            else
            {
                await AttackAction();
            }

            Turn.GetInstance().NextPhase().Forget();
        }

        private async UniTask AttackAction()
        {
            var location = MapLocation();
            var dest = (location.column + _direction.X(), location.row + _direction.Y());
            var target = _enemyManager.ExistEnemy(dest); // nullでも空振りするため、early returnしない

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
