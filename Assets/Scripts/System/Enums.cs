using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;


public enum DefaultValueType { Min, Mid, Max };
public enum CurveType { Linear = 0, Exponential = 1, Logarithmic = 2 };

public static class CurveTypeExtensions
{
    public static float GetExponent(this CurveType curveType) => Exponents[(int)curveType];

    private static readonly float[] Exponents =
    {
        1f, // CurveType.Linear = 0
        2f, // CurveType.Exponential = 1
        0.5f, // CurveType.Logarithmic = 2
    };
}


public enum InputMethod { Touch }

public enum ValueRange
{
    [Description("7-bit (0-127)")]          SevenBit,
    [Description("14-bit (0-16383)")]       FourteenBit,
    [Description("Floating Point (0-1)")]   Float,
    [Description("8-bit (0-255)")]          EightBit,
    [Description("Custom Integer Range")]   CustomInt,
    [Description("Custom Float Range")]     CustomFloat

    //"8-bit (0-255)",
    //"7-bit (-64-63)",
    //"14-bit(-16384-16383)",
    //"8-bit(-128-127)"
    //custom range
};

public enum ReleaseBehaviorType
{
    [Description("Stay")]   Normal,
    [Description("Return")] PitchWheel
};

public enum ControllerType
{
    [Description("Fader")]          Fader,
    [Description("2D Controller")]  Controller2D
};

public enum OscAddressType
{
    [Description("CC (MIDI)")] MidiCc,
    [Description("Pitch (MIDI)")] MidiPitch,
    [Description("Aftertouch (MIDI)")] MidiAftertouch,
    [Description("Custom")] Custom
};

public enum OscAddressMode
{
    Midi, Custom
}

[Flags]
public enum MidiChannel
{
    [Description("Channel 1")] One = 1,
    [Description("Channel 2")] Two = 1 << 1,
    [Description("Channel 3")] Three = 1 << 2,
    [Description("Channel 4")] Four = 1 << 3,
    [Description("Channel 5")] Five = 1 << 4,
    [Description("Channel 6")] Six = 1 << 5,
    [Description("Channel 7")] Seven = 1 << 6,
    [Description("Channel 8")] Eight = 1 << 7,
    [Description("Channel 9")] Nine = 1 << 8,
    [Description("Channel 10")] Ten = 1 << 9,
    [Description("Channel 11")] Eleven = 1 << 10,
    [Description("Channel 12")] Twelve = 1 << 11,
    [Description("Channel 13")] Thirteen = 1 << 12,
    [Description("Channel 14")] Fourteen = 1 << 13,
    [Description("Channel 15")] Fifteen = 1 << 14,
    [Description("Channel 16")] Sixteen = 1 << 15,

    [Description("All")] All = One | Two | Three | Four | Five | Six | Seven | Eight | Nine | Ten | Eleven | Twelve |
                               Thirteen | Fourteen | Fifteen | Sixteen
};

public static class EnumUtility
{
    
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        if (name == null) return null;
        
        var field = type.GetField(name);
        if (field != null && Attribute.GetCustomAttribute(field, typeof (DescriptionAttribute)) is DescriptionAttribute customAttribute)
            return customAttribute.Description;
        return null;
    }

    public static string[] GetTypeNameArray<T>() where T : Enum
    {
        var names = new List<string>();
        foreach (var val in (T[])Enum.GetValues(typeof(T)))
        {
            names.Add(val.GetDescription() ?? val.ToString());
        }

        return names.ToArray();
    }
}