using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class ControllerData
{
    [SerializeField] protected string Name;
    [SerializeField] protected List<ControllerSettings> Controllers = new();
    [SerializeField] protected int Position = NullPosition;
    [SerializeField] protected bool Enabled = true;
    [SerializeField] protected float Width = NullWidth;

    protected const int NullPosition = -1;
    protected const int NullWidth = -1;

    public struct WidthRange
    {
        public readonly float Min;
        public readonly float Max;
        public readonly float DefaultValue;

        public WidthRange(float min, float max, float defaultValue)
        {
            Min = min;
            Max = max;
            DefaultValue = defaultValue;
        }
    }

    public static readonly Dictionary<Type, WidthRange> WidthRanges = new()
    {
        {typeof(Controller2DData), new WidthRange(0.4f, 2f, 1f) },
        {typeof(FaderData), new WidthRange(0.4f, 2f, 1f) }
    };

    public void SetPosition(int index) => Position = index;

    public int GetPosition() => Position;

    public ControllerSettings GetSettings() => Controllers.Count > 0 ? Controllers[0] : null;

    public List<ControllerSettings> GetControllers() => Controllers;

    public string GetName() => Name;

    public void SetName(string name) => Name = name;

    public bool GetEnabled() => Enabled;

    public void SetWidth(float width) => Width = width;

    public float GetWidth() => Width <= NullWidth ? WidthRanges[GetType()].DefaultValue : Width;

    public WidthRange GetWidthRange() => WidthRanges[GetType()];

    public void SetEnabled(bool enabled) => Enabled = enabled;
}

[Serializable]
public class FaderData : ControllerData
{
    public FaderData(string name, ControllerSettings config)
    {
        if(config == null)
            throw new ArgumentNullException(nameof(config));
        
        Controllers.Add(config);
        this.Name = name;
    }

    public FaderData(FaderData data)
    {
        if(data == null)
            throw new ArgumentNullException(nameof(data));
        
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
        if(horizontalConfig == null)
            throw new ArgumentNullException(nameof(horizontalConfig));
        
        if(verticalConfig == null)
            throw new ArgumentNullException(nameof(verticalConfig));
        
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

    public ControllerSettings HorizontalController => Controllers[0];

    public ControllerSettings VerticalController => Controllers[1];
}


