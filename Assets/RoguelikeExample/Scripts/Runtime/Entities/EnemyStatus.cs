// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using RoguelikeExample.Entities.ScriptableObjects;

namespace RoguelikeExample.Entities
{
    /// <summary>
    /// 敵個体の持つステータス
    /// </summary>
    public struct EnemyStatus
    {
        public EnemyRace Race { get; private set; }
        public int Level { get; private set; }
        public int MaxHitPoint { get; private set; }
        public int HitPoint { get; private set; }
        public int Defense { get; private set; }
        public int Attack { get; private set; }
        public int RewardExp { get; private set; }
        public int RewardGold { get; private set; }

        public EnemyStatus(EnemyRace race, int level)
        {
            Race = race;
            Level = level;

            // 強さ系
            var strengthScalingFactor = new ScalingFactor(level, 0.5f);
            MaxHitPoint = strengthScalingFactor.Scale(race.maxHitPoint);
            HitPoint = MaxHitPoint;
            Defense = strengthScalingFactor.Scale(race.defense);
            Attack = strengthScalingFactor.Scale(race.attack);

            // 報酬系
            var rewardScalingFactor = new ScalingFactor(level, 1.5f);
            RewardExp = rewardScalingFactor.Scale(race.rewardExp);
            RewardGold = rewardScalingFactor.Scale(race.rewardGold);
        }

        private class ScalingFactor
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
