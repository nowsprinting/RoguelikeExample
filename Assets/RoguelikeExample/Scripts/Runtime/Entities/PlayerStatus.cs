// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace RoguelikeExample.Entities
{
    public class PlayerStatus : CharacterStatus
    {
        public int Level { get; private set; } = 1;
        public int Exp { get; private set; } = 0;
        public int Gold { get; private set; } = 0;
        public int Turn { get; private set; } = 0;

        public PlayerStatus(int maxHitPoint, int defense, int attack)
        {
            MaxHitPoint = maxHitPoint;
            HitPoint = maxHitPoint;
            Defense = defense;
            Attack = attack;
        }

        public void AddExp(int exp)
        {
            Exp += exp;
            // TODO: レベルアップ判定
        }

        public void AddGold(int gold)
        {
            Gold += gold;
        }

        public void IncrementTurn()
        {
            Turn++;
        }
    }
}
