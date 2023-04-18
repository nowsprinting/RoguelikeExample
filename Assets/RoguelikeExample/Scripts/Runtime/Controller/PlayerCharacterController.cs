// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Entities;
using RoguelikeExample.Input;
using RoguelikeExample.Random;
using UnityEngine;

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

        public PlayerStatus Status { get; private set; } = new PlayerStatus();

        private PlayerInputActions _inputActions;
        private bool _processing;
        internal int _moveAnimationMillis = 100; // 移動アニメーション時間（テストで上書きするのでconstにしていない）

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
        public void NewLevel(MapChip[,] map, (int colum, int row) location)
        {
            _map = map;
            SetPositionFromMapLocation(location.colum, location.row);
        }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
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
                Move((int)move.x, (int)move.y).Forget();
                return;
            }

            // yubnは反時計回り45°回転して斜め移動として扱う
            var diagonalMove = _inputActions.Player.DiagonalMove.ReadValue<Vector2>().normalized;
            if (diagonalMove == Vector2.up)
            {
                Move(-1, 1).Forget();
                return;
            }

            if (diagonalMove == Vector2.right)
            {
                Move(1, 1).Forget();
                return;
            }

            if (diagonalMove == Vector2.down)
            {
                Move(1, -1).Forget();
                return;
            }

            if (diagonalMove == Vector2.left)
            {
                Move(-1, -1).Forget();
                return;
            }
        }

        private async UniTask Move(int column, int row)
        {
            var location = MapLocation();
            (int column, int row) dest = (location.column + column, location.row - row); // y成分は上が+

            if (_map.IsWall(dest.column, dest.row))
                return;

            var enemyManager = EnemyManager();
            if (enemyManager != null && enemyManager.ExistEnemy(dest) != null)
            {
                return; // 移動先に敵がいる場合は移動しない（攻撃はspaceキー）
            }

            _processing = true;

            Status.IncrementTurn();
            NextLocation = dest;

            if (enemyManager != null)
            {
                enemyManager.ThinkActionEnemies(this);
            }

            await MoveToNextLocation(_moveAnimationMillis);

            if (enemyManager != null)
            {
                await enemyManager.DoActionEnemies(_moveAnimationMillis);
            }

            _processing = false;
        }

        private EnemyManager EnemyManager()
        {
#if !UNITY_INCLUDE_TESTS
            UnityEngine.Assertions.Assert.IsNotNull(dungeonManager, "DungeonManagerが存在しません"); // テストでは存在しないことを許容
            // Note: 原則、このようなテストのためのロジックをプロダクトコードに入れるのは避けるべきです
#endif
            if (dungeonManager != null)
            {
                return dungeonManager.EnemyManager;
            }

            return null;
        }
    }
}
