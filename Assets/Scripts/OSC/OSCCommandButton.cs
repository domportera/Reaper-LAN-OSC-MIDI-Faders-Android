using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OSCCommandButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Text title;
    [SerializeField] Text messagePreview;

    public void Initialize(Action buttonAction, string _title, string _messagePreview)
    {
        button.onClick.AddListener(() => { buttonAction.Invoke(); });
        title.text = _title;
        messagePreview.text = _messagePreview;
    }
}
