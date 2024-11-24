using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colors
{
	public class ColorSetter : MonoBehaviour
	{
		[SerializeField, FormerlySerializedAs("colorType")]
		private ColorType _colorType;

		private MaskableGraphic _textOrImage;

		private bool _canColor = true;
		private bool HasComponent => _textOrImage != null;

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

			_textOrImage.color = colors.GetColor(_colorType);
		}

		private void GetColoredComponents()
		{
			if (_textOrImage)
				return;
			
			_textOrImage = GetComponent<MaskableGraphic>();
		}
	}
}
