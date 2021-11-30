using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class ControllerOptionsPanel : MonoBehaviourExtended
{
    [SerializeField] protected InputField nameField;
    [SerializeField] protected Button applyButton;
    [SerializeField] protected Button closeButton;
    [SerializeField] protected Slider widthSlider;

    protected ControllerData controlData;
    protected RectTransform controlObjectTransform;

    private const int SLIDER_STEPS = 6;
    private int sliderStepsCorrected { get { return SLIDER_STEPS - 1; } }

    public UnityEvent OnWake = new UnityEvent();
    public UnityEvent OnSleep = new UnityEvent();

    protected void Awake()
    {
        nameField.onValueChanged.AddListener(RemoveProblemCharactersInNameField);
        applyButton.onClick.AddListener(Apply);
        closeButton.onClick.AddListener(Close);
        AwakeExtended();
    }

    protected void OnEnable()
    {
        OnWake.Invoke();
    }

    protected void OnDisable()
    {
        OnSleep.Invoke();
    }

    /// <summary>
    /// Provides ability to add functionality to the Awake method without being allowed to override it
    /// </summary>
    protected virtual void AwakeExtended(){}

    protected void BaseInitialize(ControllerData _data, RectTransform _controlObjectTransform)
    {
        controlObjectTransform = _controlObjectTransform;
        controlData = _data;
        nameField.SetTextWithoutNotify(controlData.GetName());
        InitializeWidthSlider();
        SetWidth(_data.GetWidth());
    }

    void RemoveProblemCharactersInNameField(string _input)
    {
        _input.Replace("\"", "");
        _input.Replace("\\", "");
        nameField.SetTextWithoutNotify(_input);
    }

    void SetControllerDataMasterVariables()
    {
        string controllerName = nameField.text;
        controlData.SetName(controllerName);

        float width = ConvertSliderValueToWidth((int)widthSlider.value);
        controlData.SetWidth(width);
    }

    protected virtual void Apply()
    {
        SetControllerDataMasterVariables();
        ControlsManager.instance.RespawnController(controlData);
        UtilityWindows.instance.QuickNoticeWindow("Settings applied!");
    }
    
    void Close()
    {
        gameObject.SetActive(false);
    }

    #region Width
    void InitializeWidthSlider()
    {
        Range<float> widthRange = controlData.GetWidthRange();
        widthSlider.wholeNumbers = true;
        widthSlider.minValue = ConvertWidthToSliderValue(widthRange.min);
        widthSlider.maxValue = ConvertWidthToSliderValue(widthRange.max);

        int width = ConvertWidthToSliderValue(controlData.GetWidth());
        widthSlider.SetValueWithoutNotify(width);
    }

    void SetWidth(float _width)
    {
        controlObjectTransform.sizeDelta = new Vector2(controlObjectTransform.sizeDelta.y * _width, controlObjectTransform.sizeDelta.y);
        UIManager.instance.RefreshFaderLayoutGroup();
    }

    //all this conversion stuff is to achieve the stepping slider with the multiplicative fractional width functionality
    int ConvertWidthToSliderValue(float _width)
    {
        Range<float> widthRange = controlData.GetWidthRange();
        return (int)_width.Map(widthRange.min, widthRange.max, 0, sliderStepsCorrected);
    }

    float ConvertSliderValueToWidth(int _value)
    {
        Range<float> widthRange = controlData.GetWidthRange();
        return ((float)_value).Map(0, sliderStepsCorrected, widthRange.min, widthRange.max);
    }
    #endregion Width

    bool VerifyUniqueName(string _s)
    {
        bool valid = true;
        ReadOnlyCollection<ControllerData> controllers = ControlsManager.instance.Controllers;

        foreach (ControllerData set in controllers)
        {
            if (set.GetName() == _s)
            {
                valid = false;
                break;
            }
        }

        if (!valid)
        {
            UtilityWindows.instance.ErrorWindow("Name should be unique - no two controllers in the same profile can have the same name.");
            return false;
        }

        return true;
    }
}
