using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Utilities : MonoBehaviour
{
    [SerializeField] Text errorText = null;
    [SerializeField] GameObject errorWindow = null;
    [SerializeField] Button closeErrorWindow = null;

    [Space(20)]
    [SerializeField] Text confirmationText = null;
    [SerializeField] GameObject confirmationWindow = null;
    [SerializeField] Button closeConfirmationWindow = null;

    [Space(20)]
    [SerializeField] float confirmationDisplayTime = 3f;

    [Space(20)]
    [SerializeField] GameObject verificationWindowPrefab = null;
    [SerializeField] GameObject multiOptionWindowPrefab = null;

    public static Utilities instance;

	private void Awake()
	{
        SingletonSetup();
        InitializeUI();
    }

	// Start is called before the first frame update
	void Start() 
    {
    }

   public void ErrorWindow(string _text)
    {
        errorText.text = _text;
        errorWindow.SetActive(true);
        errorWindow.transform.SetSiblingIndex(errorWindow.transform.parent.childCount - 1);
    }

    public void ConfirmationWindow(string _text, Action _onComplete = null)
    {
        confirmationText.text = _text;
        confirmationWindow.SetActive(true);
        confirmationWindow.transform.SetSiblingIndex(confirmationWindow.transform.parent.childCount - 1);
        StartCoroutine(HideConfirmationWindowAfterDelay(_onComplete));
    }

    public void VerificationWindow(string _text, UnityAction _confirm, UnityAction _cancel = null, string _confirmButtonLabel = null, string _cancelButtonLabel = null)
    {
        GameObject window = InstantiateWindow(verificationWindowPrefab);
        VerifyWindow verify = window.GetComponent<VerifyWindow>();
        verify.SetActions(_text, _confirm, _cancel, _confirmButtonLabel, _cancelButtonLabel);
    }
    public void VerificationWindow(string _inputLabel, UnityAction<string> _confirm, UnityAction _cancel = null, string _confirmButtonLabel = null, string _cancelButtonLabel = null)
    {
        GameObject window = InstantiateWindow(verificationWindowPrefab);
        VerifyWindow verify = window.GetComponent<VerifyWindow>();
        verify.SetActionsInputField(_inputLabel, _confirm, _cancel, _confirmButtonLabel, _cancelButtonLabel);
    }

    public void MultiOptionWindow(string _text, params MultiOptionAction[] _actions)
    {
        GameObject window = InstantiateWindow(multiOptionWindowPrefab);
        MultiOptionWindow multiWindow = window.GetComponent<MultiOptionWindow>();
        multiWindow.SetActions(_text, _actions);
    }

    GameObject InstantiateWindow(GameObject _prefab)
    {
        GameObject window = Instantiate(_prefab, confirmationWindow.transform.parent);
        return window;
    }

    void HideErrorWindow()
    {
        errorWindow.SetActive(false);
        errorText.text = "";
    }

    IEnumerator HideConfirmationWindowAfterDelay(Action _onHide)
    {
        yield return new WaitForSeconds(confirmationDisplayTime);
        if(confirmationWindow.activeSelf)
        {
            HideConfirmationWindow();
		}

        _onHide?.Invoke();
	}

    void HideConfirmationWindow()
    {
        confirmationWindow.SetActive(false);
        confirmationText.text = "";
    }

    void InitializeUI()
    {
        errorText.text = "";
        errorWindow.SetActive(false);
        closeErrorWindow.onClick.AddListener(HideErrorWindow);

        confirmationText.text = "";
        confirmationWindow.SetActive(false);
        closeConfirmationWindow.onClick.AddListener(HideConfirmationWindow);
    }

    void SingletonSetup()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"Can't have more than one Utilities. Destroying myself.", this);
            Destroy(gameObject);
        }
    }
}
