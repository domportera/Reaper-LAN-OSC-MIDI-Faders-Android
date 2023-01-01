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
        [FormerlySerializedAs("paletteImages")] [SerializeField] PresetPaletteImage[] _paletteImages = new PresetPaletteImage[0];
        public bool IsBuiltIn { get; private set; }

        ColorPreset _colorPreset;
        public ColorPreset Preset
        {
            get { return _colorPreset; }
            private set
            {
                _colorPreset = value;
            }
        }

        public void Initialize(ColorPreset _preset, Action _buttonFunction, Action _buttonHeldFunction, bool _isBuiltIn)
        {
            _button.OnClick.AddListener(() => _buttonFunction.Invoke());
            _button.OnPointerHeld.AddListener(() => _buttonHeldFunction.Invoke());
            SetPaletteColors(_preset);
            IsBuiltIn = _isBuiltIn;

            _title.text = _preset.Name;
            _title.color = _preset.GetColor(ColorProfile.ColorType.Tertiary);
            _background.color = _preset.GetColor(ColorProfile.ColorType.Background);
            _buttonBorder.color = _preset.GetColor(ColorProfile.ColorType.Primary);

            Preset = _preset;
        }

        void SetPaletteColors(ColorPreset _preset)
        {
            foreach (PresetPaletteImage i in _paletteImages)
            {
                i.Image.color = _preset.GetColor(i.ColorType);
            }

            _title.color = _preset.GetColor(ColorProfile.ColorType.Tertiary);
        }

        [Serializable]
        struct PresetPaletteImage
        {
            [FormerlySerializedAs("image")] public Image Image;
            [FormerlySerializedAs("colorType")] public ColorProfile.ColorType ColorType;
        }
    }
}
