using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


public static class FileHandler
{
    public static bool SaveJsonObject<T>(
        T _save,
        FileInfo _fileInfo,
        bool _prettyPrint = true,
        bool _log = false)
    {
        return SaveTextFile(JsonUtility.ToJson(_save, _prettyPrint), _fileInfo, _log);
    }

    public static bool SaveJsonObject<T>(
        T _save,
        string _directory,
        string _fileName,
        string _fileExtension = ".json",
        bool _prettyPrint = true,
        bool _log = false)
    {
        return SaveTextFile(JsonUtility.ToJson(_save, _prettyPrint), _directory, _fileName, _fileExtension, _log);
    }

    public static bool SaveTextFile(string _text, FileInfo _file, bool _log = false)
    {
        string directoryName = _file.DirectoryName;
        string withoutExtension = Path.GetFileNameWithoutExtension(_file.Name);
        string extension = _file.Extension;
        return SaveTextFile(_text, directoryName, withoutExtension, extension, _log);
    }

    public static bool SaveTextFile(
        string _text,
        string _directory,
        string _fileName,
        string _fileExtension,
        bool _log = false)
    {
        if (!Directory.Exists(_directory))
            Directory.CreateDirectory(_directory);
        _fileExtension = PrepareFileExtension(_fileExtension, _log);
        string path2 = _fileName.Trim() + _fileExtension;
        string path = Path.Combine(_directory, path2);
        try
        {
            File.WriteAllText(path, _text);
            if (_log)
                Debug.Log("Saved " + path2 + " to " + path + ")");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Error saving file {0} to {1}:\n{2}", _fileName, path, ex));
            return false;
        }
    }

    private static string PrepareFileExtension(string _fileExtension, bool _log)
    {
        if (!_fileExtension.StartsWith("."))
        {
            if (_log)
                Debug.LogWarning("Adding '.' to start of file extension " + _fileExtension);
            _fileExtension = "." + _fileExtension;
        }

        if (_fileExtension.Contains<char>(' '))
        {
            Debug.LogError("File extension should not contain whitespace. Removing.");
            _fileExtension.Replace(" ", "");
        }

        return _fileExtension;
    }

    public static List<T> LoadAllJsonObjects<T>(string _directory, string _fileExtension)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(_directory);
        List<T> objList = new List<T>();
        if (!directoryInfo.Exists)
            return objList;
        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            if (file.Extension == _fileExtension)
                objList.Add(LoadJsonObject<T>(_directory, file.Name));
        }

        return objList;
    }

    public static T LoadJsonObject<T>(string _directory, string _fileName, string _fileExtension)
    {
        return ObjectFromJson<T>(LoadTextFile(_directory, _fileName, _fileExtension));
    }

    public static T LoadJsonObject<T>(FileInfo _file)
    {
        if (_file != null)
            return ObjectFromJson<T>(LoadTextFile(_file.DirectoryName, _file.Name));
        Debug.LogWarning("Attempting to create a json object from null file info");
        return default(T);
    }

    private static T LoadJsonObject<T>(string _directory, string _fullFileName)
    {
        return ObjectFromJson<T>(LoadTextFile(_directory, _fullFileName));
    }

    public static T ObjectFromJson<T>(string _json)
    {
        if (!string.IsNullOrWhiteSpace(_json))
            return JsonUtility.FromJson<T>(_json);
        Debug.LogWarning("Attempting to create a json object from empty json text");
        return default(T);
    }

    public static string LoadTextFile(string _directory, string _fileName, string _fileExtension)
    {
        string _fullFileName = _fileName + _fileExtension;
        return LoadTextFile(_directory, _fullFileName);
    }

    public static string LoadTextFile(FileInfo _info)
    {
        if (_info != null)
            return LoadTextFile(_info.DirectoryName, _info.Name);
        Debug.LogWarning("Attempting to load text file on null FileInfo");
        return string.Empty;
    }

    private static string LoadTextFile(string _directory, string _fullFileName)
    {
        if (!Directory.Exists(_directory))
        {
            Debug.LogWarning("Directory not found at " + _directory);
            return string.Empty;
        }

        string path = Path.Combine(_directory, _fullFileName);
        if (File.Exists(path))
            return File.ReadAllText(path);
        Debug.LogWarning("File not found at " + path);
        return string.Empty;
    }

    public static FileInfo[] GetFilesInDirectory(string _directory, string _extension = "", bool _log = false)
    {
        if (Directory.Exists(_directory))
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(_directory);
            if (string.IsNullOrWhiteSpace(_extension))
                return directoryInfo.GetFiles();
            string str = PrepareFileExtension(_extension, _log);
            return directoryInfo.GetFiles("*" + str);
        }

        if (_log)
            Debug.LogWarning("Directory " + _directory + " was not found");
        return null;
    }

    public static bool DeleteFile(string _filePath)
    {
        try
        {
            File.Delete(_filePath);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("File deletion error: {0}", ex));
            return false;
        }
    }

    public static bool DeleteFile(
        string _directory,
        string _fileNameSansExtension,
        string _fileExtension,
        bool _log = false)
    {
        string str = PrepareFileExtension(_fileExtension, _log);
        return DeleteFile(Path.Combine(_directory, _fileNameSansExtension + str));
    }

    public static bool DeleteFile(FileInfo _file) => DeleteFile(_file.FullName);

    public static bool ContainsInvalidFileNameCharacters(string _name)
    {
        foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
        {
            if (_name.Contains(invalidFileNameChar.ToString()))
                return true;
        }

        return _name.Contains<char>('/') || _name.Contains<char>('\\');
    }

    public static bool ContainsInvalidFileNameCharacters(
        string _name,
        out List<char> _invalidCharacters)
    {
        _invalidCharacters = GetInvalidFileNameCharacters(_name);
        return _invalidCharacters.Count > 0;
    }

    public static List<char> GetInvalidFileNameCharacters(
        string _name,
        char[] _additionalInvalidChars = null)
    {
        char[] invalidFileNameChars = Path.GetInvalidFileNameChars();
        List<char> fileNameCharacters = new List<char>();
        foreach (char ch in invalidFileNameChars)
        {
            if (_name.Contains<char>(ch))
                fileNameCharacters.Add(ch);
        }

        if (_additionalInvalidChars != null)
        {
            foreach (char additionalInvalidChar in _additionalInvalidChars)
            {
                if (_name.Contains<char>(additionalInvalidChar))
                    fileNameCharacters.Add(additionalInvalidChar);
            }
        }

        return fileNameCharacters;
    }
}