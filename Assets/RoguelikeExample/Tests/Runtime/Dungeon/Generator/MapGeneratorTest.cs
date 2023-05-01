// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using RoguelikeExample.Random;
using RoguelikeExample.Utils;
using UnityEngine;

namespace RoguelikeExample.Dungeon.Generator
{
    /// <summary>
    /// ダンジョンマップ生成のテスト
    ///
    /// 内部メソッド個々のユニットテストは省略し、生成されたマップデータが要件を満たしていることを検証しています。
    /// しかし、たとえば<c>Room.IsOverlap</c>など間違っていてもマップ生成できてしまうメソッドは、ユニットテストを書いてもいいでしょう。
    ///
    /// テストダブルは使用せず、実際にランダムに生成されたマップを検証しています。そのため
    ///  - 開発時はRepeat属性を使用して繰り返し実行しています
    ///  - 安定してからはリグレッションテストとして最小限の実行回数にしてあります（「CIでいつか見つかるかも」という考えは好ましくない）
    /// なお、問題発生時の再現性確保のため、擬似乱数シード値をログ出力しています。
    ///
    /// マップ生成をテストケースごとに実行しています。実行効率は悪いのですが、問題発見・対処のしやすさを優先した実装です。
    /// </summary>
    [TestFixture]
    public class MapGeneratorTest
    {
        private static object[] s_testCaseSource =
        {
            new[] { 7, 7, 1, 3 }, // 最小サイズ
            new[] { 40, 10, 5, 6 }, // 幅と高さが異なる
        };
        // Note: 開発時はパターンを増やして多数試行、以降は以降はリグレッションテストとして最小限のパターンのみ残す

        private const int RepeatCount = 1;
        // Note: 開発時は繰り返し回数を増やして多数試行、以降はリグレッションテストとして1回だけ実行

        private static MapChip[,] GenerateMap(int width, int height, int roomCount, int maxRoomSize)
        {
            var random = new RandomImpl();
            Debug.Log($"Using {random}"); // 再現性を確保するためシード値を出力

            var map = MapGenerator.Generate(width, height, roomCount, maxRoomSize, random);
            Debug.Log(MapHelper.Dump(map)); // マップをテキストで出力
            return map;
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

            for (var x = 0; x < width; x++)
            {
                Assert.That(map[x, 0], Is.EqualTo(MapChip.Wall));
                Assert.That(map[x, height - 1], Is.EqualTo(MapChip.Wall));
            }

            for (var y = 1; y < (height - 1); y++)
            {
                Assert.That(map[0, y], Is.EqualTo(MapChip.Wall));
                Assert.That(map[width - 1, y], Is.EqualTo(MapChip.Wall));
            }
        }

        [TestCaseSource(nameof(s_testCaseSource))]
        [Repeat(RepeatCount)]
        public void Generate_上り階段と下り階段がひとつづつ存在すること(int width, int height, int roomCount, int maxRoomSize)
        {
            var map = GenerateMap(width, height, roomCount, maxRoomSize);
            var upStairCount = 0;
            var downStairCount = 0;

            foreach (var chip in map)
            {
                switch (chip)
                {
                    case MapChip.UpStairs:
                        upStairCount++;
                        break;
                    case MapChip.DownStairs:
                        downStairCount++;
                        break;
                }
            }

            Assert.That(upStairCount, Is.EqualTo(1), nameof(upStairCount));
            Assert.That(downStairCount, Is.EqualTo(1), nameof(downStairCount));
        }

        [TestCaseSource(nameof(s_testCaseSource))]
        [Repeat(RepeatCount)]
        public void Generate_上り階段と下り階段の間を移動可能であること(int width, int height, int roomCount, int maxRoomSize)
        {
            var map = GenerateMap(width, height, roomCount, maxRoomSize);
            var (upStairX, upStairY) = map.GetUpStairsPosition();
            var (downStairX, downStairY) = map.GetDownStairsPosition();
            var visited = new bool[width, height];
            var pathExists = PathExists(map, visited, upStairX, upStairY, downStairX, downStairY);

            Assert.That(pathExists, Is.True);
        }

        private bool PathExists(MapChip[,] map, bool[,] visited, int startX, int startY, int endX, int endY)
        {
            if (startX == endX && startY == endY)
            {
                return true;
            }

            visited[startX, startY] = true;

            var mapWidth = map.GetLength(0);
            var mapHeight = map.GetLength(1);
            var destinations = new (int x, int y)[]
            {
                (startX, startY - 1), (startX, startY + 1), (startX - 1, startY), (startX + 1, startY) // 斜め移動はなし
            };

            foreach (var (x, y) in destinations)
            {
                if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
                {
                    continue;
                }

                if (map[x, y] == MapChip.Wall) // 壁以外は移動可能
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
