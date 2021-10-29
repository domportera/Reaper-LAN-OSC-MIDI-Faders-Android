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
    [SerializeField] protected float width = NULL_WIDTH;

    protected const int NULL_POSITION = -1;
    protected const int NULL_WIDTH = -1;

    public static readonly Dictionary<Type, Range<float>> widthRanges = new Dictionary<Type, Range<float>>()
    {
        {typeof(Controller2DData), new Range<float>(0.4f, 2f, 1f) },
        {typeof(FaderData), new Range<float>(0.125f, 1f, 0.25f) }
    };

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

    public void SetWidth(float _width)
    {
        width = _width;
    }

    public float GetWidth()
    {
        if(width == NULL_WIDTH)
        {
            return widthRanges[GetType()].defaultValue;
        }

        return width;
    }

    public Range<float> GetWidthRange()
    {
        return widthRanges[GetType()];
    }

    public void SetEnabled(bool _enabled)
    {
        enabled = _enabled;
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


