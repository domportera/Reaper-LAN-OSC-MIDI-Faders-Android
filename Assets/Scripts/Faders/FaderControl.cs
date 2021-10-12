using System;
using System.Collections;
using System.Collections.Generic;
using OscJack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(OscPropertySender))]
public class FaderControl : Controller
{
    [SerializeField] Slider slider = null;
    [SerializeField] EventTrigger eventTrigger = null;
    [SerializeField] Text label = null;

    public override void Initialize(ControlsManager.ControllerData _controller, int whichIndex = 0)
    {
        Type controllerType = _controller.GetType();
        if(controllerType != ControlsManager.controllerClassesByControl[this.GetType()])
        {
            Debug.LogError($"Gave wrong controller data type {controllerType} to {this.GetType()}", this);
            return;
        }

        base.Initialize(_controller);
        label.text = _controller.GetName();
        name = _controller.GetName() + " " + controllerSettings.controlType;
        InitializeFaderInteraction();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        slider.SetValueWithoutNotify(modValue);
    }

    public override void SetSortButtonVisibility(bool _visible)
    {
        base.SetSortButtonVisibility(_visible);

        Image[] sliderImages = slider.GetComponentsInChildren<Image>();

        foreach (Image i in sliderImages)
        {
            i.enabled = !_visible;
        }
    }

    void InitializeFaderInteraction()
    {
        slider.maxValue = controllerSettings.max;
        slider.minValue = controllerSettings.min;
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
}
