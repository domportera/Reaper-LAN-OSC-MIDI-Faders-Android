using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OSCCommandButton : MonoBehaviour
{
    [FormerlySerializedAs("button")] [SerializeField] ButtonExtended _button;
    [FormerlySerializedAs("title")] [SerializeField] Text _title;
    [FormerlySerializedAs("messagePreview")] [SerializeField] Text _messagePreview;

    public void Initialize(Action _buttonAction, Action _longPressAction, string _title, string _messagePreview)
    {
        _button.OnClick.AddListener(() => { _buttonAction.Invoke(); });
        _button.OnPointerHeld.AddListener(() => { _longPressAction.Invoke(); });
        this._title.text = _title;
        this._messagePreview.text = _messagePreview;
        name = _title + " OSC button";
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
