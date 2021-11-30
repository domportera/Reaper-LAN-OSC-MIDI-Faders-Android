using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

public static class ExtensionMethods
{
    #region Finding Things
    /// <summary>
    /// Can get component on a disabled game object. This will enable then re-disable a disabled game object - so beware any OnEnabled or OnDisabled functions will be called on the target.
    /// Type provided must be a monobehaviour of course
    /// </summary>
    /// <returns>Component of type requested, or default value (null)</returns>
    public static T GetComponentSafer<T>(this GameObject _obj)
    {
        bool active = _obj.activeSelf;
        if (!active)
        {
            _obj.SetActive(true);
        }

        T component = _obj.GetComponent<T>();

        if (!active)
        {
            _obj.SetActive(false);
        }

        return component;
    }
    #endregion Finding Things

    #region Vector Operations


    #region Divide Self
    public static void DivideSelf(ref this Vector2 _numerator, Vector2 _denominator)
    {
        _numerator = _numerator / _denominator;
    }

    public static void DivideSelf(ref this Vector3 _numerator, Vector3 _denominator)
    {
        _numerator = _numerator.Divide(_denominator);
    }

    public static void DivideSelf(ref this Vector4 _numerator, Vector4 _denominator)
    {
        _numerator = _numerator.Divide(_denominator);
    }
    #endregion Divide Self

    #region Division

    public static Vector3 Divide(this Vector3 _numerator, Vector3 _denominator)
    {
        return new Vector3(_numerator.x / _denominator.x, _numerator.y / _denominator.y, _numerator.z / _denominator.z);
    }

    public static Vector4 Divide(this Vector4 _numerator, Vector4 _denominator)
    {
        return new Vector4(_numerator.x / _denominator.x, _numerator.y / _denominator.y, _numerator.z / _denominator.z, _numerator.w / _denominator.w);
    }
    #endregion Division

    #region Vector to Color Conversion
    public static Color ToColor(this Vector4 _vector)
    {
        return (Color)_vector;
    }
    public static Color ToColor(this Vector3 _vector, float _alpha = 1f)
    {
        return new Color(_vector.x, _vector.y, _vector.z, _alpha);
    }
    #endregion Vector to Color Conversion

    #endregion Vector Operataions

    #region Mapping
    public static Vector2 Map(this Vector2 x, Vector2 in_min, Vector2 in_max, Vector2 out_min, Vector2 out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    public static Vector3 Map(this Vector3 x, Vector3 in_min, Vector3 in_max, Vector3 out_min, Vector3 out_max)
    {
        Vector3 a = x - in_min;
        Vector3 b = out_max - out_min;
        Vector3 numerator = Vector3.Scale(a, b);
        Vector3 denominator = (in_max - in_min);
        Vector3 offset = out_min;
        Vector3 result = numerator.Divide(denominator) + offset;
        return result;
    }

    public static Vector4 Map(this Vector4 x, Vector4 in_min, Vector4 in_max, Vector4 out_min, Vector4 out_max)
    {
        Vector4 a = x - in_min;
        Vector4 b = out_max - out_min;
        Vector4 numerator = Vector4.Scale(a, b);
        Vector4 denominator = (in_max - in_min);
        Vector4 offset = out_min;
        Vector4 result = numerator.Divide(denominator) + offset;
        return result;
    }

    public static double Map(this double x, double in_min, double in_max, double out_min, double out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    public static float Map(this float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    public static int Map(this int x, int in_min, int in_max, int out_min, int out_max)
    {
        return (int)((x - in_min) * (out_max - out_min) / (float)(in_max - in_min) + out_min);
    }

    public static void MapSelf(ref this double x, double in_min, double in_max, double out_min, double out_max)
    {
        x = x.Map(in_min, in_max, out_min, out_max);
    }

    public static void MapSelf(ref this float x, float in_min, float in_max, float out_min, float out_max)
    {
        x = x.Map(in_min, in_max, out_min, out_max);
    }

    public static void MapSelf(ref this int x, int in_min, int in_max, int out_min, int out_max)
    {
        x = x.Map(in_min, in_max, out_min, out_max);
    }
    #endregion Mapping

    #region Averages

    public static float Average(this float x, float floatToAverageWith)
    {
        return (x + floatToAverageWith) / 2f;
    }
    public static Vector2 Average(this Vector2 x, Vector2 vectorToAverageWith)
    {
        return (x + vectorToAverageWith) / 2f;
    }
    public static Vector3 Average(this Vector3 x, Vector3 vectorToAverageWith)
    {
        return (x + vectorToAverageWith) / 2f;
    }
    public static Vector4 Average(this Vector4 x, Vector4 vectorToAverageWith)
    {
        return (x + vectorToAverageWith) / 2f;
    }

    public static float Average(this float[] floatsToAverage)
    {
        float sum = 0f;

        foreach(float f in floatsToAverage)
        {
            sum += f;
        }

        return sum / floatsToAverage.Length;
    }

    public static float Average(this int[] intsToAverage)
    {
        float sum = 0f;

        foreach (int f in intsToAverage)
        {
            sum += f;
        }

        return sum / intsToAverage.Length;
    }

    public static Vector2 Average(this Vector2[] vecsToAverage)
    {
        Vector2 sum = Vector2.zero;

        foreach (Vector2 f in vecsToAverage)
        {
            sum += f;
        }

        return sum / vecsToAverage.Length;
    }

    public static Vector3 Average(this Vector3[] vecsToAverage)
    {
        Vector3 sum = Vector3.zero;

        foreach (Vector3 f in vecsToAverage)
        {
            sum += f;
        }

        return sum / vecsToAverage.Length;
    }

    public static Vector4 Average(this Vector4[] vecsToAverage)
    {
        Vector4 sum = Vector4.zero;

        foreach (Vector4 f in vecsToAverage)
        {
            sum += f;
        }

        return sum / vecsToAverage.Length;
    }

    #endregion Averages

    #region Enums
    public static string GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr =
                       Attribute.GetCustomAttribute(field,
                         typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }

    public static int GetInt(this Enum value)
    {
        return (int)(object)value;
    }

    #endregion Enums
}

public static class FileHandler
{
    #region Saving
    /// <summary>
    /// Saves json file of class object to specified directory and file. Will overwrite an existing file. Uses Unity's JsonUtility.
    /// </summary>
    /// <returns>Returns true if successful</returns>
    public static bool SaveJsonObject<T>(T _save, string _directory, string _fileName, string _fileExtension = ".json", bool _prettyPrint = true)
    {
        string json = JsonUtility.ToJson(_save, _prettyPrint);
        return SaveTextFile(json, _directory, _fileName, _fileExtension);
    }

    /// <summary>
    /// Saves text file to specified directory and file. Will overwrite an existing file
    /// </summary>
    /// <param name="_log">Log to console on successful write</param>
    /// <returns>Returns true if successful</returns>
    public static bool SaveTextFile(string _text, string _directory, string _fileName, string _fileExtension, bool _log = false)
    {
        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }

        if(!_fileExtension.StartsWith('.'))
        {
            if(_log)
            {
                Debug.LogWarning($"Adding '.' to start of file extension {_fileExtension}");
            }

            _fileExtension = '.' + _fileExtension;
        }

        if(_fileExtension.Contains(' '))
        {
            Debug.LogError($"File extension should not contain whitespace. Removing before saving.");
            _fileExtension.Replace(" ", "");
        }

        string fullFileName = _fileName.Trim() + _fileExtension;
        string path = Path.Combine(_directory, fullFileName);

        try
        {
            File.WriteAllText(path, _text);
            if (_log)
            {
                Debug.Log($"Saved {fullFileName} to {path})");
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving file {_fileName} to {path}:\n{e}");
            return false;
        }
    }
    #endregion Saving

    #region Loading

    public static List<T> LoadAllJsonObjects<T> (string _directory, string _fileExtension)
    {
        DirectoryInfo info = new DirectoryInfo(_directory);
        List<T> objects = new List<T>();

        if(!info.Exists)
        {
            return objects;
        }

        foreach(FileInfo f in info.GetFiles())
        {
            if(f.Extension == _fileExtension)
            {
                objects.Add(LoadJsonObject<T>(_directory, f.Name));
            }
        }

        return objects;
    }

    public static T LoadJsonObject<T>(string _directory, string _fileName, string _fileExtension)
    {
        string json = LoadTextFile(_directory, _fileName, _fileExtension);
        return ObjectFromJson<T>(json);
    }

    static T LoadJsonObject<T>(string _directory, string _fullFileName)
    {
        string json = LoadTextFile(_directory, _fullFileName);
        return ObjectFromJson<T>(json);
    }

    static T ObjectFromJson<T>(string _json)
    {
        if(string.IsNullOrWhiteSpace(_json))
        {
            Debug.LogWarning($"Attempting to create a json object from empty json text");
            return default(T);
        }

        return JsonUtility.FromJson<T>(_json);
    }

    public static string LoadTextFile(string _directory, string _fileName, string _fileExtension)
    {
        string fullFileName = _fileName + _fileExtension;
        return LoadTextFile(_directory, fullFileName);
    }

    static string LoadTextFile(string _directory, string _fullFileName)
    {
        if(!Directory.Exists(_directory))
        {
            Debug.LogWarning($"Directory not found at {_directory}");
            return string.Empty;
        }

        string path = Path.Combine(_directory, _fullFileName);
        if(!File.Exists(path))
        {
            Debug.LogWarning($"File not found at {path}");
            return string.Empty;
        }

        string text = File.ReadAllText(path);
        return text;
    }

    public static FileInfo[] GetFilesInDirectory(string _directory)
    {
        if (Directory.Exists(_directory))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_directory);
            return directoryInfo.GetFiles();
        }
        else
        {
            Debug.LogWarning($"Directory {_directory} was not found");
            return null;
        }
    }

    public static bool DeleteFile(string _directory, string _fileNameSansExtension, string _fileExtension)
    {
        string filePath = Path.Combine(_directory, _fileNameSansExtension + _fileExtension);

        try
        {
            File.Delete(filePath);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"File deletion error: {e}");
            return false;
        }
    }

    #endregion Loading

    #region File Name Validity
    public static bool ContainsInvalidFileNameCharacters(string _name)
    {
        char[] invalidFileChars = Path.GetInvalidFileNameChars();
        foreach(char c in invalidFileChars)
        {
            if(_name.Contains(c.ToString()))
            {
                return true;
            }
        }

        if(_name.Contains('/') || _name.Contains('\\'))
        {
            return true;
        }

        return false;
    }

    public static bool ContainsInvalidFileNameCharacters(string _name, out List<char> _invalidCharacters)
    {
        _invalidCharacters = GetInvalidFileNameCharacters(_name);
        if(_invalidCharacters.Count > 0)
        {
            return true;
        }

        return false;
    }

    public static List<char> GetInvalidFileNameCharacters(string _name, char[] _additionalInvalidChars = null)
    {
        char[] invalidFileChars = Path.GetInvalidFileNameChars();
        List<char> invalidChars = new List<char>();

        foreach(char c in invalidFileChars)
        {
            if(_name.Contains(c))
            {
                invalidChars.Add(c);
            }
        }

        if(_additionalInvalidChars != null)
        {
            foreach(char c in _additionalInvalidChars)
            {
                if(_name.Contains(c))
                {
                    invalidChars.Add(c);
                }
            }
        }

        return invalidChars;
    }
    #endregion File Name Validity
}

[System.Serializable]
public class Range <T>
{
    public T min;
    public T max;
    public readonly T defaultValue;

    public Range(T min, T max, T defaultValue = default(T))
    {
        this.min = min;
        this.max = max;
        this.defaultValue = defaultValue;
    }
}

public abstract class MonoBehaviourExtended : MonoBehaviour
{
    public T GetComponentSafer<T>()
    {
        return gameObject.GetComponentSafer<T>();
    }

    #region Logging
    protected enum DebugMode { Off, Debug, Verbose };
    [SerializeField] DebugMode debugMode = DebugMode.Off;
    protected enum LogType { Log, Warning, Error, Assertion };

    protected void LogDebug(string _string, LogType type = LogType.Log)
    {
        if (debugMode != DebugMode.Off)
        {
            switch (type)
            {
                case LogType.Log:
                    Debug.Log(_string, this);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(_string, this);
                    break;
                case LogType.Error:
                    Debug.LogError(_string, this);
                    break;
                case LogType.Assertion:
                    Debug.LogAssertion(_string, this);
                    break;
                default:
                    break;
            }
        }
    }

    #endregion Logging

    #region Do Things Later

    protected void DoNextFrame(Action _action)
    {
        DoLater(_action, 1);
    }

    /// <summary>
    /// Invoke an action after a specified number of frames
    /// </summary>
    /// <param name="_frameDelay">The number of frames to wait before performing action</param>
    protected void DoLater(Action _action, int _frameDelay)
    {
        StartCoroutine(DoAfterFrameDelay(_action, _frameDelay));
    }

    /// <summary>
    /// Invoke an action after a time delay
    /// </summary>
    /// <param name="_secondsDelay">The amount of time to delay the action in seconds</param>
    protected void DoLater(Action _action, float _secondsDelay)
    {
        StartCoroutine(DoAfterDelay(_action, _secondsDelay));
    }

    IEnumerator DoAfterFrameDelay(Action _action, int _frames)
    {
        for(int i = 0; i < _frames; i++)
        {
            yield return null;
        }

        _action.Invoke();
    }

    IEnumerator DoAfterDelay(Action _action, float _delay)
    {
        for(float timer = 0f; timer < _delay; timer += Time.deltaTime)
        {
            yield return null;
        }

        _action.Invoke();
    }
    #endregion Do Things Later

}

public static class Operations
{
    #region Averages
    public static float Average(params float[] numsToAverage)
    {
        float sum = 0f;
        foreach (float f in numsToAverage)
        {
            sum += f;
        }

        return sum / numsToAverage.Length;
    }
    public static float Average(params int[] numsToAverage)
    {
        float sum = 0f;
        foreach (float f in numsToAverage)
        {
            sum += f;
        }
        return sum / numsToAverage.Length;
    }

    public static Vector2 Average(params Vector2[] vecsToAverage)
    {
        return vecsToAverage.Average();
    }

    public static Vector3 Average(params Vector3[] vecsToAverage)
    {
        return vecsToAverage.Average();
    }
    public static Vector4 Average(params Vector4[] vecsToAverage)
    {
        return vecsToAverage.Average();
    }
    #endregion Averages
}

public class Vector2Ext
{
    public float x;
    public float y;

    #region Constructors
    public Vector2Ext() { }
    public Vector2Ext(float x, float y)
    {
        this.x = x;
        this.y = y;
    }
    public Vector2Ext(Vector2 _vec2)
    {
        x = _vec2.x;
        y = _vec2.y;
    }
    public Vector2Ext(Vector3 _vec3)
    {
        x = _vec3.x;
        y = _vec3.y;
    }
    public Vector2Ext(Vector4 _vec4)
    {
        x = _vec4.x;
        y = _vec4.y;
    }
    public Vector2Ext(Vector2Ext _vec2)
    {
        x = _vec2.x;
        y = _vec2.y;
    }
    public Vector2Ext(Vector3Ext _vec3)
    {
        x = _vec3.x;
        y = _vec3.y;
    }
    public Vector2Ext(Vector4Ext _vec4)
    {
        x = _vec4.x;
        y = _vec4.y;
    }
    #endregion Constructors

    public Vector2 ToVector2()
    {
        return new Vector2(x, y);
    }
    public Vector2Ext yx { get { return new Vector2Ext(x, y); } }
}


public class Vector3Ext : Vector2Ext
{
    public float z;

    #region Constructors
    public Vector3Ext() { }
    public Vector3Ext(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public Vector3Ext(Vector3 _vec3)
    {
        this.x = _vec3.x;
        this.y = _vec3.y;
        this.z = _vec3.z;
    }
    public Vector3Ext(Vector2 _vec2)
    {
        this.x = _vec2.x;
        this.y = _vec2.y;
    }
    public Vector3Ext(Vector4 _vec4)
    {
        this.x = _vec4.x;
        this.y = _vec4.y;
        this.z = _vec4.z;
    }
    public Vector3Ext(Vector3Ext _vec3)
    {
        this.x = _vec3.x;
        this.y = _vec3.y;
        this.z = _vec3.z;
    }
    public Vector3Ext(Vector2Ext _vec2)
    {
        this.x = _vec2.x;
        this.y = _vec2.y;
    }
    public Vector3Ext(Vector4Ext _vec4)
    {
        this.x = _vec4.x;
        this.y = _vec4.y;
        this.z = _vec4.z;
    }
    #endregion Constructors

    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }

    public Vector3Ext xxx { get { return new Vector3Ext(x, x, x); } }
}

public class Vector4Ext : Vector3Ext
{
    public float w;

    #region Constructors
    public Vector4Ext() { }
    public Vector4Ext(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }
    public Vector4Ext(Vector4 _vec4)
    {
        this.x = _vec4.x;
        this.y = _vec4.y;
        this.z = _vec4.z;
        this.w = _vec4.w;
    }
    public Vector4Ext(Vector3 _vec3)
    {
        this.x = _vec3.x;
        this.y = _vec3.y;
        this.z = _vec3.z;
    }
    public Vector4Ext(Vector2 _vec2)
    {
        this.x = _vec2.x;
        this.y = _vec2.y;
    }
    public Vector4Ext(Vector4Ext _vec4)
    {
        this.x = _vec4.x;
        this.y = _vec4.y;
        this.z = _vec4.z;
        this.w = _vec4.w;
    }
    public Vector4Ext(Vector3Ext _vec3)
    {
        this.x = _vec3.x;
        this.y = _vec3.y;
        this.z = _vec3.z;
    }
    public Vector4Ext(Vector2Ext _vec2)
    {
        this.x = _vec2.x;
        this.y = _vec2.y;
    }

    #endregion Constructors

    public Vector4 ToVector4()
    {
        return new Vector4(x, y, z, w);
    }
    public Vector3Ext xxxx { get { return new Vector4Ext(x, x, x, x); } }
}