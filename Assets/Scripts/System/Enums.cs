using System;
using System.Collections.Generic;
using System.ComponentModel;


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

public enum OSCAddressType
{
    [Description("CC (MIDI)")] MidiCC,
    [Description("Pitch (MIDI)")] MidiPitch,
    [Description("Aftertouch (MIDI)")] MidiAftertouch,
    [Description("Custom")] Custom
};

public enum OSCAddressMode
{
    MIDI
}

public enum MIDIChannel
{
    [Description("All")]        All = -1,
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
    public static string[] GetControllerBehaviorTypeNameArray()
    {
        List<string> names = new List<string>();
        foreach (ReleaseBehaviorType behaviorType in (ReleaseBehaviorType[])Enum.GetValues(typeof(ReleaseBehaviorType)))
        {
            names.Add(behaviorType.GetDescription());
        }

        return names.ToArray();
    }

    public static string[] GetOSCAddressTypeNameArray()
    {
        List<string> names = new List<string>();
        foreach (OSCAddressType address in (OSCAddressType[])Enum.GetValues(typeof(OSCAddressType)))
        {
            names.Add(address.GetDescription());
        }

        return names.ToArray();
    }

    public static string[] GetMidiChannelNameArray()
    {
        List<string> names = new List<string>();
        foreach (MIDIChannel channel in (MIDIChannel[])Enum.GetValues(typeof(MIDIChannel)))
        {
            names.Add(channel.GetDescription());
        }

        return names.ToArray();
    }

    public static string[] GetValueRangeNameArray()
    {
        List<string> names = new List<string>();
        foreach (ValueRange range in (ValueRange[])Enum.GetValues(typeof(ValueRange)))
        {
            names.Add(range.GetDescription());
        }

        return names.ToArray();
    }
}