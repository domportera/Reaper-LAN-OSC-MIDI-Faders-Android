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
        horizontalOptions.Initialize(_data.GetHorizontalController());
        verticalOptions.Initialize(_data.GetVerticalController());

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        horizontalOptions.SetControllerValuesToFields();
        verticalOptions.SetControllerValuesToFields();
        base.Apply();
    }
}
