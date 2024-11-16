using UnityEngine;
using UnityEngine.Serialization;

public class Controller2DOptionsPanel : ControllerOptionsPanel
{
    [FormerlySerializedAs("horizontalOptions")] [SerializeField]
    private ControllerOptionsMenu _horizontalOptions;
    [FormerlySerializedAs("verticalOptions")] [SerializeField]
    private ControllerOptionsMenu _verticalOptions;

    public void Initialize(Controller2DData data, RectTransform controlObjectTransform, OscSelectionMenu oscMenu)
    {
        _horizontalOptions.Initialize(data.HorizontalController, this, oscMenu);
        _verticalOptions.Initialize(data.VerticalController, this, oscMenu);

        BaseInitialize(data, controlObjectTransform);
    }

    protected override void Apply()
    {
        _horizontalOptions.SetControllerValuesToFields();
        _verticalOptions.SetControllerValuesToFields();
        base.Apply();
    }
}
