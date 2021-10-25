using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

    public static Vector3 Divide(this Vector3 _numerator, Vector3 _denominator)
    {
        return new Vector3(_numerator.x / _denominator.x, _numerator.y / _denominator.y, _numerator.z / _denominator.z);
    }

    public static Vector4 Divide(this Vector4 _numerator, Vector4 _denominator)
    {
        return new Vector4(_numerator.x / _denominator.x, _numerator.y / _denominator.y, _numerator.z / _denominator.z, _numerator.w / _denominator.w);
    }
    #endregion

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
    #endregion Mapping

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
    public static bool SaveJson<T>(T _save, string _directory, string _fileName, string _fileExtension = ".json", bool _prettyPrint = true)
    {
        string json = JsonUtility.ToJson(_save, _prettyPrint);
        return SaveTextFile(json, _directory, _fileName, _fileExtension);
    }

    /// <summary>
    /// Saves text file to specified directory and file. Will overwrite an existing file
    /// </summary>
    /// <param name="_log">Log to console on successful write</param>
    /// <returns>Returns true if successful</returns>
    private static bool SaveTextFile(string _text, string _directory, string _fileName, string _fileExtension, bool _log = false)
    {
        if (!Directory.Exists(_directory))
        {
            Directory.CreateDirectory(_directory);
        }

        string fullFileName = _fileName + _fileExtension;
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
    public static T LoadJson<T>(string _directory, string _fileName, string _fileExtension)
    {
        string json = LoadTextFile(_directory, _fileName, _fileExtension);

        if (string.IsNullOrWhiteSpace(json))
        {
            return default(T);
        }

        return JsonUtility.FromJson<T>(json);
    }

    public static string LoadTextFile(string _directory, string _fileName, string _fileExtension)
    {
        if (!Directory.Exists(_directory))
        {
            Debug.LogWarning($"Directory not found at {_directory}");
            return string.Empty;
        }

        string path = Path.Combine(_directory, _fileName + _fileExtension);
        if (!File.Exists(path))
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
}

public class Range <T>
{
    public T min;
    public T max;

    public Range(T min, T max)
    {
        this.min = min;
        this.max = max;
    }
}

public abstract class MonoBehaviourExtended : MonoBehaviour
{
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
        for(float timer = 0f; timer < _delay; _delay += Time.deltaTime)
        {
            yield return null;
        }

        _action.Invoke();
    }

    #endregion Logging


}
