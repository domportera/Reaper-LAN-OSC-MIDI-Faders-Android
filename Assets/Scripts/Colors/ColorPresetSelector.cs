using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Colors
{
    public class ColorPresetSelector : MonoBehaviour
    {
        [FormerlySerializedAs("button")] [SerializeField] ButtonExtended _button;
        [FormerlySerializedAs("title")] [SerializeField] Text _title;
        [FormerlySerializedAs("background")] [SerializeField] Image _background;
        [FormerlySerializedAs("buttonBorder")] [SerializeField] Image _buttonBorder;
        [FormerlySerializedAs("paletteImages")] [SerializeField] PresetPaletteImage[] _paletteImages = Array.Empty<PresetPaletteImage>();
        public bool IsBuiltIn { get; private set; }

        ColorProfile _colorPreset;
        public ColorProfile Preset
        {
            get { return _colorPreset; }
            private set
            {
                _colorPreset = value;
            }
        }

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

        void SetPaletteColors(ColorProfile preset)
        {
            foreach (PresetPaletteImage i in _paletteImages)
            {
                i.Image.color = preset.GetColor(i.ColorType);
            }

            _title.color = preset.GetColor(ColorType.Tertiary);
        }

        [Serializable]
        struct PresetPaletteImage
        {
            [FormerlySerializedAs("image")] public Image Image;
            [FormerlySerializedAs("colorType")] public ColorType ColorType;
        }
    }
}
