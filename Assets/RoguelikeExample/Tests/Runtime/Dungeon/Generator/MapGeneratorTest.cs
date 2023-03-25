// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using System.Text;
using NUnit.Framework;
using RoguelikeExample.Random;
using UnityEngine;

namespace RoguelikeExample.Dungeon.Generator
{
    [TestFixture]
    public class MapGeneratorTest
    {
        private static object[] s_testCaseSource =
        {
            new[] { 7, 7, 1, 3 }, // 最小サイズ
            new[] { 40, 10, 5, 6 }, // 幅と高さが異なる
        };

        private const int RepeatCount = 1;

        private static MapChip[,] GenerateMap(int width, int height, int roomCount, int maxRoomSize)
        {
            var random = new RandomImpl();
            Debug.Log($"Using {random.ToString()}"); // 再現性を確保するためシード値を出力

            var map = MapGenerator.Generate(width, height, roomCount, maxRoomSize, random);
            Debug.Log(Dump(map));
            return map;
        }

        private static string Dump(MapChip[,] map)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            var builder = new StringBuilder();
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    builder.Append(map[j, i].ToString("D"));
                }

                builder.Append(Environment.NewLine);
            }

            return builder.ToString();
        }

        [TestCaseSource(nameof(s_testCaseSource))]
        public void Generate_指定サイズのダンジョンマップが生成されること(int width, int height, int roomCount, int maxRoomSize)
        {
            var map = GenerateMap(width, height, roomCount, maxRoomSize);

            Assert.That(map, Is.Not.Null);
            Assert.That(map.GetLength(0), Is.EqualTo(width));
            Assert.That(map.GetLength(1), Is.EqualTo(height));
        }

        [TestCaseSource(nameof(s_testCaseSource))]
        [Repeat(RepeatCount)]
        public void Generate_生成されたマップの外周はすべて壁であること(int width, int height, int roomCount, int maxRoomSize)
        {
            var map = GenerateMap(width, height, roomCount, maxRoomSize);

            for (var i = 0; i < width; i++)
            {
                Assert.That(map[i, 0], Is.EqualTo(MapChip.Wall));
                Assert.That(map[i, height - 1], Is.EqualTo(MapChip.Wall));
            }

            for (var i = 0; i < height; i++)
            {
                Assert.That(map[0, i], Is.EqualTo(MapChip.Wall));
                Assert.That(map[width - 1, i], Is.EqualTo(MapChip.Wall));
            }
        }

        [TestCaseSource(nameof(s_testCaseSource))]
        [Repeat(RepeatCount)]
        public void Generate_登り階段と下り階段がひとつづつ存在すること(int width, int height, int roomCount, int maxRoomSize)
        {
            var map = GenerateMap(width, height, roomCount, maxRoomSize);
            var upStairCount = 0;
            var downStairCount = 0;

            foreach (var chip in map)
            {
                switch (chip)
                {
                    case MapChip.UpStair:
                        upStairCount++;
                        break;
                    case MapChip.DownStair:
                        downStairCount++;
                        break;
                }
            }

            Assert.That(upStairCount, Is.EqualTo(1), nameof(upStairCount));
            Assert.That(downStairCount, Is.EqualTo(1), nameof(downStairCount));
        }

        [TestCaseSource(nameof(s_testCaseSource))]
        [Repeat(RepeatCount)]
        public void Generate_登り階段と下り階段の間を移動可能であること(int width, int height, int roomCount, int maxRoomSize)
        {
            var map = GenerateMap(width, height, roomCount, maxRoomSize);
            var (upStairX, upStairY) = GetChipPosition(map, MapChip.UpStair);
            var (downStairX, downStairY) = GetChipPosition(map, MapChip.DownStair);
            var visited = new bool[map.GetLength(0), map.GetLength(1)];
            var pathExists = PathExists(map, visited, upStairX, upStairY, downStairX, downStairY);

            Assert.That(pathExists, Is.True);
        }

        private static (int x, int y) GetChipPosition(MapChip[,] map, MapChip chip)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    if (map[i, j] == chip)
                    {
                        return (i, j);
                    }
                }
            }

            throw new ArgumentException($"指定されたマップチップが存在しません: {chip}");
        }

        private bool PathExists(MapChip[,] map, bool[,] visited, int startX, int startY, int endX, int endY)
        {
            if (startX == endX && startY == endY)
            {
                return true;
            }

            visited[startX, startY] = true;

            var dest = new (int x, int y)[]
            {
                (startX, startY - 1), (startX, startY + 1), (startX - 1, startY), (startX + 1, startY) // 斜め移動はなし
            };

            foreach (var (x, y) in dest)
            {
                if (x < 0 || x >= map.GetLength(0) || y < 0 || y >= map.GetLength(1))
                {
                    continue;
                }

                if (map[x, y] == MapChip.Wall)
                {
                    continue;
                }

                if (visited[x, y])
                {
                    continue;
                }

                if (PathExists(map, visited, x, y, endX, endY))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
