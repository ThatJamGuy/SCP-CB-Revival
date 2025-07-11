#if SORTIFY_ATTRIBUTES
using UnityEngine;

namespace Sortify
{
    public class MinMaxSliderAttribute : PropertyAttribute
    {
        public float Min;
        public float Max;

        /// <summary>
        /// Initializes a new instance of the MinMaxSliderAttribute with specified minimum and maximum values for the slider.
        /// </summary>
        /// <param name="min">The minimum value of the slider.</param>
        /// <param name="max">The maximum value of the slider.</param>
        public MinMaxSliderAttribute(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
#endif
