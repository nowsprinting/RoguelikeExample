// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace RoguelikeExample.Dungeon
{
    public class MapUtil
    {
        private readonly MapChip[,] _map;
        private readonly int _mapWidth;
        private readonly int _mapHeight;

        public MapUtil(MapChip[,] map)
        {
            _map = map;
            _mapWidth = map.GetLength(0);
            _mapHeight = map.GetLength(1);
        }

        private (int x, int y) GetChipPosition(MapChip chip)
        {
            for (var x = 0; x < _mapWidth; x++)
            {
                for (var y = 0; y < _mapHeight; y++)
                {
                    if (_map[x, y] == chip)
                    {
                        return (x, y);
                    }
                }
            }

            throw new ArgumentException($"指定されたマップチップが存在しません: {chip}");
        }

        public (int x, int y) GetUpStairPosition()
        {
            return GetChipPosition(MapChip.UpStair);
        }

        public (int x, int y) GetDownStairPosition()
        {
            return GetChipPosition(MapChip.DownStair);
        }

        public bool IsWall(int x, int y)
        {
            return _map[x, y] == MapChip.Wall;
        }
    }
}
