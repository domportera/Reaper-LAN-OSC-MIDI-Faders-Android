using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colors
{
	public class ColorSetter : MonoBehaviour
	{
		[FormerlySerializedAs("colorType")] [SerializeField]
		private ColorType _colorType;

		private Image _image;
		private Text _text;

		private bool _canColor = true;
		private bool HasComponent => _image != null || _text != null;
		private bool _isImage = false;

		private void Awake()
		{
			GetColoredComponents();
		}

		private void Start()
		{
			ColorController.AddToControls(this);
		}

		private void OnDestroy()
		{
			ColorController.RemoveFromControls(this);
		}

		internal void SetColors(ColorProfile colors)
		{
			GetColoredComponents();
			if (!_canColor || !HasComponent) return;

			if (!_isImage)
			{
				_text.color = colors.GetColor(_colorType);
			}
			else
			{
				_image.color = colors.GetColor(_colorType);
			}
		}

		private void GetColoredComponents()
		{
			if (_text)
				return;
			
			_text = GetComponent<Text>();
			if (_text == null)
			{
				_image = GetComponent<Image>();
				if (_image == null)
				{
					Debug.LogWarning($"{name} has no component to color", this);
					_canColor = false;
				}
				else
				{
					_canColor = true;
					_isImage = true;
				}
			}
		}
	}
}
