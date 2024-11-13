using UnityEngine;
using UnityEngine.Serialization;

public class Controller2DOptions : ControllerOptionsPanel
{
    [FormerlySerializedAs("horizontalOptions")] [SerializeField] ControllerOptionsMenu _horizontalOptions;
    [FormerlySerializedAs("verticalOptions")] [SerializeField] ControllerOptionsMenu _verticalOptions;

    public void Initialize(Controller2DData _data, RectTransform _controlObjectTransform, OSCSelectionMenu _oscMenu)
    {
        _horizontalOptions.Initialize(_data.GetHorizontalController(), this, _oscMenu);
        _verticalOptions.Initialize(_data.GetVerticalController(), this, _oscMenu);

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        _horizontalOptions.SetControllerValuesToFields();
        _verticalOptions.SetControllerValuesToFields();
        base.Apply();
    }
}
