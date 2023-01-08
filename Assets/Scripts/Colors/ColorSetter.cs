using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colors
{
	public class ColorSetter : MonoBehaviour
	{
		[FormerlySerializedAs("colorType")] [SerializeField]
		ColorType _colorType;

		Image _image;
		Text _text;

		bool _canColor = true;
		bool HasComponent => _image != null || _text != null;
		bool _isImage = false;

		void Awake()
		{
			GetColoredComponents();
		}

		void Start()
		{
			ColorController.AddToControls(this);
		}

		void OnDestroy()
		{
			ColorController.RemoveFromControls(this);
		}

		internal void SetColors(ColorProfile colors)
		{
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

		void GetColoredComponents()
		{
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
