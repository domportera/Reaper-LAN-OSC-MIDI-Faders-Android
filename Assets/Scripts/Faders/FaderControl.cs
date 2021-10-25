using System;
using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
public class FaderControl : Controller, ISortingMember
{
    [SerializeField] Slider slider = null;
    [SerializeField] EventTrigger eventTrigger = null;
    [SerializeField] Text label = null;
    [SerializeField] Button sortLeftButton;
    [SerializeField] Button sortRightButton;

    public override void Initialize(ControllerData _controller, int whichIndex = 0)
    {
        Type controllerType = _controller.GetType();
        if(controllerType != ControlsManager.controllerClassesByControl[this.GetType()])
        {
            Debug.LogError($"Gave wrong controller data type {controllerType} to {this.GetType()}", this);
            return;
        }

        base.Initialize(_controller);
        label.text = _controller.GetName();
        name = _controller.GetName() + " " + controllerSettings.ReleaseBehavior;
        InitializeFaderInteraction();
        InitializeSorting();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        slider.SetValueWithoutNotify(modValue);
    }

    void InitializeFaderInteraction()
    {
        slider.maxValue = controllerSettings.Max;
        slider.minValue = controllerSettings.Min;
        slider.onValueChanged.AddListener(SetValue);

        EventTrigger.Entry startEntry = new EventTrigger.Entry();
        startEntry.eventID = EventTriggerType.PointerDown;
        startEntry.callback.AddListener((data) => { StartSliding(); });
        eventTrigger.triggers.Add(startEntry);

        EventTrigger.Entry endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) => { EndSliding(); });
        eventTrigger.triggers.Add(endEntry);
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
        sortLeftButton.onClick.AddListener(SortLeft);
        sortRightButton.onClick.AddListener(SortRight);
        SetSortButtonVisibility(false);
    }

    public void SetSortButtonVisibility(bool _visible)
    {
        sortLeftButton.gameObject.SetActive(_visible);
        sortRightButton.gameObject.SetActive(_visible);

        Image[] sliderImages = slider.GetComponentsInChildren<Image>();

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
