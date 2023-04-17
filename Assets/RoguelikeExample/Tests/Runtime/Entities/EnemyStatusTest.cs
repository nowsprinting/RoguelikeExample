// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;
using RoguelikeExample.Entities.ScriptableObjects;
using UnityEngine;

namespace RoguelikeExample.Entities
{
    /// <summary>
    /// 敵ステータスのレベル補正計算のテスト
    ///
    /// 難易度調整で変更が入る可能性が高いため、開発時に意図通り実装されていることを検証できたら用済みです。
    /// <c>Ignore</c>属性で無視したり、テスト自体を削除してしまいましょう。
    /// </summary>
    [TestFixture]
    public class EnemyStatusTest
    {
        [Test]
        public void Constructor_各ステータスはレベル補正されて設定される()
        {
            var race = ScriptableObject.CreateInstance<EnemyRace>();
            race.maxHitPoint = 100;
            race.defense = 200;
            race.attack = 300;
            race.rewardExp = 400;
            race.rewardGold = 500;

            var actual = new EnemyStatus(race, 5);
            Assert.That(actual.Race, Is.EqualTo(race));
            Assert.That(actual.Level, Is.EqualTo(5));
            Assert.That(actual.MaxHitPoint, Is.EqualTo(223));
            Assert.That(actual.HitPoint, Is.EqualTo(actual.MaxHitPoint));
            Assert.That(actual.Defense, Is.EqualTo(447));
            Assert.That(actual.Attack, Is.EqualTo(670));
            Assert.That(actual.RewardExp, Is.EqualTo(4472));
            Assert.That(actual.RewardGold, Is.EqualTo(5590));
        }
    }
}
