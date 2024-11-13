using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OscCommandButton : MonoBehaviour
{
    [FormerlySerializedAs("button")] [SerializeField]
    private ButtonExtended _button;
    [FormerlySerializedAs("title")] [SerializeField]
    private Text _title;
    [FormerlySerializedAs("messagePreview")] [SerializeField]
    private Text _messagePreview;

    public void Initialize(Action buttonAction, Action longPressAction, string title, string messagePreview)
    {
        _button.OnClick.AddListener(() => { buttonAction.Invoke(); });
        _button.OnPointerHeld.AddListener(() => { longPressAction.Invoke(); });
        this._title.text = title;
        this._messagePreview.text = messagePreview;
        name = title + " OSC button";
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
