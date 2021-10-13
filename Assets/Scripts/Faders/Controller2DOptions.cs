using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ControlsManager;

public class Controller2DOptions : ControllerOptions
{
    [SerializeField] ControllerOptionsMenu horizontalOptions;
    [SerializeField] ControllerOptionsMenu verticalOptions;

    public void Initialize(Controller2DData _data, RectTransform _controlObjectTransform)
    {
        horizontalOptions.controllerConfig = _data.GetHorizontalController();
        verticalOptions.controllerConfig = _data.GetVerticalController();
        horizontalOptions.Initialize(_data);
        verticalOptions.Initialize(_data);

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void SetControllerValuesToFields(ControllerOptionsMenu _menu)
    {
        base.SetControllerValuesToFields(_menu);
        horizontalOptions.SetControllerValuesToFields();
        verticalOptions.SetControllerValuesToFields();
    }

    protected override void Apply()
    {
        base.Apply();
    }
}
