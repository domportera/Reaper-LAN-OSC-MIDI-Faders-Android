using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPresetSelector : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Text title;
    [SerializeField] Image background;
    [SerializeField] Image buttonBorder;
    [SerializeField] PresetPaletteImage[] paletteImages = new PresetPaletteImage[0];

    ColorPreset colorPreset;
    public ColorPreset Preset
    {
        get { return colorPreset; }
        private set
        {
            colorPreset = value;
        }
    }

    public void Initialize(ColorPreset _preset, Action _buttonFunction)
    {
        button.onClick.AddListener(() => { _buttonFunction.Invoke(); });
        SetPaletteColors(_preset);

        title.text = _preset.name;
        title.color = _preset.tertiary;
        background.color = _preset.background;
        buttonBorder.color = _preset.primary;

        Preset = _preset;
    }

    void SetPaletteColors(ColorPreset _preset)
    {
        foreach (PresetPaletteImage i in paletteImages)
        {
            switch (i.colorType)
            {
                case ColorProfile.ColorType.Background:
                    i.image.color = _preset.background;
                    break;
                case ColorProfile.ColorType.Primary:
                    i.image.color = _preset.primary;
                    break;
                case ColorProfile.ColorType.Secondary:
                    i.image.color = _preset.secondary;
                    break;
                case ColorProfile.ColorType.Tertiary:
                    i.image.color = _preset.tertiary;
                    break;
                default:
                    Debug.LogError($"ColorType {i.colorType} not implemented in ColorPresetSelector", this);
                    break;
            }
        }
    }

    [Serializable]
    struct PresetPaletteImage
    {
        public Image image;
        public ColorProfile.ColorType colorType;
    }
}
