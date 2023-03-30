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
        private PlayerInputActions _inputActions;
        private bool _processing;

        public MapUtil DungeonMap { get; set; }

        /// <summary>
        /// キャラクターの位置をマップ座標 (x, y) で返す
        /// </summary>
        /// <returns>3Dのx, 3Dの-z</returns>
        public (int x, int y) GetMapPosition()
        {
            var position = transform.position;
            return ((int)position.x, -1 * (int)position.z);
        }

        /// <summary>
        /// キャラクターの位置をマップ座標 (x, y) で設定
        /// </summary>
        /// <param name="x">0以上の整数</param>
        /// <param name="y">0以上の整数</param>
        public void SetMapPosition(int x, int y)
        {
            transform.position = new Vector3(x, 0, -1 * y);
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
            if (DungeonMap == null || _processing)
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
            var mapPosition = GetMapPosition();
            (int x, int y) dest = (mapPosition.x + x, mapPosition.y + y);
            if (DungeonMap.IsWall(dest.x, dest.y))
                yield break;

            _processing = true;

            // TODO: 移動アニメーション
            yield return new WaitForSeconds(0.1f);

            SetMapPosition(dest.x, dest.y); // 移動後
            yield return null;

            _processing = false;
        }
    }
}
