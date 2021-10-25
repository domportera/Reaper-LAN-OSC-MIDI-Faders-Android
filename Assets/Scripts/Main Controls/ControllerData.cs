using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public abstract class ControllerData
{
    [SerializeField] protected string name;
    [SerializeField] protected List<ControllerSettings> controllers = new List<ControllerSettings>();
    [SerializeField] protected int position = NULL_POSITION;
    [SerializeField] protected bool enabled = true;

    protected const int NULL_POSITION = -1;

    public void SetPosition(int _index)
    {
        position = _index;
    }

    public int GetPosition()
    {
        return position;
    }

    public ControllerSettings GetController()
    {
        if (controllers.Count > 0)
        {
            return controllers[0];
        }

        return null;
    }

    public List<ControllerSettings> GetControllers()
    {
        return controllers;
    }

    public string GetName()
    {
        return name;
    }

    public void SetName(string _name)
    {
        name = _name;
    }

    public bool GetEnabled()
    {
        return enabled;
    }

    public void SetEnabled(bool _enabled)
    {
        enabled = _enabled;
    }

    public void SetWidth(float value)
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class FaderData : ControllerData
{
    public FaderData(string name, ControllerSettings config)
    {
        controllers.Add(config);
        this.name = name;
    }

    public FaderData(FaderData _data)
    {
        name = _data.name;
        controllers = _data.controllers;
        position = _data.GetPosition();
    }
}

[Serializable]
public class Controller2DData : ControllerData
{
    [SerializeField] protected float width = 1f;

    public Controller2DData(string name, ControllerSettings horizontalConfig, ControllerSettings verticalConfig)
    {
        controllers.Add(horizontalConfig);
        controllers.Add(verticalConfig);
        this.name = name;
    }
    public Controller2DData(Controller2DData _data)
    {
        name = _data.name;
        controllers = _data.controllers;
        position = _data.GetPosition();
    }

    public ControllerSettings GetHorizontalController()
    {
        return controllers[0];
    }
    public ControllerSettings GetVerticalController()
    {
        return controllers[1];
    }
}


