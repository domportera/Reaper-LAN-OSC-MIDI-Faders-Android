using System;
using OscJack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
public class FaderControl : Controller, ISortingMember
{
    [FormerlySerializedAs("slider")] [SerializeField]
    private Slider _slider = null;
    [FormerlySerializedAs("eventTrigger")] [SerializeField]
    private EventTrigger _eventTrigger = null;
    [FormerlySerializedAs("label")] [SerializeField]
    private Text _label = null;
    [FormerlySerializedAs("sortLeftButton")] [SerializeField]
    private Button _sortLeftButton;
    [FormerlySerializedAs("sortRightButton")] [SerializeField]
    private Button _sortRightButton;

    public override void Initialize(ControllerData controller, int whichIndex = 0)
    {
        var controllerType = controller.GetType();
        if(controllerType != ControlsManager.ControllerClassesByControl[this.GetType()])
        {
            Debug.LogError($"Gave wrong controller data type {controllerType} to {this.GetType()}", this);
            return;
        }

        base.Initialize(controller);
        _label.text = controller.GetName();
        name = controller.GetName() + " Fader";
        InitializeFaderInteraction();
        InitializeSorting();
    }

    // Update is called once per frame
    protected void Update()
    {
        _slider.SetValueWithoutNotify(SmoothValue);
    }

    private void InitializeFaderInteraction()
    {
        _slider.maxValue = MaxControllerValue;
        _slider.minValue = MinControllerValue;
        _slider.onValueChanged.AddListener(SetValue);

        var startEntry = new EventTrigger.Entry();
        startEntry.eventID = EventTriggerType.PointerDown;
        startEntry.callback.AddListener((data) => { StartSliding(); });
        _eventTrigger.triggers.Add(startEntry);

        var endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) => { EndSliding(); });
        _eventTrigger.triggers.Add(endEntry);
    }


    private void StartSliding()
    {

    }

    private void EndSliding()
    {
        ReturnToCenter();
    }

    #region Sorting
    public void InitializeSorting()
    {
        _sortLeftButton.onClick.AddListener(SortLeft);
        _sortRightButton.onClick.AddListener(SortRight);
        SetSortButtonVisibility(false);
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

    public void SortLeft()
    {
        SortPosition(false);
    }

    public void SortRight()
    {
        SortPosition(true);
    }

    public void SortPosition(bool right)
    {
        transform.SetSiblingIndex(right ? transform.GetSiblingIndex() + 1 : transform.GetSiblingIndex() - 1);
    }
    #endregion Sorting
}
