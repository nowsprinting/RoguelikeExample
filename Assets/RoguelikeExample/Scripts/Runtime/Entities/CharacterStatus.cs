// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;

namespace RoguelikeExample.Entities
{
    public abstract class CharacterStatus
    {
        public int MaxHitPoint { get; protected set; }
        public int HitPoint { get; protected set; }
        public int Defense { get; protected set; }
        public int Attack { get; protected set; }

        /// <summary>
        /// 攻撃される
        /// 被ダメージ処理だけで、ヒットポイントが0になっても破壊処理は行わない
        /// </summary>
        /// <param name="attackPower">攻撃力</param>
        /// <returns>ダメージの値（防御を通った値。HP減少値とは必ずしも一致しない）</returns>
        public int Attacked(int attackPower)
        {
            var damage = Math.Max(0, attackPower - Defense);
            HitPoint = Math.Max(0, HitPoint - damage);
            return damage;
        }

        /// <summary>
        /// 生きている（破壊されていない）
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return HitPoint > 0;
        }

        protected class ScalingFactor
        {
            private readonly double _scalingFactor;

            public ScalingFactor(int level, float coefficient)
            {
                _scalingFactor = Math.Pow(level, coefficient);
            }

            public int Scale(int value)
            {
                return (int)(_scalingFactor * value);
            }
        }
    }
}
