using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public sealed class FaderControlUi : MonoBehaviour, ISortingMember
{
    private AxisController _axisController;
    [SerializeField] private RectTransform _rootTransform;
    [SerializeField] private Slider _slider;
    [SerializeField] private EventTrigger _eventTrigger;
    [SerializeField] private Text _label;
    [SerializeField] private Button _sortLeftButton;
    [SerializeField] private Button _sortRightButton;

    private void Awake()
    {
        if(!_rootTransform)
            _rootTransform = GetComponent<RectTransform>();
    }
    
    public void Initialize(FaderData controlData)
    {
        if (!_rootTransform)
            _rootTransform = GetComponent<RectTransform>();
        _axisController = new AxisController(controlData.Settings);
        var rectTransform = GetComponent<RectTransform>();
        var initialSizeDelta = rectTransform.sizeDelta;

        var displayName = controlData.Name;
        _label.text = displayName;
        name = displayName + " Fader";
        InitializeFaderInteraction();
        InitializeSortingButtons();
        
        var width = controlData.Width;
        rectTransform.sizeDelta = new Vector2(initialSizeDelta.x * width, initialSizeDelta.y);
    }

    // Update is called once per frame
    private void Update()
    {
        _axisController.Update(Time.deltaTime);
        _slider.SetValueWithoutNotify(_axisController.SmoothValue);
    }

    private void InitializeFaderInteraction()
    {
        _slider.maxValue = AxisController.MaxControllerValue;
        _slider.minValue = AxisController.MinControllerValue;
        _slider.onValueChanged.AddListener(f => _axisController.SetValue(f));

        var startEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        startEntry.callback.AddListener(_ => StartSliding());
        _eventTrigger.triggers.Add(startEntry);

        var endEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };
        endEntry.callback.AddListener(_ => EndSliding());
        _eventTrigger.triggers.Add(endEntry);
    }


    private void StartSliding()
    {

    }

    private void EndSliding()
    {
        _axisController.Release();
    }

    #region Sorting
    private void InitializeSortingButtons()
    {
        _sortLeftButton.onClick.AddListener(() => _rootTransform.SetSiblingIndex(_rootTransform.GetSiblingIndex() - 1));
        _sortRightButton.onClick.AddListener(() => _rootTransform.SetSiblingIndex(_rootTransform.GetSiblingIndex() + 1));
    }

    public void SetSortButtonVisibility(bool visible)
    {
        _sortLeftButton.gameObject.SetActive(visible);
        _sortRightButton.gameObject.SetActive(visible);

        var sliderImages = _slider.GetComponentsInChildren<Image>();

        foreach (var i in sliderImages)
        {
            i.enabled = !visible;
        }
    }

    public RectTransform RectTransform => _rootTransform;

    #endregion Sorting
}
