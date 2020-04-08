using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Utilities : MonoBehaviour
{
    [SerializeField] Text errorText = null;
    [SerializeField] GameObject errorWindow = null;
    [SerializeField] Button closeErrorWindow = null;

    float errorTextTime = 4f;


    // Start is called before the first frame update
    void Start() 
    {
        errorText.text = "";
        errorWindow.SetActive(false);
        closeErrorWindow.onClick.AddListener(HideErrorWindow);
    }

   public void SetErrorText(string _text)
    {
        errorText.text = _text;
        errorWindow.SetActive(true);
    }

    void HideErrorWindow()
    {
        errorWindow.SetActive(false);
        errorText.text = "";
    }
}
