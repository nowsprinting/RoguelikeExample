// Copyright (c) 2023 Koji Hasegawa.
// This software is released under the MIT License.

namespace RoguelikeExample.Random
{
    /// <summary>
    /// Random number generator interface.
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns a non-negative random integer.
        /// </summary>
        /// <returns>Generated random number</returns>
        int Next();

        /// <summary>
        /// Returns random number that is less than specified max-value.
        /// </summary>
        /// <param name="maxValue">Upper bound of the random number to be generated</param>
        /// <returns>Generated random number</returns>
        int Next(int maxValue);

        /// <summary>
        /// Returns random number that is within a specified range.
        /// </summary>
        /// <param name="minValue">Lower bound of the random number to be generated</param>
        /// <param name="maxValue">Upper bound of the random number to be generated</param>
        /// <returns>Generated random number</returns>
        int Next(int minValue, int maxValue);
    }
}
