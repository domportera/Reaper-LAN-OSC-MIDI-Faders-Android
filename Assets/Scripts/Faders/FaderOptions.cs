using UnityEngine;

public class FaderOptions : ControllerOptionsPanel
{
   [SerializeField]
    private ControllerOptionsMenu _optionsMenu;

    public void Initialize(FaderData data, OscSelectionMenu oscMenu)
    {
        BaseInitialize(data);
        _optionsMenu.Initialize(data.GetSettings(), this, oscMenu);

        OnWake += () => _optionsMenu.ResetValues();
    }

    protected override void Apply()
    {
        _optionsMenu.SetControllerValuesToFields();
        base.Apply();
    }

}
