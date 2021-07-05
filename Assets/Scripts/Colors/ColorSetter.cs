using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSetter : MonoBehaviour
{
    [SerializeField] Image[] backgrounds;
    [SerializeField] Image[] faderBackgrounds;
    [SerializeField] Image[] faderHandles;
    [SerializeField] Text[] texts;
    [SerializeField] Image[] scrollHandles;
    [SerializeField] Image[] scrollBackgrounds;
    [SerializeField] Image[] buttons;

    public void SetColors(ColorProfile _colors)
    {
        SetBackgroundColor(_colors.background);
        SetFaderBackgroundColor(_colors.faderBackground);
        SetFaderHandleColor(_colors.faderHandle);
        SetTextColor(_colors.text);
        SetScrollHandleColor(_colors.scrollHandle);
        SetScrollBackgroundColor(_colors.scrollBackground);
        SetButtonColor(_colors.button);
    }

    void SetButtonColor(Color _color)
    {
        foreach (Image i in buttons)
        {
            i.color = _color;
        }
    }

    void SetScrollHandleColor(Color _color)
    {
        foreach (Image i in scrollHandles)
        {
            i.color = _color;
        }
    }

    void SetScrollBackgroundColor(Color _color)
    {
        foreach (Image i in scrollBackgrounds)
        {
            i.color = _color;
        }
    }

    void SetFaderHandleColor(Color _color)
    {
        foreach (Image i in faderHandles)
        {
            i.color = _color;
        }
    }
    void SetFaderBackgroundColor(Color _color)
    {
        foreach (Image i in faderBackgrounds)
        {
            i.color = _color;
        }
    }

    void SetBackgroundColor(Color _color)
    {
        foreach (Image i in backgrounds)
        {
            i.color = _color;
        }

    }

    void SetTextColor(Color _color)
    {
        foreach (Text t in texts)
        {
            t.color = _color;
        }
	}
}
