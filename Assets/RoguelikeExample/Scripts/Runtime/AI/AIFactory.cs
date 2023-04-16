// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

using System;
using RoguelikeExample.Random;

namespace RoguelikeExample.AI
{
    public static class AIFactory
    {
        public static AbstractAI CreateAI(AIType aiType, IRandom random)
        {
            switch (aiType)
            {
                case AIType.BackAndForth:
                    return new BackAndForthAI(random);
                default:
                    throw new ArgumentOutOfRangeException($"未定義のAIType `{aiType}` が渡されました");
            }
        }
    }
}
