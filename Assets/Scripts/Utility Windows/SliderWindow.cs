using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SliderWindow : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] Text title;
    [SerializeField] Button closeButton;

    public void SetActions(string _title, float _defaultValue, float _min, float _max, UnityAction<float> _sliderAction, UnityAction _onClose = null)
    {
        if (_onClose != null)
        {
            closeButton.onClick.AddListener(_onClose);
        }

        closeButton.onClick.AddListener(Close);
        slider.minValue = _min;
        slider.maxValue = _max;
        slider.SetValueWithoutNotify(_defaultValue);
        slider.onValueChanged.AddListener(_sliderAction);
        title.text = _title;
    }

    void Close()
    {
        Destroy(gameObject);
    }
}
