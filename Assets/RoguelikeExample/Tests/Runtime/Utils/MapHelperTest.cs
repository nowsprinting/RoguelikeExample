// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using NUnit.Framework;
using RoguelikeExample.Dungeon;

namespace RoguelikeExample.Utils
{
    public class MapHelperTest
    {
        [Test]
        public void Dump_文字列形式の出力が得られる()
        {
            var map = new MapChip[,]
            {
                { MapChip.Wall, MapChip.Wall, }, // x=0 : 壁、壁
                { MapChip.Room, MapChip.Corridor, }, // x=1 : 部屋、通路
                { MapChip.UpStairs, MapChip.DownStairs, }, // x=2 : 上り階段、下り階段
                // z=0（奥）、z=-1（手前）
            };
            var actual = MapHelper.Dump(map);
            Assert.That(actual, Is.EqualTo($"013{Environment.NewLine}024{Environment.NewLine}"));
        }

        [Test]
        public void CreateFromDumpStrings_文字列からMapが得られる()
        {
            var actual = MapHelper.CreateFromDumpStrings(new[] { "013", "024", });
            Assert.That(actual[0, 0], Is.EqualTo(MapChip.Wall));
            Assert.That(actual[0, 1], Is.EqualTo(MapChip.Wall));
            Assert.That(actual[1, 0], Is.EqualTo(MapChip.Room));
            Assert.That(actual[1, 1], Is.EqualTo(MapChip.Corridor));
            Assert.That(actual[2, 0], Is.EqualTo(MapChip.UpStairs));
            Assert.That(actual[2, 1], Is.EqualTo(MapChip.DownStairs));
        }
    }
}
