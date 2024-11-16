using System;
using System.IO;
using System.IO.Compression;
using PopUpWindows;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Application = UnityEngine.Application;
using CompressionLevel = System.IO.Compression.CompressionLevel;

public class ImportExport : MonoBehaviour
{
    [FormerlySerializedAs("toggleBackupButton")] [SerializeField]
    private Button _toggleBackupButton;

    [FormerlySerializedAs("closeButton")] [SerializeField]
    private Button _closeButton;

    [FormerlySerializedAs("exportButton")] [SerializeField]
    private Button _exportButton;

    [FormerlySerializedAs("importButton")] [SerializeField]
    private Button _importButton;

    [FormerlySerializedAs("panel")] [SerializeField]
    private GameObject _panel;

    [FormerlySerializedAs("runtimeDebugger")]
    [Tooltip(
        "We need this to destroy when reloading the scene after an import - the package marks it as DontDestroyOnLoad which messes up our Log button")]
    [SerializeField]
    private GameObject _runtimeDebugger;

    private const string ExportName = "Reaper Faders Backup";
    private const string FileExtension = ".zip";

    private readonly string[] _fileExtensions = { "*/*" };
    private string _exportPath;

    private void Awake()
    {
#if !UNITY_EDITOR
        _exportPath =
 Path.Combine(Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android")),"Download", "Reaper Fader Backups");
        NativeFilePicker.RequestPermission();
        Debug.Log("Backup export path set: " + _exportPath);
#else
        //I am using a path other than persistent data path on Windows to prevent previous backups from being zipped up into subsequent backups
        _exportPath = Path.Combine(Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%"), "Reaper Faders");
        Debug.LogWarning(
            "Using windows-specific export path. If using a non-windows development environment, you may want to change this.",
            this);
#endif

        //backup button binding
        _toggleBackupButton.onClick.AddListener(TogglePanel);
        _closeButton.onClick.AddListener(TogglePanel);
        _importButton.onClick.AddListener(ConfirmImport);
        _importButton.onClick.AddListener(TogglePanel);
        _exportButton.onClick.AddListener(Export);
        _exportButton.onClick.AddListener(TogglePanel);
    }

    private void ConfirmImport()
    {
        RequestPermission();
        PopUpController.Instance.ConfirmationWindow(
            "WARNING: This will wipe all of your current profiles and replace them with the backup archive selected.\nPlease double-check that you're selecting the correct file.\nIf you can't see the file in the file picker, you may need to restart your device. This is an Android bug.",
            FilePickerToImport, null, "Import anyway");
    }

    private void FilePickerToImport()
    {
        NativeFilePicker.PickFile(Import, _fileExtensions);
    }

    private void Export()
    {
        if (!Directory.Exists(_exportPath)) Directory.CreateDirectory(_exportPath);

        RequestPermission();

        CreateZipFile(GetExportPath());
    }

    private void CreateZipFile(string destinationPath, float delay = 0f)
    {
        ZipFile.CreateFromDirectory(Application.persistentDataPath, destinationPath, CompressionLevel.Optimal, false);
        PopUpController.Instance.QuickNoticeWindow("Backup created! Check your Downloads folder.");
        Debug.Log($"Exported backup to {destinationPath}");
    }

    private string GetExportPath()
    {
        return Path.Combine(_exportPath,
            ExportName + " " + DateTime.Now.ToString("dd-MM-yy HH-mm-ss") + FileExtension);
    }

    private void Import(string path)
    {
        //import files safely
        try
        {
            ExtractAndReplaceFiles(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"IMPORT ERROR: {e}");
            PopUpController.Instance.ErrorWindow(
                "Error importing zip file! Check the log for more detailed technical information on the error.");
            return;
        }

        //confirmation window then reload scene
        PopUpController.Instance.QuickNoticeWindow("Import complete!", ReloadScene);
    }

    private void ExtractAndReplaceFiles(string path)
    {
        var tempDirectory = Path.Combine(Application.persistentDataPath, "Temp");

        if (Directory.Exists(tempDirectory)) Directory.Delete(tempDirectory, true);

        ZipFile.ExtractToDirectory(path, tempDirectory);

        //Delete old files
        var directories = Directory.GetDirectories(Application.persistentDataPath);

        foreach (var dir in directories)
            if (dir != tempDirectory)
                Directory.Delete(dir, true);

        //Move imported files to the proper place
        directories = Directory.GetDirectories(tempDirectory);

        foreach (var dir in directories)
        {
            var info = new DirectoryInfo(dir);
            var destinationPath = Path.Combine(Application.persistentDataPath, info.Name);
            var sourcePath = info.FullName;
            Directory.Move(sourcePath, destinationPath);
        }

        //Delete the temp directory
        Directory.Delete(tempDirectory, true);
    }

    private void ReloadScene()
    {
        Destroy(_runtimeDebugger);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void RequestPermission()
    {
#if PLATFORM_ANDROID
        NativeFilePicker.RequestPermission();
#endif
    }

    private void TogglePanel()
    {
        _panel.SetActive(!_panel.activeSelf);
    }
}