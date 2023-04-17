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
