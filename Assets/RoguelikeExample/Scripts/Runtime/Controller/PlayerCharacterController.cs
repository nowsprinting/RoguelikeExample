// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Input;
using UnityEngine;

namespace RoguelikeExample.Controller
{
    /// <summary>
    /// Using <c>PlayerInputActions</c>
    /// </summary>
    public sealed class PlayerCharacterController : MonoBehaviour
    {
        public MapChip[,] Map { get; set; }

        private PlayerInputActions _inputActions;
        private bool _processing;

        /// <summary>
        /// キャラクターの位置をマップ座標 (column, row) で返す
        /// </summary>
        /// <returns>column=3Dのx, row=3Dの-z</returns>
        public (int column, int row) GetMapLocation()
        {
            var position = transform.position;
            return ((int)position.x, -1 * (int)position.z);
        }

        /// <summary>
        /// キャラクターの位置をマップ座標 (column, row) で設定
        /// </summary>
        /// <param name="column">0以上の整数</param>
        /// <param name="row">0以上の整数</param>
        public void SetPositionFromMapLocation(int column, int row)
        {
            transform.position = new Vector3(column, 0, -1 * row);
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
            if (Map == null || _processing)
                return; // 移動などの処理中は入力を受け付けない

            var move = _inputActions.Player.Move.ReadValue<Vector2>().normalized;
            if (move != Vector2.zero)
            {
                StartCoroutine(Move((int)move.x, (int)move.y));
            }

            // TODO: yubn

            // TODO: ダッシュ移動
        }

        private IEnumerator Move(int x, int y)
        {
            var mapPosition = GetMapLocation();
            (int column, int row) dest = (mapPosition.column + x, mapPosition.row + y);
            if (Map.IsWall(dest.column, dest.row))
                yield break;

            _processing = true;

            // TODO: 移動アニメーション
            yield return new WaitForSeconds(0.1f);

            SetPositionFromMapLocation(dest.column, dest.row); // 移動後
            yield return null;

            _processing = false;
        }
    }
}
