using System.Collections.Generic;
using _Project.Scripts.Systems;
using DrumRhythmGame.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class ErrorSettingPanel : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Configuration")] [SerializeField]
        private KeyCode sliderSnappingKey = KeyCode.LeftShift;

        [Header("UI Elements")] [SerializeField]
        private Slider missHitFrequencyRateSlider;

        [SerializeField] private Slider latencyFrequencyRateSlider;

        [SerializeField] private Slider maxLatencyTimeSlider;

        [SerializeField] private Dropdown presetDropdown;
        [SerializeField] private Button zeroAllButton;

        [SerializeField] private TextMeshProUGUI latencyFrequencyRateText;
        [SerializeField] private TextMeshProUGUI maxLatencyTimeText;
        [SerializeField] private TextMeshProUGUI missHitFrequencyRateText;

        #endregion

        #region Event Functions

        private void Awake()
        {
            SetupSlidersWithSavedValues();
        }


        private void OnEnable()
        {
            SetupListeners();
        }

        private void OnDisable()
        {
            missHitFrequencyRateSlider.onValueChanged.RemoveAllListeners();
            latencyFrequencyRateSlider.onValueChanged.RemoveAllListeners();
            maxLatencyTimeSlider.onValueChanged.RemoveAllListeners();
            presetDropdown.onValueChanged.RemoveAllListeners();
        }

        #endregion

        /// <summary>
        ///     Visually updates the sliders with the saved values.
        /// </summary>
        private void SetupSlidersWithSavedValues()
        {
            missHitFrequencyRateSlider.value = SaveData.Instance.partnerErrorData.Current.missHitFrequencyRate;
            latencyFrequencyRateSlider.value = SaveData.Instance.partnerErrorData.Current.latencyFrequencyRate;
            maxLatencyTimeSlider.value = SaveData.Instance.partnerErrorData.Current.maxLatencyTime;

            latencyFrequencyRateText.text = $"{latencyFrequencyRateSlider.value * 100:F0}%";
            maxLatencyTimeText.text = $"{maxLatencyTimeSlider.value:F2}";
            missHitFrequencyRateText.text = $"{missHitFrequencyRateSlider.value * 100:F0}%";
        }

        private void SetupListeners()
        {
            // TODO: Could probably extract the listeners into a separate method
            missHitFrequencyRateSlider.onValueChanged.AddListener(rate =>
            {
                var value = GetRoundedValue(rate);
                missHitFrequencyRateSlider.value = value;
                SaveData.Instance.partnerErrorData.Current.missHitFrequencyRate = value;
                missHitFrequencyRateText.text = $"{value * 100:F0}%";
            });
            latencyFrequencyRateSlider.onValueChanged.AddListener(rate =>
            {
                var value = GetRoundedValue(rate);
                latencyFrequencyRateSlider.value = value;
                SaveData.Instance.partnerErrorData.Current.latencyFrequencyRate = value;
                latencyFrequencyRateText.text = $"{value * 100:F0}%";
            });
            maxLatencyTimeSlider.onValueChanged.AddListener(time =>
            {
                var value = GetRoundedValue(time);
                maxLatencyTimeSlider.value = value;
                SaveData.Instance.partnerErrorData.Current.maxLatencyTime = value;
                maxLatencyTimeText.text = $"{value:F2}";
            });
            
            // Resets all of the sliders to 0
            zeroAllButton.onClick.AddListener(() =>
            {
                SetErrors(0, 0, 0);
                // TODO: Convert this to a singleton and use Instance instead
                GetComponent<PromptPositionSettingPanel>().Reset();
                GetComponent<AvatarPositionOffset>().Reset();
            });

            // Initialize preset dropdown
            var options = new List<Dropdown.OptionData>();
            for (var i = 0; i < SaveData.Instance.partnerErrorData.PresetLength; i++)
                options.Add(new Dropdown.OptionData($"Preset {i + 1}"));
            presetDropdown.options = options;
            presetDropdown.onValueChanged.AddListener(value =>
            {
                SaveData.Instance.partnerErrorData.UpdateCurrentIndex(value);
                SetErrors(
                    SaveData.Instance.partnerErrorData.Current.missHitFrequencyRate,
                    SaveData.Instance.partnerErrorData.Current.latencyFrequencyRate,
                    SaveData.Instance.partnerErrorData.Current.maxLatencyTime);
            });
            presetDropdown.value = SaveData.Instance.partnerErrorData.CurrentIndex;
        }

        /// <summary>
        ///     Clamps the passed value to the rounding factor (default 10), if the snapping key is pressed.
        /// </summary>
        /// <param name="input">The value to round</param>
        /// <param name="roundingFactor">The factor to which we round</param>
        /// <returns></returns>
        private float GetRoundedValue(float input, int roundingFactor = 10)
        {
            return Input.GetKey(sliderSnappingKey)
                ? Mathf.Round(input * roundingFactor) / roundingFactor
                : input;
        }

        /// <summary>
        ///     Set all of error values. This invokes each onValueChanged event.
        /// </summary>
        /// <param name="missHitFrequencyRate"></param>
        /// <param name="latencyFrequencyRate"></param>
        /// <param name="maxLatencyTime"></param>
        private void SetErrors(float missHitFrequencyRate, float latencyFrequencyRate, float maxLatencyTime)
        {
            missHitFrequencyRateSlider.value = missHitFrequencyRate;
            latencyFrequencyRateSlider.value = latencyFrequencyRate;
            maxLatencyTimeSlider.value = maxLatencyTime;
        }
    }
}