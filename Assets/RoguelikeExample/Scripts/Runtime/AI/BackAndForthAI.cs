// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Random;
using UnityEngine;

namespace RoguelikeExample.AI
{
    /// <summary>
    /// 直線的に往復する行動AI。接敵したらその場で攻撃する
    ///
    /// 仕様
    ///  - 最初の方向はランダムだが、必ず壁がない方向を選択する
    ///  - 以降は毎ターン直進し、壁にあたったら反転する
    ///  - 接敵したらその場で攻撃する
    /// </summary>
    public class BackAndForthAI : AbstractAI
    {
        private Vector2 _direction;

        /// <inheritdoc/>
        public BackAndForthAI(IRandom random) : base(random) { }

        /// <inheritdoc/>
        public override (int column, int row) ThinkAction(
            MapChip[,] map,
            (int column, int row) myLocation,
            (int column, int row) targetLocation)
        {
            if (IsEngagement(myLocation, targetLocation))
            {
                return targetLocation; // 接敵している場合はその場から攻撃する
            }

            if (_direction == default)
            {
                _direction = DecideFirstDirection(map, myLocation); // 最初の移動方向を決める
            }

            if (map.IsWall(myLocation.column + (int)_direction.x, myLocation.row + (int)_direction.y))
            {
                _direction = -_direction; // 移動先が壁なら反転
            }

            return (myLocation.column + (int)_direction.x, myLocation.row + (int)_direction.y); // 移動先の座標を返す
        }

        private Vector2 DecideFirstDirection(MapChip[,] map, (int column, int row) myLocation)
        {
            var lotteryBox = new List<Vector2>();
            if (!map.IsWall(myLocation.column - 1, myLocation.row)) lotteryBox.Add(Vector2.left);
            if (!map.IsWall(myLocation.column + 1, myLocation.row)) lotteryBox.Add(Vector2.right);
            if (!map.IsWall(myLocation.column, myLocation.row - 1)) lotteryBox.Add(Vector2.up);
            if (!map.IsWall(myLocation.column, myLocation.row + 1)) lotteryBox.Add(Vector2.down);

            if (lotteryBox.Any())
            {
                return lotteryBox[_random.Next(lotteryBox.Count)];
            }

            Debug.LogError("壁に囲まれているので、AIの初期方向を決められません");
            return Vector2.zero;
        }
    }
}
