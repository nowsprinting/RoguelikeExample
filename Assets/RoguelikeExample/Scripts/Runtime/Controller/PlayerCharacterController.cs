// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using RoguelikeExample.Dungeon;
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
        private PlayerInputActions _inputActions;
        private bool _processing;
        internal int _turn;

        /// <summary>
        /// インスタンス生成時に <c>DungeonManager</c> から設定される
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
                StartCoroutine(Move((int)move.x, (int)move.y));
                return;
            }

            // yubnは反時計回り45°回転して斜め移動として扱う
            var diagonalMove = _inputActions.Player.DiagonalMove.ReadValue<Vector2>().normalized;
            if (diagonalMove == Vector2.up)
            {
                StartCoroutine(Move(-1, 1));
                return;
            }

            if (diagonalMove == Vector2.right)
            {
                StartCoroutine(Move(1, 1));
                return;
            }

            if (diagonalMove == Vector2.down)
            {
                StartCoroutine(Move(1, -1));
                return;
            }

            if (diagonalMove == Vector2.left)
            {
                StartCoroutine(Move(-1, -1));
                return;
            }
        }

        private IEnumerator Move(int x, int y)
        {
            var location = GetMapLocation();
            (int column, int row) dest = (location.column + x, location.row - y); // y成分は上が+
            if (_map.IsWall(dest.column, dest.row))
                yield break;

            _processing = true;
            _turn++;

            // TODO: 移動アニメーション
            yield return new WaitForSeconds(0.1f);

            SetPositionFromMapLocation(dest.column, dest.row); // 移動後
            yield return null;

            _processing = false;
        }
    }
}
