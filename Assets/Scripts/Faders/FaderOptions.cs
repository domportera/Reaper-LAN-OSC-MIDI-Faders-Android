using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderOptions : ControllerOptions
{
    [SerializeField] ControllerOptionsMenu optionsMenu;

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
