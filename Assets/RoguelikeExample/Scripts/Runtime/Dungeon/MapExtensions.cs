// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace RoguelikeExample.Dungeon
{
    public static class MapExtensions
    {
        private static (int column, int row) GetChipPosition(this MapChip[,] map, MapChip chip)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    if (map[x, y] == chip)
                    {
                        return (x, y);
                    }
                }
            }

            throw new ArgumentException($"指定されたマップチップが存在しません: {chip}");
        }

        public static (int column, int row) GetUpStairsPosition(this MapChip[,] map)
        {
            return GetChipPosition(map, MapChip.UpStairs);
        }

        public static (int column, int row) GetDownStairsPosition(this MapChip[,] map)
        {
            return GetChipPosition(map, MapChip.DownStairs);
        }

        public static bool IsWall(this MapChip[,] map, int column, int row)
        {
            return map[column, row] == MapChip.Wall;
        }

        public static bool IsRoom(this MapChip[,] map, int column, int row)
        {
            return map[column, row] == MapChip.Room;
        }

        public static bool IsCorridor(this MapChip[,] map, int column, int row)
        {
            return map[column, row] == MapChip.Corridor;
        }

        public static bool IsUpStairs(this MapChip[,] map, int column, int row)
        {
            return map[column, row] == MapChip.UpStairs;
        }

        public static bool IsDownStairs(this MapChip[,] map, int column, int row)
        {
            return map[column, row] == MapChip.DownStairs;
        }

        public static bool IsStairs(this MapChip[,] map, int column, int row)
        {
            return map.IsUpStairs(column, row) || map.IsDownStairs(column, row);
        }
    }
}
