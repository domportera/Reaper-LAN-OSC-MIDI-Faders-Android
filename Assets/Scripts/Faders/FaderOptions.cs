using UnityEngine;
using UnityEngine.Serialization;

public class FaderOptions : ControllerOptionsPanel
{
    [FormerlySerializedAs("optionsMenu")] [SerializeField]
    private ControllerOptionsMenu _optionsMenu;

    public void Initialize(FaderData data, RectTransform controlObjectTransform, OscSelectionMenu oscMenu)
    {
        _optionsMenu.Initialize(data.GetSettings(), this, oscMenu);

        BaseInitialize(data, controlObjectTransform);
    }

    protected override void Apply()
    {
        _optionsMenu.SetControllerValuesToFields();
        base.Apply();
    }

}
