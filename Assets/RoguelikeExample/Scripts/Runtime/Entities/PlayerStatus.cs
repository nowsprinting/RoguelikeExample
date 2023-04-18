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

        public void IncrementTurn()
        {
            Turn++;
        }
    }
}
