using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ControlsManager;

public class FaderOptions : ControllerOptions
{
    [SerializeField] ControllerOptionsMenu optionsMenu;
    public void Initialize(FaderData _data, RectTransform _controlObjectTransform)
    {
        optionsMenu.controllerConfig = _data.GetController();
        optionsMenu.Initialize(_data);

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        optionsMenu.SetControllerValuesToFields();
        base.Apply();
    }
}
