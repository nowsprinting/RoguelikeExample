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

        private EnemyManager _enemyManager;

        public PlayerStatus Status { get; internal set; } = new PlayerStatus(10, 1, 3);

        private PlayerInputActions _inputActions;
        private bool _processing;
        private Vector2 _direction;
        internal int _actionAnimationMillis = 100; // 行動アニメーション時間（テストで上書きするのでconstにしていない）

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
            if (move != Vector2.zero)
            {
                Move((int)move.x, (int)move.y);
                return;
            }

            // yubnは反時計回り45°回転して斜め移動として扱う
            var diagonalMove = _inputActions.Player.DiagonalMove.ReadValue<Vector2>().normalized;
            if (diagonalMove == Vector2.up)
            {
                Move(-1, 1);
                return;
            }

            if (diagonalMove == Vector2.right)
            {
                Move(1, 1);
                return;
            }

            if (diagonalMove == Vector2.down)
            {
                Move(1, -1);
                return;
            }

            if (diagonalMove == Vector2.left)
            {
                Move(-1, -1);
                return;
            }
        }

        private void Move(int x, int y)
        {
            var location = MapLocation();
            (int column, int row) dest = (location.column + x, location.row - y); // y成分は上が+
            _direction = new Vector2(x, y); // 移動できないときも方向だけは反映

            if (_map.IsWall(dest.column, dest.row))
                return;

            if (_enemyManager.ExistEnemy(dest) != null)
                return; // 移動先に敵がいる場合は移動しない（攻撃はspaceキー）

            NextLocation = dest;
            DoAction(async () => { await MoveToNextLocation(_actionAnimationMillis); }).Forget();
        }

        private void Attack(InputAction.CallbackContext context)
        {
            if (_processing)
                return; // 移動などの処理中は入力を受け付けない

            async UniTask Action()
            {
                var location = MapLocation();
                var dest = (location.column + (int)_direction.x, location.row - (int)_direction.y); // y成分は上が+
                var target = _enemyManager.ExistEnemy(dest); // nullでも空振りするため、early returnしない

                var damage = -1;
                if (target)
                {
                    damage = target.Status.Attacked(Status.Attack);
                }

                await UniTask.Delay(_actionAnimationMillis); // TODO: 攻撃演出

                if (target && !target.Status.IsAlive())
                {
                    Status.AddExp(target.Status.RewardExp);
                    Status.AddGold(target.Status.RewardGold);

                    // TODO: 破壊演出（forget）
                    Destroy(target.gameObject);
                }
            }

            DoAction(Action).Forget();
        }

        /// <summary>
        /// 敵キャラクター思考 → プレイヤーの行動 → 敵キャラクター行動
        /// </summary>
        /// <param name="action">プレイヤーの行動</param>
        private async UniTask DoAction(Func<UniTask> action)
        {
            Status.IncrementTurn();
            _processing = true;

            _enemyManager.ThinkActionEnemies(this);
            await action();
            await _enemyManager.DoActionEnemies(_actionAnimationMillis);

            _processing = false;
        }
    }
}
