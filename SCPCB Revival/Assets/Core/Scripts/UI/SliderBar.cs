using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace scpcbr {
    /// <summary>
    /// A script that provides a simple system to easily create SCP:CB style meters and bars by utilizing invisible sliders to determine the value of the
    /// bar and which segments of the bar should be enabled to represent this.
    /// </summary>
    public class SliderBar : MonoBehaviour {
        // Two simple things that need to be defined. The progress of the slider can be influenced by code through a reference for the slider value.
        // Since that already exists on the Slider itself, there will not be a function to set the sliders value through code on this script.

        [Header("Slider Bar Variables")]
        [SerializeField] private Image[] barSegments;
        [SerializeField] private Slider barSlider;

        private void Start() {
            foreach (var segment in barSegments) segment.enabled = false;
        }

        private void Update() {
            UpdateSliderBar(barSlider.value);
        }

        private void UpdateSliderBar(float progress) {
            var filledSegmentsCount = Mathf.FloorToInt(progress * barSegments.Length);
            var filledSegments = barSegments.Take(filledSegmentsCount).ToList();
            var unfilledSegments = barSegments.Skip(filledSegmentsCount).ToList();

            filledSegments.ForEach(segment => segment.enabled = true);
            unfilledSegments.ForEach(segment => segment.enabled = false);
        }
    }
}