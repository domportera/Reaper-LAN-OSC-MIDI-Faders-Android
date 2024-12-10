using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colors
{
	[RequireComponent(typeof(MaskableGraphic))]
	public class ColorSetter : MonoBehaviour
	{
		[SerializeField, FormerlySerializedAs("colorType")]
		private ColorType _colorType;
		
		private MaskableGraphic _graphic;

		private void Start()
		{
			if (!TryGetComponent(out _graphic))
			{
				throw new Exception("ColorSetter must have a MaskableGraphic component");
			}

			ColorController.AddToControls(this);
		}

		private void OnDestroy()
		{
			ColorController.RemoveFromControls(this);
		}

		internal void SetColors(ColorProfile colors)
		{
			_graphic.color = colors.GetColor(_colorType);
		}
	}
}
