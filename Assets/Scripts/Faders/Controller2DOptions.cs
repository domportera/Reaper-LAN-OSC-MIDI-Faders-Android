using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Controller2DOptions : ControllerOptionsPanel
{
    [SerializeField] ControllerOptionsMenu horizontalOptions;
    [SerializeField] ControllerOptionsMenu verticalOptions;

    public void Initialize(Controller2DData _data, RectTransform _controlObjectTransform, OSCSelectionMenu _oscMenu)
    {
        horizontalOptions.Initialize(_data.GetHorizontalController(), this, _oscMenu);
        verticalOptions.Initialize(_data.GetVerticalController(), this, _oscMenu);

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        horizontalOptions.SetControllerValuesToFields();
        verticalOptions.SetControllerValuesToFields();
        base.Apply();
    }
}
