// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeExample.Random;
using UnityEngine.Assertions;

namespace RoguelikeExample.Dungeon.Generator
{
    public static class MapGenerator
    {
        /// <summary>
        /// ランダムなダンジョンマップを生成します
        /// </summary>
        /// <param name="width">生成されるマップの幅（7以上）</param>
        /// <param name="height">生成されるマップの高さ（7以上）</param>
        /// <param name="roomCount">部屋の最大数（1以上）</param>
        /// <param name="maxRoomSize">部屋の最大サイズ（3以上）</param>
        /// <param name="random">擬似乱数発生器インスタンス</param>
        /// <returns></returns>
        public static MapChip[,] Generate(int width, int height, int roomCount, int maxRoomSize, IRandom random = null)
        {
            Assert.IsTrue(width >= 7);
            Assert.IsTrue(height >= 7);
            Assert.IsTrue(roomCount >= 1);
            Assert.IsTrue(maxRoomSize >= 3 && maxRoomSize <= (Math.Min(width, height) - 4));

            if (random == null)
            {
                random = new RandomImpl();
            }

            var map = new MapChip[width, height];
            var rooms = CreateRooms(width, height, roomCount, maxRoomSize, random);

            // 部屋を通路でつなぐ
            for (var i = 0; i < rooms.Count - 1; i++)
            {
                ConnectWithCorridor(map, rooms[i], rooms[i + 1]);
            }

            // 部屋の床を塗る
            foreach (var room in rooms)
            {
                room.FillFloor(map);
            }

            // 階段を配置する
            var upStair = rooms[0].GetCanSetStairPoint(map, random);
            map[upStair.x, upStair.y] = MapChip.UpStair;
            var downStair = rooms[^1].GetCanSetStairPoint(map, random);
            map[downStair.x, downStair.y] = MapChip.DownStair;

            return map;
        }

        private static List<Room> CreateRooms(int width, int height, int roomCount, int maxRoomSize, IRandom random)
        {
            var rooms = new List<Room>();
            for (var i = 0; i < roomCount; i++)
            {
                var room = new Room(width, height, maxRoomSize, random);
                if (!rooms.Any(existRoom => room.IsOverlap(existRoom)))
                {
                    rooms.Add(room);
                }
            }

            return rooms;
        }

        private readonly struct Room
        {
            private readonly int _width;
            private readonly int _height;
            private readonly int _x;
            private readonly int _y;
            public int CenterX => (_x + _width / 2);
            public int CenterY => (_y + _height / 2);

            public Room(int mapWidth, int mapHeight, int maxRoomSize, IRandom random)
            {
                const int MinRoomSize = 3;
                _width = random.Next(MinRoomSize, maxRoomSize);
                _height = random.Next(MinRoomSize, maxRoomSize);
                _x = random.Next(1, mapWidth - _width - 1);
                _y = random.Next(1, mapHeight - _height - 1);
            }

            public bool IsOverlap(Room other)
            {
                return _x <= other._x + other._width &&
                       _x + _width >= other._x &&
                       _y <= other._y + other._height &&
                       _y + _height >= other._y;
            }

            public void FillFloor(MapChip[,] map)
            {
                for (var i = _x; i < _x + _width; i++)
                {
                    for (var j = _y; j < _y + _height; j++)
                    {
                        map[i, j] = MapChip.Room;
                    }
                }
            }

            public (int x, int y) GetCanSetStairPoint(MapChip[,] map, IRandom random)
            {
                while (true)
                {
                    var x = random.Next(_x, (_x + _width));
                    var y = random.Next(_y, (_y + _height));
                    if (map[x, y] == MapChip.Room)
                        // TODO: 通路の横に配置されるのも回避したい
                    {
                        return (x, y);
                    }
                }
            }
        }

        private static void ConnectWithCorridor(MapChip[,] map, Room start, Room end)
        {
            var startX = start.CenterX;
            var startY = start.CenterY;
            var endX = end.CenterX;
            var endY = end.CenterY;

            while (startX != endX)
            {
                map[startX, startY] = MapChip.Corridor;
                startX += (startX < endX) ? 1 : -1;
            }

            while (startY != endY)
            {
                map[startX, startY] = MapChip.Corridor;
                startY += (startY < endY) ? 1 : -1;
            }
        }
    }
}
