using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonExtended : Button
{
    const float DEFAULT_HOLD_STATE = float.MaxValue;
    float pressTime = DEFAULT_HOLD_STATE;

    float holdTime = 0.8f;
    bool ignorePointerUp = false;

    //hiding the original event so we can allow the button color to change states as normal, even if we ignore calling the onClick stuff in OnPointerUp
    public new ButtonClickedEvent onClick = new ButtonClickedEvent(); 

    public ButtonExtendedEvent OnPointerHeld = new ButtonExtendedEvent();

    //needed to make a separate class since the UnityEvent class's functions are not virtual
    public class ButtonExtendedEvent
    {
        UnityEvent myEvent = new UnityEvent();
        public int subscriptionCount { get; private set; }

        public void AddListener(UnityAction _action)
        {
            myEvent.AddListener(_action);
            subscriptionCount++;
        }

        public void RemoveListener(UnityAction _action)
        {
            myEvent.RemoveListener(_action);
            subscriptionCount--;
        }

        public void RemoveAllListeners()
        {
            myEvent.RemoveAllListeners();
            subscriptionCount = 0;
        }

        public void Invoke()
        {
            myEvent.Invoke();
        }
        
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        pressTime = DEFAULT_HOLD_STATE;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        pressTime = Time.time;
        ignorePointerUp = false;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if(OnPointerHeld.subscriptionCount == 0 || !ignorePointerUp)
        {
            onClick.Invoke();
        }

        pressTime = DEFAULT_HOLD_STATE;
    }

    private void Update()
    {
        ButtonLongPressCheck();
    }

    private void ButtonLongPressCheck()
    {
        if(pressTime == DEFAULT_HOLD_STATE)
        {
            return;
        }

        if(Time.time - pressTime > holdTime)
        {
            ignorePointerUp = true;
            OnPointerHeld.Invoke();
            pressTime = DEFAULT_HOLD_STATE;
        }
    }

    public void SetHoldtime(float _seconds)
    {
        holdTime = _seconds;
    }
}
