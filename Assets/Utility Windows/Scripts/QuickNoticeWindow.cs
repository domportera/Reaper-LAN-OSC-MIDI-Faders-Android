using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class QuickNoticeWindow : MonoBehaviour
{
    [SerializeField] Text confirmationText = null;
    [SerializeField] Button closeButton = null;

    UnityAction onHide = null;

    // Start is called before the first frame update
    void Awake()
    {
        closeButton.onClick.AddListener(HideConfirmationWindow);
    }

    public void Initialize(string _text, float _hideDelay, UnityAction _onHide)
    {
        onHide = _onHide;
        confirmationText.text = _text;
        StartCoroutine(HideConfirmationWindowAfterDelay(_hideDelay));
    }

    IEnumerator HideConfirmationWindowAfterDelay(float _hideDelay)
    {
        yield return new WaitForSeconds(_hideDelay);
        HideConfirmationWindow();
    }

    void HideConfirmationWindow()
    {
        onHide?.Invoke();
        Destroy(gameObject);
    }
}
