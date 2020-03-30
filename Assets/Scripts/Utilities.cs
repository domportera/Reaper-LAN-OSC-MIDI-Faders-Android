using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Utilities : MonoBehaviour
{
    [SerializeField] Text errorText = null;
    float errorTextTime = 5f;

    Coroutine clearErrorTextRoutine = null;

    // Start is called before the first frame update
    void Start() 
    {
        errorText.text = "";
    }

    // Update is called once per frame
    void Update()
    {

    }

   public void SetErrorText(string _text)
    {
        errorText.text = _text;

        if(clearErrorTextRoutine != null)
        {
            StopCoroutine(clearErrorTextRoutine); 
        }

        clearErrorTextRoutine = StartCoroutine(ClearErrorText());
    }

    IEnumerator ClearErrorText()
    {
        yield return new WaitForSeconds(errorTextTime);
        errorText.text = "";
    }
}
