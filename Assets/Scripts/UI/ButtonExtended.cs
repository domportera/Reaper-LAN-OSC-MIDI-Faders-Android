using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonExtended : Button, IBeginDragHandler, IEndDragHandler
{
    private float _pressTime;

    private const float DefaultPressTime = float.MaxValue;

    private const float HoldTime = 0.8f;

    private bool _ignoreClick;
    private bool _isDragging;
    private bool _couldBeHolding;

    public RectTransform RectTransform => (RectTransform)transform;

    //hiding the original event so we can allow the button color to change states as normal, even if we ignore calling the onClick stuff in OnPointerUp
    [FormerlySerializedAs("onClick")] public ButtonClickedEvent OnClick = new (); 

    public readonly ButtonExtendedEvent OnPointerHeld = new ();

    //needed to make a separate class since the UnityEvent class's functions are not virtual
    public class ButtonExtendedEvent
    {
        private readonly UnityEvent _myEvent = new ();
        public int SubscriptionCount { get; private set; }

        public void AddListener(UnityAction action)
        {
            _myEvent.AddListener(action);
            SubscriptionCount++;
        }

        public void RemoveListener(UnityAction action)
        {
            _myEvent.RemoveListener(action);
            SubscriptionCount--;
        }

        public void RemoveAllListeners()
        {
            _myEvent.RemoveAllListeners();
            SubscriptionCount = 0;
        }

        public void Invoke() => _myEvent.Invoke();
        
    }

    public void OnBeginDrag(PointerEventData data)
    {
        _couldBeHolding = false;
        _pressTime = DefaultPressTime;
        _isDragging = true;
    }

    public void OnEndDrag(PointerEventData data)
    {
        _isDragging = false;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        _couldBeHolding = false;
        _pressTime = DefaultPressTime;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        _pressTime = Time.time;
        _ignoreClick = false;
        _couldBeHolding = true;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if(_isDragging)
        {
            return;
        }

        if(OnPointerHeld.SubscriptionCount == 0 || !_ignoreClick)
        {
            OnClick.Invoke();
        }

        _pressTime = DefaultPressTime;
    }

    private void Update()
    {
        ButtonLongPressCheck();
    }

    private void ButtonLongPressCheck()
    {
        if(!_couldBeHolding)
            return;

        var held = Time.time - _pressTime > HoldTime;
        
        if (!held)
            return;
        
        _ignoreClick = true;
        OnPointerHeld.Invoke();
        _pressTime = DefaultPressTime;
    }
}
