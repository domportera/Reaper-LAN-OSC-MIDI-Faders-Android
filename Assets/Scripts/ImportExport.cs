using System.Collections;
using UnityEngine;
using System.IO.Compression;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.Android;

public class ImportExport : MonoBehaviour
{
    [SerializeField] Button toggleBackupButton;
    [SerializeField] Button closeButton;
    [SerializeField] Button exportButton;
    [SerializeField] Button importButton;
    [SerializeField] GameObject panel;

    [Tooltip("We need a reference to this to destroy it when the scene is reloaded so that when it is reloaded there is only one - it is marked as \"Don't Destroy On Load\" in the source code")]
    [SerializeField] GameObject runtimeDebugger;

    readonly string[] fileExtensions = { "*/*" };
    string exportPath;
    readonly string exportName = "Reaper Faders Backup";
    readonly string fileExtension = ".zip";

	private void Awake()
	{
#if !UNITY_EDITOR
        exportPath = Path.Combine(Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android")),"Download", "Reaper Fader Backups");
        NativeFilePicker.RequestPermission();
        Debug.Log("Backup export path set: " + exportPath);
#else
        //I am using a path other than persistent data path on Windows to prevent previous backups from being zipped up into subsequent backups
        exportPath = Path.Combine(System.Environment.ExpandEnvironmentVariables(@"%PROGRAMDATA%"), "Reaper Faders");
        Debug.LogWarning($"Using windows-specific export path. If using a non-windows development environment, you may want to change this.", this);
#endif

        //backup button binding
        toggleBackupButton.onClick.AddListener(TogglePanel);
        closeButton.onClick.AddListener(TogglePanel);
        importButton.onClick.AddListener(ConfirmImport);
        exportButton.onClick.AddListener(Export);
        exportButton.onClick.AddListener(TogglePanel);
    }

    void ConfirmImport()
    {
        RequestPermission();
        Utilities.instance.VerificationWindow($"WARNING: This will wipe all of your current profiles and replace them with the backup archive selected. If this backup's contents are corrupted or modified, you may be out of luck.", FilePickerToImport, null, "Import anyway");
    }

    void FilePickerToImport()
    {
        NativeFilePicker.PickFile(Import, fileExtensions);
    }

    void Export()
    {
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        RequestPermission();

        CreateZipFile(GetExportPath());
	}

    void CreateZipFile(string _path, float _delay = 0f)
    {
        ZipFile.CreateFromDirectory(Application.persistentDataPath, _path, System.IO.Compression.CompressionLevel.Optimal, false);
        Utilities.instance.ConfirmationWindow("Backup created! Check your Downloads folder.");
        Debug.Log($"Exported backup to {_path}");
    }

    string GetExportPath()
    {
        return Path.Combine(exportPath, exportName + " " + System.DateTime.Now.ToString("dd-MM-yy HH-mm-ss") + fileExtension);
    }

    void Import(string _path)
    {
        //import files safely
        try
        {
            ExtractAndReplaceFiles(_path);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"IMPORT ERROR: {e}");
            Utilities.instance.ErrorWindow($"Error importing zip file! Check the log for more detailed technical information on the error.");
            return;
		}

        //confirmation window then reload scene
        Utilities.instance.ConfirmationWindow("Import complete!", ReloadScene);
	}

    void ExtractAndReplaceFiles(string _path)
    {
        string tempDirectory = Path.Combine(Application.persistentDataPath, "Temp");

        if(Directory.Exists(tempDirectory))
        {
            Directory.Delete(tempDirectory, true);
		}

        ZipFile.ExtractToDirectory(_path, tempDirectory);

        //Delete old files
        string[] directories = Directory.GetDirectories(Application.persistentDataPath);

        foreach (string dir in directories)
        {
            if (dir != tempDirectory)
            {
                Directory.Delete(dir, true);
            }
        }

        //Move imported files to the proper place
        directories = Directory.GetDirectories(tempDirectory);

        foreach (string dir in directories)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            string destinationPath = Path.Combine(Application.persistentDataPath, info.Name);
            string sourcePath = info.FullName;
            Directory.Move(sourcePath, destinationPath);
		}

        //Delete the temp directory
        Directory.Delete(tempDirectory, true);
    }

    void ReloadScene()
    {
        Destroy(runtimeDebugger);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

    void RequestPermission()
    {

#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }
#endif
    }

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
	}
}

