using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;


public enum DefaultValueType { Min, Mid, Max };
public enum CurveType { Linear, Exponential, Logarithmic}
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

public enum MidiChannel
{
    [Description("All")]        All,
    [Description("Channel 1")]  One,
    [Description("Channel 2")]  Two,
    [Description("Channel 3")]  Three,
    [Description("Channel 4")]  Four,
    [Description("Channel 5")]  Five,
    [Description("Channel 6")]  Six,
    [Description("Channel 7")]  Seven,
    [Description("Channel 8")]  Eight,
    [Description("Channel 9")]  Nine,
    [Description("Channel 10")] Ten,
    [Description("Channel 11")] Eleven,
    [Description("Channel 12")] Twelve,
    [Description("Channel 13")] Thirteen,
    [Description("Channel 14")] Fourteen,
    [Description("Channel 15")] Fifteen,
    [Description("Channel 16")] Sixteen
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
    
    public static string[] GetControllerBehaviorTypeNameArray()
    {
        var names = new List<string>();
        foreach (var behaviorType in (ReleaseBehaviorType[])Enum.GetValues(typeof(ReleaseBehaviorType)))
        {
            names.Add(behaviorType.GetDescription());
        }

        return names.ToArray();
    }

    public static string[] GetOscAddressTypeNameArray()
    {
        var names = new List<string>();
        foreach (var address in (OscAddressType[])Enum.GetValues(typeof(OscAddressType)))
        {
            names.Add(address.GetDescription());
        }

        return names.ToArray();
    }

    public static string[] GetMidiChannelNameArray()
    {
        var names = new List<string>();
        foreach (var channel in (MidiChannel[])Enum.GetValues(typeof(MidiChannel)))
        {
            names.Add(channel.GetDescription());
        }

        return names.ToArray();
    }

    public static string[] GetValueRangeNameArray()
    {
        var names = new List<string>();
        foreach (var range in (ValueRange[])Enum.GetValues(typeof(ValueRange)))
        {
            names.Add(range.GetDescription());
        }

        return names.ToArray();
    }
}