// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Text;
using RoguelikeExample.Dungeon;

namespace RoguelikeExample.Utils
{
    public static class MapHelper
    {
        /// <summary>
        /// ログ出力用の文字列にダンプする
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public static string Dump(MapChip[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var builder = new StringBuilder();

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    builder.Append(map[x, y].ToString("D"));
                }

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        /// <summary>
        /// ログ出力用の文字列配列からMapを生成する
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static MapChip[,] CreateFromDumpStrings(string[] src)
        {
            var width = src[0].Length;
            var height = src.Length;
            var map = new MapChip[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    map[x, y] = (MapChip)int.Parse(src[y][x].ToString());
                }
            }

            return map;
        }
    }
}
