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

    public void ConfirmationWindow(string _text)
    {
        confirmationText.text = _text;
        confirmationWindow.SetActive(true);
        confirmationWindow.transform.SetSiblingIndex(confirmationWindow.transform.parent.childCount - 1);
        StartCoroutine(HideConfirmationWindowAfterDelay());
    }

    public void VerificationWindow(string _text, UnityAction _confirm, UnityAction _cancel = null, string _confirmButtonLabel = null, string _cancelButtonLabel = null)
    {
        GameObject window = Instantiate(verificationWindowPrefab, confirmationWindow.transform.parent);
        VerifyWindow verify = window.GetComponent<VerifyWindow>();
        verify.SetActions(_text, _confirm, _cancel, _confirmButtonLabel, _cancelButtonLabel);
    }
    public void VerificationWindow(string _inputLabel, UnityAction<string> _confirm, UnityAction _cancel = null, string _confirmButtonLabel = null, string _cancelButtonLabel = null)
    {
        GameObject window = Instantiate(verificationWindowPrefab, confirmationWindow.transform.parent);
        VerifyWindow verify = window.GetComponent<VerifyWindow>();
        verify.SetActionsInputField(_inputLabel, _confirm, _cancel, _confirmButtonLabel, _cancelButtonLabel);
    }

    void HideErrorWindow()
    {
        errorWindow.SetActive(false);
        errorText.text = "";
    }

    IEnumerator HideConfirmationWindowAfterDelay()
    {
        yield return new WaitForSeconds(confirmationDisplayTime);
        if(confirmationWindow.activeSelf)
        {
            HideConfirmationWindow();
		}
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
