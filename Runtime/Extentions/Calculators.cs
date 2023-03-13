
namespace HappyPixels.EditorAddons.Extensions
{
    public static class Calculators
    {
        /// <summary>
        /// Normalizes any range between 0 and 1
        /// </summary>
        /// <param name="value">Value to be normalized</param>
        /// <param name="min">Minimum value in the range</param>
        /// <param name="max">Maximum value in the range</param>
        /// <returns>Normalized value between 0 and 1</returns>
        public static float Normalize01(float value, float min, float max) => 
            (value - min) / (max - min);

        /// <summary>
        /// If value was between 0 and 1, given the min and max it will be scaled back to it's original value
        /// </summary>
        /// <param name="value">Normalized value between 0 and 1 to be denormalized</param>
        /// <param name="min">Minimum value in the range</param>
        /// <param name="max">Maximum value in the range</param>
        /// <returns></returns>
        public static float ReverseNormalization01(float value, float min, float max) =>
            value * (max - min) + min;
    }
}
