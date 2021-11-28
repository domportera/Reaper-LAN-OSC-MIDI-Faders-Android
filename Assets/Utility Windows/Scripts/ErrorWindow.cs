using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ErrorWindow : MonoBehaviour
{

    [SerializeField] Text errorText = null;
    [SerializeField] Button closeButton = null;

    private void Awake()
    {
        closeButton.onClick.AddListener(HideErrorWindow);
    }

    public void Initialize(string _text, UnityAction _onClose)
    {
        errorText.text = _text;
        if(_onClose != null)
        {
            closeButton.onClick.AddListener(_onClose);
        }
    }

    void HideErrorWindow()
    {
        Destroy(gameObject);
    }
}
