// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using RoguelikeExample.Controller;
using RoguelikeExample.Dungeon;
using RoguelikeExample.Random;
using UnityEngine;

namespace RoguelikeExample.AI
{
    /// <summary>
    /// 直線的に往復する行動AI。接敵したらその場で攻撃する
    ///
    /// 仕様
    /// - 最初の方向はランダムだが、必ず壁がない方向を選択する
    /// - 以降は毎ターン直進し、壁にあたったら反転する
    /// - 接敵したらその場で攻撃する
    /// </summary>
    public class BackAndForthAI : AbstractAI
    {
        private Direction _direction;

        /// <inheritdoc/>
        public BackAndForthAI(IRandom random) : base(random) { }

        /// <inheritdoc/>
        public override (int column, int row) ThinkAction(
            MapChip[,] map,
            EnemyCharacterController myself,
            PlayerCharacterController target)
        {
            var myLocation = myself.MapLocation();
            var targetLocation = target.NextLocation;

            if (IsEngagement(myLocation, target.NextLocation))
            {
                return targetLocation; // 接敵している場合はその場から攻撃する
            }

            if (_direction == default)
            {
                _direction = DecideFirstDirection(map, myLocation); // 最初の移動方向を決める
            }

            if (map.IsWall(myLocation.column + _direction.X(), myLocation.row + _direction.Y()))
            {
                _direction = _direction.TurnBack(); // 移動先が壁なら反転
            }

            return (myLocation.column + _direction.X(), myLocation.row + _direction.Y()); // 移動先の座標を返す
        }

        private Direction DecideFirstDirection(MapChip[,] map, (int column, int row) myLocation)
        {
            var lotteryBox = new List<Direction>();
            if (!map.IsWall(myLocation.column - 1, myLocation.row)) lotteryBox.Add(Direction.Left);
            if (!map.IsWall(myLocation.column + 1, myLocation.row)) lotteryBox.Add(Direction.Right);
            if (!map.IsWall(myLocation.column, myLocation.row - 1)) lotteryBox.Add(Direction.Up);
            if (!map.IsWall(myLocation.column, myLocation.row + 1)) lotteryBox.Add(Direction.Down);

            if (lotteryBox.Any())
            {
                return lotteryBox[_random.Next(lotteryBox.Count)];
            }

            Debug.LogError("壁に囲まれているので、AIの初期方向を決められません");
            return Direction.None;
        }
    }
}
