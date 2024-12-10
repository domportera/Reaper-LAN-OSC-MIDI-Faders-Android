using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colors
{
    public class ColorPresetSelector : MonoBehaviour
    {
        [FormerlySerializedAs("button")] [SerializeField]
        private ButtonExtended _button;
        [FormerlySerializedAs("title")] [SerializeField]
        private Text _title;
        [FormerlySerializedAs("background")] [SerializeField]
        private Image _background;
        [FormerlySerializedAs("buttonBorder")] [SerializeField]
        private Image _buttonBorder;
        [FormerlySerializedAs("paletteImages")] [SerializeField]
        private PresetPaletteImage[] _paletteImages = Array.Empty<PresetPaletteImage>();
        public bool IsBuiltIn { get; private set; }

        public ColorProfile Preset { get; private set; }

        public void Initialize(ColorProfile preset, Action buttonFunction, Action buttonHeldFunction, bool isBuiltIn)
        {
            _button.OnClick.AddListener(buttonFunction.Invoke);
            _button.OnPointerHeld.AddListener(buttonHeldFunction.Invoke);
            SetPaletteColors(preset);
            IsBuiltIn = isBuiltIn;

            _title.text = preset.Name;
            _title.color = preset.GetColor(ColorType.Tertiary);
            _background.color = preset.GetColor(ColorType.Background);
            _buttonBorder.color = preset.GetColor(ColorType.Primary);

            Preset = preset;
        }

        private void SetPaletteColors(ColorProfile preset)
        {
            foreach (var i in _paletteImages)
            {
                i.Image.color = preset.GetColor(i.ColorType);
            }

            _title.color = preset.GetColor(ColorType.Tertiary);
        }

        [Serializable]
        private struct PresetPaletteImage
        {
            [FormerlySerializedAs("image")] public Image Image;
            [FormerlySerializedAs("colorType")] public ColorType ColorType;
        }
    }
}
