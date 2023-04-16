// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using RoguelikeExample.Dungeon;
using RoguelikeExample.Random;
using UnityEngine;

namespace RoguelikeExample.Controller
{
    [DisallowMultipleComponent]
    public class CharacterController : MonoBehaviour
    {
        protected IRandom _random;
        protected MapChip[,] _map;

        /// <summary>
        /// キャラクターの位置をマップ座標 (column, row) で返す
        /// </summary>
        /// <returns>column=3Dのx, row=3Dの-z</returns>
        internal (int column, int row) GetMapLocation()
        {
            var position = transform.position;
            return ((int)position.x, -1 * (int)position.z);
        }

        /// <summary>
        /// キャラクターの位置をマップ座標 (column, row) で設定
        /// </summary>
        /// <param name="column">0以上の整数</param>
        /// <param name="row">0以上の整数</param>
        internal void SetPositionFromMapLocation(int column, int row)
        {
            transform.position = new Vector3(column, 0, -1 * row);
        }
    }
}
