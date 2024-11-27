using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
// ReSharper disable ConvertClosureToMethodGroup

public class OscCommandButton : MonoBehaviour
{
    [FormerlySerializedAs("button")] [SerializeField]
    private ButtonExtended _button;
    [FormerlySerializedAs("title")] [SerializeField]
    private Text _title;
    [FormerlySerializedAs("messagePreview")] [SerializeField]
    private Text _messagePreview;

    public void Initialize(Action pressAction, Action longPressAction, string title, string messagePreview)
    {
        _button.OnClick.AddListener(() => pressAction.Invoke());
        _button.OnPointerHeld.AddListener(() => longPressAction.Invoke());
        _title.text = title;
        _messagePreview.text = messagePreview;
        name = title + " OSC button";
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
