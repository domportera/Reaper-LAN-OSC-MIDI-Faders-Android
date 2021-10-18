using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;
using static ControlsManager;

public abstract class ControllerOptions : MonoBehaviour
{
    [SerializeField] protected InputField nameField;
    [SerializeField] protected Slider widthSlider;
    [SerializeField] protected Button applyButton = null;
    [SerializeField] protected Button closeButton = null;
    [SerializeField] protected Button deleteButton = null;

    protected ControllerData controlData;
    RectTransform controlObjectTransform;

    void Awake()
    {
        nameField.onValueChanged.AddListener(RemoveProblemCharactersInNameField);
        applyButton.onClick.AddListener(Apply);
        closeButton.onClick.AddListener(Close);
        deleteButton.onClick.AddListener(Delete);
    }

    protected void Start()
    {
        gameObject.SetActive(false);
    }

    protected void BaseInitialize(ControllerData _data, RectTransform _controlObjectTransform)
    {
        controlObjectTransform = _controlObjectTransform;
        controlData = _data;
        nameField.SetTextWithoutNotify(controlData.GetName());
    }

    protected void SetName(string _s)
    {
        controlData.SetName(_s);
    }

    protected void RemoveProblemCharactersInNameField(string _input)
    {
        _input.Replace("\"", "");
        _input.Replace("\\", "");
        nameField.SetTextWithoutNotify(_input);
    }

    protected void SetControllerMasterVariables()
    {
        string controllerName = nameField.text;
        controlData.SetName(controllerName);
        //controlData.SetWidth(widthSlider.value);
    }


    protected bool VerifyUniqueName(string _s)
    {
        bool valid = true;
        ReadOnlyCollection<ControllerData> controllers = ControlsManager.instance.Controllers;

        foreach (ControllerData set in controllers)
        {
            if (set.GetName() == _s)
            {
                valid = false;
                break;
            }
        }

        if (!valid)
        {
            Utilities.instance.ErrorWindow("Name should be unique - no two controllers in the same profile can have the same name.");
            return false;
        }

        return true;
    }

    protected void Delete()
    {
        //delete from ControlsManager and destroy objects
        ControlsManager.instance.DestroyController(controlData);
        UIManager.instance.DestroyControllerObjects(controlData);
    }

    protected virtual void Apply()
    {
        SetControllerMasterVariables();
        ControlsManager.instance.RespawnController(controlData);
        Utilities.instance.ConfirmationWindow("Settings applied!");
    }

    protected void Close()
    {
        gameObject.SetActive(false);
    }

    protected virtual void SetFaderWidth(float _width)
    {
        controlObjectTransform.sizeDelta = new Vector2(_width, controlObjectTransform.sizeDelta.y);
        RefreshFaderLayoutGroup();
    }

    protected void RefreshFaderLayoutGroup()
    {
        UIManager.instance.GetControllerLayoutGroup().enabled = false;
        UIManager.instance.GetControllerLayoutGroup().enabled = true;
    }
}
