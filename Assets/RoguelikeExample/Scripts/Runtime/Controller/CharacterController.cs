// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
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
        /// キャラクターの（当該ターンの）移動先マップ座標
        /// </summary>
        public (int column, int row) NextLocation { get; protected set; }

        /// <summary>
        /// キャラクターの現在位置をマップ座標 (column, row) で返す
        /// </summary>
        /// <returns>column=3Dのx, row=3Dの-z</returns>
        public (int column, int row) MapLocation()
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
            NextLocation = (column, row);
        }

        /// <summary>
        /// キャラクターの現在位置を <c>NextLocation</c> に移動
        /// </summary>
        /// <param name="millis">移動アニメーションにかける時間（ミリ秒）</param>
        public async UniTask MoveToNextLocation(int millis)
        {
            transform.position = new Vector3(NextLocation.column, 0, -1 * NextLocation.row);
            await UniTask.Delay(millis); // TODO: 指定時間かけて移動する
        }
    }
}
