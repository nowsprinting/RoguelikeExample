// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace RoguelikeExample.Dungeon
{
    public static class MapExtensions
    {
        private static (int column, int row) GetChipPosition(MapChip[,] map, MapChip chip)
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

        public static (int column, int row) GetUpStairPosition(this MapChip[,] map)
        {
            return GetChipPosition(map, MapChip.UpStair);
        }

        public static (int column, int row) GetDownStairPosition(this MapChip[,] map)
        {
            return GetChipPosition(map, MapChip.DownStair);
        }

        public static bool IsWall(this MapChip[,] map, int column, int row)
        {
            return map[column, row] == MapChip.Wall;
        }
    }
}
