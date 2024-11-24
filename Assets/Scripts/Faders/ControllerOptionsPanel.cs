using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using PopUpWindows;

public abstract class ControllerOptionsPanel : MonoBehaviour
{
    private static ControlsManager _controlsManager;
    [SerializeField] protected InputField NameField;
    [SerializeField] protected Button ApplyButton;
    [SerializeField] protected Button CloseButton;
    [SerializeField] protected Slider WidthSlider;

    private ControllerData _controlData;

    private const int WidthSliderSteps = 7;
    private const int WidthSliderStepsMax = WidthSliderSteps - 1;

    public event Action OnWake;
    public event Action OnSleep;
    
    private bool IsInitialized => _controlData != null;

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
        CloseButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    protected void OnEnable()
    {
        OnWake?.Invoke();
        if (IsInitialized)
        {
            ResetUiFields();
        }
    }

    protected void OnDisable()
    {
        OnSleep?.Invoke();
    }

    public void ResetUiFields()
    {
        var width = ConvertWidthToSliderValue(_controlData.GetWidth());
        WidthSlider.SetValueWithoutNotify(width);
        NameField.SetTextWithoutNotify(_controlData.GetName());
    }

    protected void BaseInitialize(ControllerData data)
    {
        _controlData = data;
        InitializeWidthSlider();
        NameField.characterValidation = InputField.CharacterValidation.None;
        ResetUiFields();
    }

    private void RemoveProblemCharactersInNameField(string input)
    {
        input = input.Replace("\"", "").Replace("\\", "");
        NameField.SetTextWithoutNotify(input);
    }

    protected virtual void Apply()
    {
        if (!VerifyUniqueName(NameField.text))
            return;
        var controllerName = NameField.text;
        _controlData.SetName(controllerName);

        var width = ConvertSliderValueToWidth((int)WidthSlider.value);
        _controlData.SetWidth(width);
        UIManager.Instance.RefreshFaderLayoutGroup();
        _controlsManager.RespawnController(_controlData);
        PopUpController.Instance.QuickNoticeWindow("Settings applied!");
    }

    #region Width

    private void InitializeWidthSlider()
    { 
        var widthRange = _controlData.GetWidthRange();
        WidthSlider.wholeNumbers = true;
        WidthSlider.minValue = ConvertWidthToSliderValue(widthRange.Min);
        WidthSlider.maxValue = ConvertWidthToSliderValue(widthRange.Max);
    }

    //all this conversion stuff is to achieve the stepping slider with the multiplicative fractional width functionality
    private int ConvertWidthToSliderValue(float width)
    {
        var widthRange = _controlData.GetWidthRange();
        return (int)width.Map(widthRange.Min, widthRange.Max, 0, WidthSliderStepsMax);
    }

    private float ConvertSliderValueToWidth(int value)
    {
        var widthRange = _controlData.GetWidthRange();
        return ((float)value).Map(0, WidthSliderStepsMax, widthRange.Min, widthRange.Max);
    }
    #endregion Width

    private bool VerifyUniqueName(string s)
    {
        var invalid = _controlsManager.Controllers
            .Where(x => x != _controlData)
            .Any(x => x.GetName() == s);

        if (!invalid) return true;
        
        PopUpController.Instance.ErrorWindow("Name should be unique - no two controllers in the same profile can have the same name.");
        return false;
    }
}
