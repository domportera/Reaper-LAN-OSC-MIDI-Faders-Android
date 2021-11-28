using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UtilityWindows : MonoBehaviour
{
    [SerializeField] RectTransform windowParent = null;
    [SerializeField] float confirmationDisplayTime = 3f;

    [Space(20)]
    [SerializeField] GameObject confirmationWindowPrefab = null;
    [SerializeField] GameObject multiOptionWindowPrefab = null;
    [SerializeField] GameObject sliderWindowPrefab = null;
    [SerializeField] GameObject errorWindowPrefab = null;
    [SerializeField] GameObject quickNoticeWindowPrefab = null;

    public static UtilityWindows instance;

	private void Awake()
	{
        SingletonSetup();
    }

   public void ErrorWindow(string _text, UnityAction _onClose = null)
    {
        GameObject window = InstantiateWindow(errorWindowPrefab);
        ErrorWindow error = window.GetComponent<ErrorWindow>();
        error.Initialize(_text, _onClose);
    }

    public void QuickNoticeWindow(string _text, UnityAction _onComplete = null)
    {
        GameObject window = InstantiateWindow(errorWindowPrefab);
        QuickNoticeWindow confirm = window.GetComponent<QuickNoticeWindow>();
        confirm.Initialize(_text, confirmationDisplayTime, _onComplete);
    }

    public void ConfirmationWindow(string _text, UnityAction _confirm, UnityAction _cancel = null, string _confirmButtonLabel = null, string _cancelButtonLabel = null)
    {
        GameObject window = InstantiateWindow(confirmationWindowPrefab);
        ConfirmationWindow verify = window.GetComponent<ConfirmationWindow>();
        verify.SetActions(_text, _confirm, _cancel, _confirmButtonLabel, _cancelButtonLabel);
    }

    public void TextInputWindow(string _inputLabel, UnityAction<string> _confirm, UnityAction _cancel = null, string _confirmButtonLabel = null, string _cancelButtonLabel = null)
    {
        GameObject window = InstantiateWindow(confirmationWindowPrefab);
        ConfirmationWindow verify = window.GetComponent<ConfirmationWindow>();
        verify.SetActionsInputField(_inputLabel, _confirm, _cancel, _confirmButtonLabel, _cancelButtonLabel);
    }

    public void MultiOptionWindow(string _text, params MultiOptionAction[] _actions)
    {
        GameObject window = InstantiateWindow(multiOptionWindowPrefab);
        MultiOptionWindow multiWindow = window.GetComponent<MultiOptionWindow>();
        multiWindow.SetActions(_text, _actions);
    }

    public void SliderWindow(string _text, float _defaultValue, float _min, float _max, UnityAction<float> _sliderAction, UnityAction _onClose = null)
    {
        GameObject window = InstantiateWindow(sliderWindowPrefab);
        SliderWindow sliderWindow = window.GetComponent<SliderWindow>();
        sliderWindow.SetActions(_text, _defaultValue, _min, _max, _sliderAction, _onClose);
    }


    GameObject InstantiateWindow(GameObject _prefab)
    {
        GameObject window = Instantiate(_prefab, windowParent);
        return window;
    }

    void SingletonSetup()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning($"Can't reference more than one Utility Windows manager as a singleton (\"instance\"). Ensure you are using direct references.", this);
        }
    }
}
