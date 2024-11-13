using System.Collections.ObjectModel;
using System.Linq;
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
    private int SliderStepsCorrected { get { return SliderSteps - 1; } }

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

    private void RemoveProblemCharactersInNameField(string input)
    {
        input.Replace("\"", "");
        input.Replace("\\", "");
        NameField.SetTextWithoutNotify(input);
    }

    private void SetControllerDataMasterVariables()
    {
        var controllerName = NameField.text;
        ControlData.SetName(controllerName);

        var width = ConvertSliderValueToWidth((int)WidthSlider.value);
        ControlData.SetWidth(width);
    }

    protected virtual void Apply()
    {
        SetControllerDataMasterVariables();
        _controlsManager.RespawnController(ControlData);
        PopUpController.Instance.QuickNoticeWindow("Settings applied!");
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }

    #region Width

    private void InitializeWidthSlider(ControllerData data)
    { 
        var widthRange = data.GetWidthRange();
        WidthSlider.wholeNumbers = true;
        WidthSlider.minValue = ConvertWidthToSliderValue(widthRange.Min);
        WidthSlider.maxValue = ConvertWidthToSliderValue(widthRange.Max);

        var width = ConvertWidthToSliderValue(data.GetWidth());
        WidthSlider.SetValueWithoutNotify(width);
    }

    private void SetWidth(float width)
    {
        ControlObjectTransform.sizeDelta = new Vector2(ControlObjectTransform.sizeDelta.y * width, ControlObjectTransform.sizeDelta.y);
        UIManager.Instance.RefreshFaderLayoutGroup();
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
