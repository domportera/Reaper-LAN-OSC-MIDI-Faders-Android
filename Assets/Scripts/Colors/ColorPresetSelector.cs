using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPresetSelector : MonoBehaviour
{
    [SerializeField] ButtonExtended button;
    [SerializeField] Text title;
    [SerializeField] Image background;
    [SerializeField] Image buttonBorder;
    [SerializeField] PresetPaletteImage[] paletteImages = new PresetPaletteImage[0];
    public bool isBuiltIn { get; private set; }

    ColorPreset colorPreset;
    public ColorPreset Preset
    {
        get { return colorPreset; }
        private set
        {
            colorPreset = value;
        }
    }

    public void Initialize(ColorPreset _preset, Action _buttonFunction, Action _buttonHeldFunction, bool _isBuiltIn)
    {
        button.onClick.AddListener(() => _buttonFunction.Invoke());
        button.OnPointerHeld.AddListener(() => _buttonHeldFunction.Invoke());
        SetPaletteColors(_preset);
        isBuiltIn = _isBuiltIn;

        title.text = _preset.Name;
        title.color = _preset.GetColor(ColorProfile.ColorType.Tertiary);
        background.color = _preset.GetColor(ColorProfile.ColorType.Background);
        buttonBorder.color = _preset.GetColor(ColorProfile.ColorType.Primary);

        Preset = _preset;
    }

    void SetPaletteColors(ColorPreset _preset)
    {
        foreach (PresetPaletteImage i in paletteImages)
        {
            i.image.color = _preset.GetColor(i.colorType);
        }

        title.color = _preset.GetColor(ColorProfile.ColorType.Tertiary);
    }

    [Serializable]
    struct PresetPaletteImage
    {
        public Image image;
        public ColorProfile.ColorType colorType;
    }
}
