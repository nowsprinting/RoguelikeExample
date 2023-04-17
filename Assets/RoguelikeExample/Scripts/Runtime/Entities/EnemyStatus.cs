// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using RoguelikeExample.Entities.ScriptableObjects;

namespace RoguelikeExample.Entities
{
    /// <summary>
    /// 敵個体の持つステータス
    /// </summary>
    public class EnemyStatus : CharacterStatus
    {
        public EnemyRace Race { get; private set; }
        public int Level { get; private set; }
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
    }
}
