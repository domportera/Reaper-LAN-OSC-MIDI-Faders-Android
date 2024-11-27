using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ProfileButtonUi : MonoBehaviour
{
    [FormerlySerializedAs("highlightImage")] [SerializeField]
    private GameObject _highlightImage;
    [FormerlySerializedAs("titleText")] [SerializeField]
    private Text _titleText;
    [FormerlySerializedAs("button")] [SerializeField]
    private ButtonExtended _button;
    [FormerlySerializedAs("root")] [SerializeField]
    private GameObject _root;

    public void ToggleHighlight(bool enable)
    {
        _highlightImage.SetActive(enable);
	}

    public void SetText(string profileName)
    {
        _titleText.text = profileName;
	}

    public void SetButtonActions(UnityAction pressAction, UnityAction holdAction)
    {
        _button.OnClick.AddListener(pressAction);
        _button.OnPointerHeld.AddListener(holdAction);
	}

	public void Annihilate()
	{
        Destroy(_root);
	}
}
