using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.Serialization;


[Serializable]
public abstract class ControllerData
{
    [FormerlySerializedAs("name")] [SerializeField] protected string Name;
    [FormerlySerializedAs("controllers")] [SerializeField] protected List<ControllerSettings> Controllers = new List<ControllerSettings>();
    [FormerlySerializedAs("position")] [SerializeField] protected int Position = NullPosition;
    [FormerlySerializedAs("enabled")] [SerializeField] protected bool Enabled = true;
    [FormerlySerializedAs("width")] [SerializeField] protected float Width = NullWidth;

    protected const int NullPosition = -1;
    protected const int NullWidth = -1;

    public struct WidthRange
    {
        public float min, max, defaultValue;
        
        public WidthRange(float min, float max, float defaultValue)
        {
            this.min = min;
            this.max = max;
            this.defaultValue = defaultValue;
        }
    }

    public static readonly Dictionary<Type, WidthRange> WidthRanges = new()
    {
        {typeof(Controller2DData), new (0.4f, 2f, 1f) },
        {typeof(FaderData), new (0.125f, 1f, 0.25f) }
    };

    public void SetPosition(int index)
    {
        Position = index;
    }

    public int GetPosition()
    {
        return Position;
    }

    public ControllerSettings GetController()
    {
        return Controllers.Count > 0 ? Controllers[0] : null;
    }

    public List<ControllerSettings> GetControllers()
    {
        return Controllers;
    }

    public string GetName()
    {
        return Name;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    public bool GetEnabled()
    {
        return Enabled;
    }

    public void SetWidth(float width)
    {
        Width = width;
    }

    public float GetWidth()
    {
        return Width <= NullWidth ? WidthRanges[GetType()].defaultValue : Width;
    }

    public WidthRange GetWidthRange()
    {
        return WidthRanges[GetType()];
    }

    public void SetEnabled(bool enabled)
    {
        Enabled = enabled;
    }
}

[Serializable]
public class FaderData : ControllerData
{
    public FaderData(string name, ControllerSettings config)
    {
        Controllers.Add(config);
        this.Name = name;
    }

    public FaderData(FaderData data)
    {
        Name = data.Name;
        Controllers = data.Controllers;
        Position = data.GetPosition();
    }
}

[Serializable]
public class Controller2DData : ControllerData
{
    public Controller2DData(string name, ControllerSettings horizontalConfig, ControllerSettings verticalConfig)
    {
        Controllers.Add(horizontalConfig);
        Controllers.Add(verticalConfig);
        this.Name = name;
    }
    public Controller2DData(Controller2DData data)
    {
        Name = data.Name;
        Controllers = data.Controllers;
        Position = data.GetPosition();
    }

    public ControllerSettings GetHorizontalController()
    {
        return Controllers[0];
    }
    public ControllerSettings GetVerticalController()
    {
        return Controllers[1];
    }
}


