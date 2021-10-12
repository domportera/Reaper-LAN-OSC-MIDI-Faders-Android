using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ControlsManager;

public class Controller2DOptions : MonoBehaviour
{
    [SerializeField] FaderOptions horizontalOptions;
    [SerializeField] FaderOptions verticalOptions;

    public void Initialize(Controller2DData _data)
    {
        horizontalOptions.controllerConfig = _data.GetHorizontalController();
        verticalOptions.controllerConfig = _data.GetVerticalController();
        horizontalOptions.Initialize(_data);
        verticalOptions.Initialize(_data);
    }
}
