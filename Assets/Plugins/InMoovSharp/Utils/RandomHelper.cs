using System;

namespace InMoov.Core.Utils
{
    public static class RandomHelper
    {
        private static Random _random = new Random();

        public static int Value => Range(int.MinValue, int.MaxValue);

        public static float ValueF => Range(0, 1);

        /// <summary>
        /// Gets a random float value between min and max.
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <returns>A random float value.</returns>
        public static float Range(float min, float max) => (float)(_random.NextDouble() * (max - min) + min);

        /// <summary>
        /// Gets a random integer value between min and max.
        /// </summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        /// <returns>An random integer value.</returns>
        public static int Range(int min, int max) => _random.Next(min, max);
    }
}
