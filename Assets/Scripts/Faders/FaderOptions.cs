using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaderOptions : ControllerOptionsPanel
{
    [SerializeField] ControllerOptionsMenu optionsMenu;

    public void Initialize(FaderData _data, RectTransform _controlObjectTransform, OSCSelectionMenu _oscMenu)
    {
        optionsMenu.Initialize(_data.GetController(), this, _oscMenu);

        BaseInitialize(_data, _controlObjectTransform);
    }

    protected override void Apply()
    {
        optionsMenu.SetControllerValuesToFields();
        base.Apply();
    }

}
