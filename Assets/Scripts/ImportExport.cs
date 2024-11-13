using System;
using System.IO;
using System.IO.Compression;
using PopUpWindows;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CompressionLevel = System.IO.Compression.CompressionLevel;

public class ImportExport : MonoBehaviour
{
    [FormerlySerializedAs("toggleBackupButton")] [SerializeField]
    Button _toggleBackupButton;

    [FormerlySerializedAs("closeButton")] [SerializeField]
    Button _closeButton;

    [FormerlySerializedAs("exportButton")] [SerializeField]
    Button _exportButton;

    [FormerlySerializedAs("importButton")] [SerializeField]
    Button _importButton;

    [FormerlySerializedAs("panel")] [SerializeField]
    GameObject _panel;

    [FormerlySerializedAs("runtimeDebugger")]
    [Tooltip(
        "We need this to destroy when reloading the scene after an import - the package marks it as DontDestroyOnLoad which messes up our Log button")]
    [SerializeField]
    GameObject _runtimeDebugger;

    const string ExportName = "Reaper Faders Backup";
    const string FileExtension = ".zip";

    readonly string[] _fileExtensions = { "*/*" };
    string _exportPath;

    void Awake()
    {
#if !UNITY_EDITOR
        exportPath =
 Path.Combine(Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android")),"Download", "Reaper Fader Backups");
        NativeFilePicker.RequestPermission();
        Debug.Log("Backup export path set: " + exportPath);
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

    void ConfirmImport()
    {
        RequestPermission();
        PopUpController.Instance.ConfirmationWindow(
            "WARNING: This will wipe all of your current profiles and replace them with the backup archive selected.\nPlease double-check that you're selecting the correct file.\nIf you can't see the file in the file picker, you may need to restart your device. This is an Android bug.",
            FilePickerToImport, null, "Import anyway");
    }

    void FilePickerToImport()
    {
        NativeFilePicker.PickFile(Import, _fileExtensions);
    }

    void Export()
    {
        if (!Directory.Exists(_exportPath)) Directory.CreateDirectory(_exportPath);

        RequestPermission();

        CreateZipFile(GetExportPath());
    }

    void CreateZipFile(string destinationPath, float delay = 0f)
    {
        ZipFile.CreateFromDirectory(Application.persistentDataPath, destinationPath, CompressionLevel.Optimal, false);
        PopUpController.Instance.QuickNoticeWindow("Backup created! Check your Downloads folder.");
        Debug.Log($"Exported backup to {destinationPath}");
    }

    string GetExportPath()
    {
        return Path.Combine(_exportPath,
            ExportName + " " + DateTime.Now.ToString("dd-MM-yy HH-mm-ss") + FileExtension);
    }

    void Import(string path)
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

    void ExtractAndReplaceFiles(string path)
    {
        string tempDirectory = Path.Combine(Application.persistentDataPath, "Temp");

        if (Directory.Exists(tempDirectory)) Directory.Delete(tempDirectory, true);

        ZipFile.ExtractToDirectory(path, tempDirectory);

        //Delete old files
        string[] directories = Directory.GetDirectories(Application.persistentDataPath);

        foreach (string dir in directories)
            if (dir != tempDirectory)
                Directory.Delete(dir, true);

        //Move imported files to the proper place
        directories = Directory.GetDirectories(tempDirectory);

        foreach (string dir in directories)
        {
            var info = new DirectoryInfo(dir);
            string destinationPath = Path.Combine(Application.persistentDataPath, info.Name);
            string sourcePath = info.FullName;
            Directory.Move(sourcePath, destinationPath);
        }

        //Delete the temp directory
        Directory.Delete(tempDirectory, true);
    }

    void ReloadScene()
    {
        Destroy(_runtimeDebugger);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void RequestPermission()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            Permission.RequestUserPermission(Permission.ExternalStorageRead);

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
#endif
    }

    void TogglePanel()
    {
        _panel.SetActive(!_panel.activeSelf);
    }
}