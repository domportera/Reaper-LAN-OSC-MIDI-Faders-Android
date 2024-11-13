using UnityEngine;
using UnityEngine.Serialization;

public class FaderOptions : ControllerOptionsPanel
{
    [FormerlySerializedAs("optionsMenu")] [SerializeField] ControllerOptionsMenu _optionsMenu;

    public void Initialize(FaderData _data, RectTransform _controlObjectTransform, OSCSelectionMenu _oscMenu)
    {
        _optionsMenu.Initialize(_data.GetController(), this, _oscMenu);

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        _optionsMenu.SetControllerValuesToFields();
        base.Apply();
    }

}
