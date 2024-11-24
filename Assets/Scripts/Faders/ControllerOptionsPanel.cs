using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PopUpWindows;

public abstract class ControllerOptionsPanel : MonoBehaviour
{
    [SerializeField] private ControlsManager _controlsManager;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] protected InputField NameField;
    [SerializeField] protected Button ApplyButton;
    [SerializeField] protected Button CloseButton;
    [SerializeField] protected Slider WidthSlider;

    private ControllerData ControlData;

    public const int SliderSteps = 7;
    private static int SliderStepsCorrected => SliderSteps - 1;

    public event Action OnWake;
    public event Action OnSleep;
    
    private bool IsInitialized => ControlData != null;

    protected void Awake()
    {
        if (!_controlsManager)
        {
            _controlsManager = FindFirstObjectByType<ControlsManager>();
            if (!_controlsManager)
            {
                enabled = false;
                Debug.LogError("ControlsManager not found");
                return;
            }
        }
        NameField.onValueChanged.AddListener(RemoveProblemCharactersInNameField);
        ApplyButton.onClick.AddListener(Apply);
        CloseButton.onClick.AddListener(Close);
    }

    protected void OnEnable()
    {
        OnWake?.Invoke();
        if (IsInitialized)
            SetWidthSliderToControllerWidth();
    }

    protected void OnDisable()
    {
        OnSleep?.Invoke();
    }

    protected void BaseInitialize(ControllerData data)
    {
        ControlData = data;
        NameField.SetTextWithoutNotify(ControlData.GetName());
        InitializeWidthSlider();
    }

    private void RemoveProblemCharactersInNameField(string input)
    {
        input.Replace("\"", "");
        input.Replace("\\", "");
        NameField.SetTextWithoutNotify(input);
    }


    protected virtual void Apply()
    {
        var controllerName = NameField.text;
        ControlData.SetName(controllerName);

        var width = ConvertSliderValueToWidth((int)WidthSlider.value);
        ControlData.SetWidth(width);
        UIManager.Instance.RefreshFaderLayoutGroup();
        _controlsManager.RespawnController(ControlData);
        PopUpController.Instance.QuickNoticeWindow("Settings applied!");
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    #region Width

    private void InitializeWidthSlider()
    { 
        var widthRange = ControlData.GetWidthRange();
        WidthSlider.wholeNumbers = true;
        WidthSlider.minValue = ConvertWidthToSliderValue(widthRange.Min);
        WidthSlider.maxValue = ConvertWidthToSliderValue(widthRange.Max);
        SetWidthSliderToControllerWidth();
    }

    protected void SetWidthSliderToControllerWidth()
    {
        var width = ConvertWidthToSliderValue(ControlData.GetWidth());
        WidthSlider.SetValueWithoutNotify(width);
    }

    //all this conversion stuff is to achieve the stepping slider with the multiplicative fractional width functionality
    private int ConvertWidthToSliderValue(float width)
    {
        var widthRange = ControlData.GetWidthRange();
        return (int)width.Map(widthRange.Min, widthRange.Max, 0, SliderStepsCorrected);
    }

    private float ConvertSliderValueToWidth(int value)
    {
        var widthRange = ControlData.GetWidthRange();
        return ((float)value).Map(0, SliderStepsCorrected, widthRange.Min, widthRange.Max);
    }
    #endregion Width

    private bool VerifyUniqueName(string s)
    {
        var invalid = _controlsManager.Controllers.Any(x => x.GetName() == s);

        if (!invalid) return true;
        
        PopUpController.Instance.ErrorWindow("Name should be unique - no two controllers in the same profile can have the same name.");
        return false;
    }
}
