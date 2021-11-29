using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSCCommandButton : MonoBehaviour
{
    [SerializeField] ButtonExtended button;
    [SerializeField] Text title;
    [SerializeField] Text messagePreview;

    public void Initialize(Action _buttonAction, Action _longPressAction, string _title, string _messagePreview)
    {
        button.onClick.AddListener(() => { _buttonAction.Invoke(); });
        button.OnPointerHeld.AddListener(() => { _longPressAction.Invoke(); });
        title.text = _title;
        messagePreview.text = _messagePreview;
        name = _title + " OSC button";
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
