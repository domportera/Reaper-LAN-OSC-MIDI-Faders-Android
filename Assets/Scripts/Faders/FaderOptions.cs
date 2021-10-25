using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderOptions : ControllerOptions
{
    [SerializeField] ControllerOptionsMenu optionsMenu;

    public static readonly Range<float> widthRange = new Range<float>(0.1f, 0.7386f);

    protected override void Start()
    {
        base.Start();
        UIManager.instance.WidthEvent.AddListener(SetWidth);
        SetWidth(ControlsManager.instance.FaderWidth);
    }

    public void Initialize(FaderData _data, RectTransform _controlObjectTransform)
    {
        optionsMenu.Initialize(_data.GetController());

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        optionsMenu.SetControllerValuesToFields();
        base.Apply();
    }

}
