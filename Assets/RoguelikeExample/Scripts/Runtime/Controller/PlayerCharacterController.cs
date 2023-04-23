// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using Cysharp.Threading.Tasks;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities;
using RoguelikeExample.Input;
using RoguelikeExample.Random;
using UnityEngine;
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
        private Turn _turn; // イベントだけでなく操作受付可否判定などで参照するため保持

        private PlayerInputActions _inputActions;
        private Direction _direction;

        /// <summary>
        /// インゲーム開始時に <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="random">このキャラクターが消費する擬似乱数発生器インスタンス</param>
        /// <param name="turn">行動ターンのステート</param>
        /// <param name="enemyManager">敵キャラクター管理</param>
        public void Initialize(IRandom random, Turn turn = null, EnemyManager enemyManager = null)
        {
            _enemyManager = enemyManager;
            _random = random;
            _turn = turn;
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
                    ThinkingToRun();
                    break;
                case TurnState.PlayerAction:
                    DoAction().Forget();
                    break;
            }
        }

        private void Update()
        {
            if (_turn.State != TurnState.PlayerIdol)
                return; // PlayerIdol以外は入力を受け付けない

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
            _turn.IsRun = _inputActions.Player.Run.ReadValue<float>() > 0 && !direction.IsDiagonal(); // 高速移動

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
            _turn.NextPhase().Forget();
        }

        private void ThinkingToRun()
        {
            if (!_turn.IsRun)
            {
                _turn.NextPhase().Forget(); // 高速移動中でなければ何もしないでフェーズを送る
                return;
            }

            var location = MapLocation();
            (int column, int row) dest = (location.column + _direction.X(), location.row + _direction.Y());

            if (_map.IsWall(dest.column, dest.row))
            {
                // 移動先が壁
                // TODO: 方向を変える。通路なら何度でもok、部屋では1回だけかつ曲がった先に通路か階段で止まる場合のみ

                _turn.IsRun = false; // 仮
                _turn.NextPhase().Forget(); // 仮
                return;
            }

            if (_enemyManager.ExistEnemy(dest) != null)
            {
                // 移動先が敵キャラクター
                _turn.IsRun = false;
                _turn.NextPhase().Forget();
                return;
            }

            NextLocation = dest;
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

        private async UniTask DoAction()
        {
            if (NextLocation != MapLocation())
            {
                await MoveToNextLocation(AnimationMillis());
                JudgeToStopRun(); // 高速移動を止めるか判断
            }
            else
            {
                await AttackAction();
            }

            _turn.NextPhase().Forget();
        }

        private void JudgeToStopRun()
        {
            var location = MapLocation();

            if (_map.IsUpStair(location.column, location.row) || _map.IsDownStair(location.column, location.row))
            {
                _turn.IsRun = false; // 階段に乗ったら止まる
                return;
            }

            if (_map.IsRoom(location.column, location.row) && (
                    _map.IsCorridor(location.column - 1, location.row) ||
                    _map.IsCorridor(location.column + 1, location.row) ||
                    _map.IsCorridor(location.column, location.row - 1) ||
                    _map.IsCorridor(location.column, location.row + 1)))
            {
                _turn.IsRun = false; // 部屋にいるとき、通路に隣接したら止まる
                return;
            }

            (int column, int row) dest = (location.column + _direction.X(), location.row + _direction.Y());

            if (_map[location.column, location.row] == MapChip.Corridor &&
                _map[dest.column, dest.row] == MapChip.Room)
            {
                _turn.IsRun = false; // 通路から部屋に入る手前で止まる
                return;
            }
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
