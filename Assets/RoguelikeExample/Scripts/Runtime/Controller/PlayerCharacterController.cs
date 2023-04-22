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
        [SerializeField, Tooltip("Dungeon Manager")]
        internal DungeonManager dungeonManager;
        // 設定されていなくてもエラーにはしないこと。
        // Dungeon.unityでは設定必須なので、<c>DungeonSceneValidator</c>でバリデーションしている

        [SerializeField, Tooltip("行動アニメーション時間")]
        internal int actionAnimationMillis = 100;

        [SerializeField, Tooltip("高速移動時アニメーション時間")]
        internal int runAnimationMillis = 10;

        private EnemyManager _enemyManager;

        public PlayerStatus Status { get; internal set; } = new PlayerStatus(10, 1, 3);

        private PlayerInputActions _inputActions;
        private bool _processing;
        private Direction _direction;

        /// <summary>
        /// インゲーム開始時に <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="random">このキャラクターが消費する擬似乱数インスタンス</param>
        public void Initialize(IRandom random)
        {
            _random = random;
        }

        /// <summary>
        /// 新しいレベルに移動したときに <c>DungeonManager</c> から設定される
        /// </summary>
        /// <param name="map">当該レベルのマップ</param>
        /// <param name="location">キャラクターの初期位置</param>
        /// <param name="enemyManager">当該レベルの敵管理</param>
        public void NewLevel(MapChip[,] map, (int colum, int row) location, EnemyManager enemyManager)
        {
            _map = map;
            SetPositionFromMapLocation(location.colum, location.row);
            _enemyManager = enemyManager;
        }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Attack.performed += Attack;
            _inputActions.Enable();
        }

        private void OnDestroy()
        {
            _inputActions?.Dispose();
        }

        private void Update()
        {
            if (_processing)
                return; // 移動などの処理中は入力を受け付けない

            // hjkl, arrow, stick
            var move = _inputActions.Player.Move.ReadValue<Vector2>().normalized;
            if (move == Vector2.up)
            {
                Move(Direction.Up);
                return;
            }

            if (move == Vector2.down)
            {
                Move(Direction.Down);
                return;
            }

            if (move == Vector2.right)
            {
                Move(Direction.Right);
                return;
            }

            if (move == Vector2.left)
            {
                Move(Direction.Left);
                return;
            }

            // yubnは反時計回り45°回転して斜め移動として扱う
            var diagonalMove = _inputActions.Player.DiagonalMove.ReadValue<Vector2>().normalized;
            if (diagonalMove == Vector2.up)
            {
                Move(Direction.UpLeft);
                return;
            }

            if (diagonalMove == Vector2.down)
            {
                Move(Direction.DownRight);
                return;
            }

            if (diagonalMove == Vector2.right)
            {
                Move(Direction.UpRight);
                return;
            }

            if (diagonalMove == Vector2.left)
            {
                Move(Direction.DownLeft);
                return;
            }
        }

        private void Move(Direction direction)
        {
            _direction = direction; // 移動できないときも方向だけは反映するので、early returnしない

            var run = _inputActions.Player.Run.ReadValue<float>() > 0 && !direction.IsDiagonal(); // 高速移動。ループの外で確定させておく

            while (true)
            {
                var location = MapLocation();
                (int column, int row) dest = (location.column + direction.X(), location.row + direction.Y());

                if (_map.IsWall(dest.column, dest.row))
                    return; // 移動先が壁

                if (_enemyManager.ExistEnemy(dest) != null)
                    return; // 移動先に敵がいる場合は移動しない（攻撃はspaceキー）

                NextLocation = dest;

                var animationMillis = run ? runAnimationMillis : actionAnimationMillis;
                DoAction(async () => { await MoveToNextLocation(animationMillis); }, animationMillis).Forget();

                if (!run)
                    break;

                (run, _direction) = GetNextDirectionIfContinueRun(_direction);
            }
        }

        private (bool continueRun, Direction nextDirection) GetNextDirectionIfContinueRun(Direction direction)
        {
            var location = MapLocation();
            (int column, int row) dest = (location.column + direction.X(), location.row + direction.Y());

            if (_map.IsUpStair(location.column, location.row) || _map.IsDownStair(location.column, location.row))
                return (false, direction); // 階段に乗ったら止まる

            if (_map.IsRoom(location.column, location.row) && (
                    _map.IsCorridor(location.column - 1, location.row) ||
                    _map.IsCorridor(location.column + 1, location.row) ||
                    _map.IsCorridor(location.column, location.row - 1) ||
                    _map.IsCorridor(location.column, location.row + 1)))
                return (false, direction); // 部屋にいるとき、通路に隣接したら止まる

            if (_map[location.column, location.row] == MapChip.Corridor && _map[dest.column, dest.row] == MapChip.Room)
                return (false, direction); // 通路から部屋に入る手前で止まる

            if (_map.IsWall(dest.column, dest.row))
            {
                return (false, direction); // TODO: 方向を変える。通路なら何度でもok、部屋では1回だけかつ曲がった先に通路か階段で止まる場合のみ
            }

            return (true, direction);
        }

        private void Attack(InputAction.CallbackContext context)
        {
            if (_processing)
                return; // 移動などの処理中は入力を受け付けない

            async UniTask Action()
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

                    // TODO: 破壊演出（forget）
                    Destroy(target.gameObject);
                }
            }

            DoAction(Action, actionAnimationMillis).Forget();
        }

        /// <summary>
        /// 敵キャラクター思考 → プレイヤーの行動 → 敵キャラクター行動
        /// </summary>
        /// <param name="action">プレイヤーの行動</param>
        /// <param name="animationMillis">移動アニメーションにかける時間（ミリ秒）</param>
        private async UniTask DoAction(Func<UniTask> action, int animationMillis)
        {
            Status.IncrementTurn();
            _processing = true;

            _enemyManager.ThinkActionEnemies(this);
            await action();
            await _enemyManager.DoActionEnemies(animationMillis);

            _processing = false;
        }
    }
}
