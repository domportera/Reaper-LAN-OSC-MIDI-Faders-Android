using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ProfileLoader : MonoBehaviour
{
    [FormerlySerializedAs("highlightImage")] [SerializeField]
    private GameObject _highlightImage;
    [FormerlySerializedAs("titleText")] [SerializeField]
    private Text _titleText;
    [FormerlySerializedAs("button")] [SerializeField]
    private ButtonExtended _button;
    [FormerlySerializedAs("root")] [SerializeField]
    private GameObject _root;

    public bool IsActiveProfile { get; set; }

    public void ToggleHighlight(bool enable)
    {
        _highlightImage.SetActive(enable);
	}

    public void SetText(string profileName)
    {
        _titleText.text = profileName;
	}

    public void SetButtonActions(UnityAction buttonAction, UnityAction holdAction)
    {
        _button.OnClick.AddListener(buttonAction);
        _button.OnPointerHeld.AddListener(holdAction);
	}

	public void Annihilate()
	{
        Destroy(_root);
	}

    public string GetName()
    {
        return _titleText.text;
    }
}
