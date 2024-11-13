using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class SliderWindow : MonoBehaviour
    {
        [FormerlySerializedAs("slider")] [SerializeField]
        private Slider _slider;
        [FormerlySerializedAs("title")] [SerializeField]
        private Text _title;
        [FormerlySerializedAs("valueText")] [SerializeField]
        private Text _valueText;
        [FormerlySerializedAs("descriptionText")] [SerializeField]
        private Text _descriptionText;
        [FormerlySerializedAs("closeButton")] [SerializeField]
        private Button _closeButton;
        [FormerlySerializedAs("cancelButton")] [SerializeField]
        private Button _cancelButton;
        [FormerlySerializedAs("confirmButton")] [SerializeField]
        private Button _confirmButton;

        public void SetActions(string title, string description, float defaultValue, float min, float max, bool roundToInt, UnityAction<float> onConfirm, UnityAction onCancel = null)
        {
            if (onCancel != null)
            {
                _closeButton.onClick.AddListener(onCancel);
                _cancelButton.onClick.AddListener(onCancel);
            }
            else
            {
                _cancelButton.gameObject.SetActive(false);
            }

            _closeButton.onClick.AddListener(Close);
            _cancelButton.onClick.AddListener(Close);

            _confirmButton.onClick.AddListener(() => onConfirm?.Invoke(_slider.value));

            _slider.minValue = min;
            _slider.maxValue = max;

            _slider.SetValueWithoutNotify(roundToInt ? Mathf.RoundToInt(defaultValue) : defaultValue);

            _slider.onValueChanged.AddListener((float f) => SetValueText(f, roundToInt));

        
            if(roundToInt)
            {
                _slider.onValueChanged.AddListener((float f) => _slider.value = Mathf.RoundToInt(f));
            }

            this._title.text = title;
            _descriptionText.text = description;

            SetValueText(_slider.value, roundToInt);
        }


        private void SetValueText(float input, bool roundToInt)
        {
            if(roundToInt)
            {
                _valueText.text = Mathf.RoundToInt(input).ToString();
            }
            else
            {
                _valueText.text = input.ToString("f2");
            }
        }

        private void Close()
        {
            Destroy(gameObject);
        }
    }
}
