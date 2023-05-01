// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace RoguelikeExample.Controller
{
    public static class DirectionExtensions
    {
        /// <summary>
        /// (x, y) に変換する
        /// </summary>
        public static (int x, int y) ToVector(this Direction direction)
        {
            return direction switch
            {
                Direction.None => (0, 0),
                Direction.Up => (0, -1),
                Direction.Down => (0, 1),
                Direction.Left => (-1, 0),
                Direction.Right => (1, 0),
                Direction.UpLeft => (-1, -1),
                Direction.UpRight => (1, -1),
                Direction.DownLeft => (-1, 1),
                Direction.DownRight => (1, 1),
                _ => throw new ArgumentException($"不正な方向が指定されました: {direction}")
            };
        }

        /// <summary>
        /// x成分を返す
        /// </summary>
        public static int X(this Direction direction)
        {
            return direction.ToVector().x;
        }

        /// <summary>
        /// y成分を返す
        /// </summary>
        public static int Y(this Direction direction)
        {
            return direction.ToVector().y;
        }

        /// <summary>
        /// 左にターンした方向を返す
        /// </summary>
        public static Direction TurnLeft(this Direction direction)
        {
            return direction switch
            {
                Direction.None => Direction.None,
                Direction.Up => Direction.Left,
                Direction.Down => Direction.Right,
                Direction.Left => Direction.Down,
                Direction.Right => Direction.Up,
                Direction.UpLeft => Direction.DownLeft,
                Direction.UpRight => Direction.UpLeft,
                Direction.DownLeft => Direction.DownRight,
                Direction.DownRight => Direction.UpRight,
                _ => throw new ArgumentException($"不正な方向が指定されました: {direction}")
            };
        }

        /// <summary>
        /// 右にターンした方向を返す
        /// </summary>
        public static Direction TurnRight(this Direction direction)
        {
            return direction switch
            {
                Direction.None => Direction.None,
                Direction.Up => Direction.Right,
                Direction.Down => Direction.Left,
                Direction.Left => Direction.Up,
                Direction.Right => Direction.Down,
                Direction.UpLeft => Direction.UpRight,
                Direction.UpRight => Direction.DownRight,
                Direction.DownLeft => Direction.UpLeft,
                Direction.DownRight => Direction.DownLeft,
                _ => throw new ArgumentException($"不正な方向が指定されました: {direction}")
            };
        }

        /// <summary>
        /// 真後ろにターンした方向を返す
        /// </summary>
        public static Direction TurnBack(this Direction direction)
        {
            return direction switch
            {
                Direction.None => Direction.None,
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                Direction.UpLeft => Direction.DownRight,
                Direction.UpRight => Direction.DownLeft,
                Direction.DownLeft => Direction.UpRight,
                Direction.DownRight => Direction.UpLeft,
                _ => throw new ArgumentException($"不正な方向が指定されました: {direction}")
            };
        }

        /// <summary>
        /// 斜め方向かどうか
        /// </summary>
        public static bool IsDiagonal(this Direction direction)
        {
            return direction == Direction.UpLeft ||
                   direction == Direction.UpRight ||
                   direction == Direction.DownLeft ||
                   direction == Direction.DownRight;
        }

        /// <summary>
        /// デバイスからの入力を <c>Direction</c> に変換する
        /// </summary>
        /// <param name="src">8方向にスナップかつ正規化された方向入力</param>
        public static Direction FromVector2(Vector2 src)
        {
            if (src == Vector2.up) return Direction.Up;
            if (src == Vector2.down) return Direction.Down;
            if (src == Vector2.left) return Direction.Left;
            if (src == Vector2.right) return Direction.Right;

            if (src.y > 0.5f)
            {
                if (src.x < -0.5f) return Direction.UpLeft;
                if (src.x > 0.5f) return Direction.UpRight;
            }

            if (src.y < -0.5f)
            {
                if (src.x < -0.5f) return Direction.DownLeft;
                if (src.x > 0.5f) return Direction.DownRight;
            }

            return Direction.None;
        }
    }
}
