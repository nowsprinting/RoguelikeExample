// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using NUnit.Framework;

namespace RoguelikeExample.Entities
{
    public class CharacterStatusTest
    {
        [Test]
        public void Attacked_防御を超えない攻撃力_ダメージは0()
        {
            var sut = new CharacterStatusImpl(hitPoint: 3, defense: 2);
            var damage = sut.Attacked(attackPower: 1);
            Assert.That(damage, Is.EqualTo(0)); // マイナスにはならない
            Assert.That(sut.IsAlive, Is.True);
        }

        [Test]
        public void Attacked_防御を超える攻撃力_HPにダメージ()
        {
            var sut = new CharacterStatusImpl(hitPoint: 3, defense: 1);
            var damage = sut.Attacked(attackPower: 2);
            Assert.That(damage, Is.EqualTo(1), "damage");
            Assert.That(sut.HitPoint, Is.EqualTo(2), "hit point");
            Assert.That(sut.IsAlive, Is.True);
        }

        [Test]
        public void Attacked_オーバーキル_HPはマイナスにはならない()
        {
            var sut = new CharacterStatusImpl(hitPoint: 3, defense: 1);
            var damage = sut.Attacked(attackPower: 5);
            Assert.That(damage, Is.EqualTo(4), "damage");
            Assert.That(sut.HitPoint, Is.EqualTo(0), "hit point");
            Assert.That(sut.IsAlive, Is.False);
        }

        private class CharacterStatusImpl : CharacterStatus
        {
            public CharacterStatusImpl(int hitPoint = 0, int defense = 0, int attack = 0)
            {
                MaxHitPoint = hitPoint;
                HitPoint = hitPoint;
                Defense = defense;
                Attack = attack;
            }
        }
    }
}
