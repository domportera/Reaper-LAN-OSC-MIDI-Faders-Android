using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Serialization;
using PopUpWindows;

public abstract class ControllerOptionsPanel : MonoBehaviour
{
    [SerializeField] private ControlsManager _controlsManager;
    [FormerlySerializedAs("nameField")] [SerializeField] protected InputField NameField;
    [FormerlySerializedAs("applyButton")] [SerializeField] protected Button ApplyButton;
    [FormerlySerializedAs("closeButton")] [SerializeField] protected Button CloseButton;
    [FormerlySerializedAs("widthSlider")] [SerializeField] protected Slider WidthSlider;

    protected ControllerData ControlData;
    protected RectTransform ControlObjectTransform;

    public const int SliderSteps = 7;
    int SliderStepsCorrected { get { return SliderSteps - 1; } }

    public UnityEvent OnWake = new();
    public UnityEvent OnSleep = new();

    protected void Awake()
    {
        NameField.onValueChanged.AddListener(RemoveProblemCharactersInNameField);
        ApplyButton.onClick.AddListener(Apply);
        CloseButton.onClick.AddListener(Close);
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

    protected void BaseInitialize(ControllerData data, RectTransform controlObjectTransform)
    {
        ControlObjectTransform = controlObjectTransform;
        ControlData = data;
        NameField.SetTextWithoutNotify(ControlData.GetName());
        InitializeWidthSlider(data);
        SetWidth(data.GetWidth());
    }

    void RemoveProblemCharactersInNameField(string input)
    {
        input.Replace("\"", "");
        input.Replace("\\", "");
        NameField.SetTextWithoutNotify(input);
    }

    void SetControllerDataMasterVariables()
    {
        string controllerName = NameField.text;
        ControlData.SetName(controllerName);

        float width = ConvertSliderValueToWidth((int)WidthSlider.value);
        ControlData.SetWidth(width);
    }

    protected virtual void Apply()
    {
        SetControllerDataMasterVariables();
        _controlsManager.RespawnController(ControlData);
        PopUpController.Instance.QuickNoticeWindow("Settings applied!");
    }
    
    void Close()
    {
        gameObject.SetActive(false);
    }

    #region Width
    void InitializeWidthSlider(ControllerData _data)
    { 
        var widthRange = _data.GetWidthRange();
        WidthSlider.wholeNumbers = true;
        WidthSlider.minValue = ConvertWidthToSliderValue(widthRange.min);
        WidthSlider.maxValue = ConvertWidthToSliderValue(widthRange.max);

        int width = ConvertWidthToSliderValue(_data.GetWidth());
        WidthSlider.SetValueWithoutNotify(width);
    }

    void SetWidth(float _width)
    {
        ControlObjectTransform.sizeDelta = new Vector2(ControlObjectTransform.sizeDelta.y * _width, ControlObjectTransform.sizeDelta.y);
        UIManager.Instance.RefreshFaderLayoutGroup();
    }

    //all this conversion stuff is to achieve the stepping slider with the multiplicative fractional width functionality
    int ConvertWidthToSliderValue(float _width)
    {
        var widthRange = ControlData.GetWidthRange();
        return (int)_width.Map(widthRange.min, widthRange.max, 0, SliderStepsCorrected);
    }

    float ConvertSliderValueToWidth(int _value)
    {
        var widthRange = ControlData.GetWidthRange();
        return ((float)_value).Map(0, SliderStepsCorrected, widthRange.min, widthRange.max);
    }
    #endregion Width

    bool VerifyUniqueName(string s)
    {
        bool valid = true;
        ReadOnlyCollection<ControllerData> controllers = _controlsManager.Controllers;

        foreach (ControllerData set in controllers)
        {
            if (set.GetName() != s)
                continue;
            
            valid = false;
            break;
        }

        if (valid) return true;
        
        PopUpController.Instance.ErrorWindow("Name should be unique - no two controllers in the same profile can have the same name.");
        return false;

    }
}
