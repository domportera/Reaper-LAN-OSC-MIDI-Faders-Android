using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    public void SetConfirmationText(string _text)
    {
        confirmationText.text = _text;
        confirmationWindow.SetActive(true);
        StartCoroutine(HideConfirmationWindowAfterDelay());
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
