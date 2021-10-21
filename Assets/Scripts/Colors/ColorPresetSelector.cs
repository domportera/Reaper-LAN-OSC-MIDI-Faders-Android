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
