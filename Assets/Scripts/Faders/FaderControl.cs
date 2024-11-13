using System;
using OscJack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
public class FaderControl : Controller, ISortingMember
{
    [FormerlySerializedAs("slider")] [SerializeField] Slider _slider = null;
    [FormerlySerializedAs("eventTrigger")] [SerializeField] EventTrigger _eventTrigger = null;
    [FormerlySerializedAs("label")] [SerializeField] Text _label = null;
    [FormerlySerializedAs("sortLeftButton")] [SerializeField] Button _sortLeftButton;
    [FormerlySerializedAs("sortRightButton")] [SerializeField] Button _sortRightButton;

    public override void Initialize(ControllerData _controller, int whichIndex = 0)
    {
        Type controllerType = _controller.GetType();
        if(controllerType != ControlsManager.ControllerClassesByControl[this.GetType()])
        {
            Debug.LogError($"Gave wrong controller data type {controllerType} to {this.GetType()}", this);
            return;
        }

        base.Initialize(_controller);
        _label.text = _controller.GetName();
        name = _controller.GetName() + " Fader";
        InitializeFaderInteraction();
        InitializeSorting();
    }

    // Update is called once per frame
    protected void Update()
    {
        _slider.SetValueWithoutNotify(SmoothValue);
    }

    void InitializeFaderInteraction()
    {
        _slider.maxValue = MaxControllerValue;
        _slider.minValue = MinControllerValue;
        _slider.onValueChanged.AddListener(SetValue);

        EventTrigger.Entry startEntry = new EventTrigger.Entry();
        startEntry.eventID = EventTriggerType.PointerDown;
        startEntry.callback.AddListener((data) => { StartSliding(); });
        _eventTrigger.triggers.Add(startEntry);

        EventTrigger.Entry endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) => { EndSliding(); });
        _eventTrigger.triggers.Add(endEntry);
    }


    void StartSliding()
    {

    }

    void EndSliding()
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

    public void SetSortButtonVisibility(bool _visible)
    {
        _sortLeftButton.gameObject.SetActive(_visible);
        _sortRightButton.gameObject.SetActive(_visible);

        Image[] sliderImages = _slider.GetComponentsInChildren<Image>();

        foreach (Image i in sliderImages)
        {
            i.enabled = !_visible;
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

    public void SortPosition(bool _right)
    {
        transform.SetSiblingIndex(_right ? transform.GetSiblingIndex() + 1 : transform.GetSiblingIndex() - 1);
    }
    #endregion Sorting
}
