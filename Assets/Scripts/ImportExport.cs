using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class ImportExport : MonoBehaviour
{
    [SerializeField] Button toggleBackupButton;
    [SerializeField] Button closeButton;
    [SerializeField] Button exportButton;
    [SerializeField] Button importButton;
    [SerializeField] GameObject panel;

    readonly string[] fileExtensions = { "*/*" };
    string exportPath;
    string exportName = "/Reaper Faders Backup.zip";

	private void Awake()
	{
#if !UNITY_EDITOR
        exportPath = Application.persistentDataPath.Substring(0, Application.persistentDataPath.IndexOf("Android")) + "Download/ReaperFaderBackups";
        //NativeFilePicker.RequestPermission();W
        Debug.Log("PATH: " + exportPath);
#else
        exportPath = Application.persistentDataPath + "/Backups";
#endif

        //backup button binding
        toggleBackupButton.onClick.AddListener(TogglePanel);
        closeButton.onClick.AddListener(TogglePanel);
        importButton.onClick.AddListener(Import);
        exportButton.onClick.AddListener(Export);
    }

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Import()
    {
         NativeFilePicker.PickFile(ImportFile, fileExtensions);
    }

    void Export()
    {
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        ZipFile.CreateFromDirectory(Application.persistentDataPath, exportPath + exportName, System.IO.Compression.CompressionLevel.NoCompression, false);
	}

    void ImportFile(string _path)
    {
        //import files
        ZipFile.ExtractToDirectory(_path, Application.persistentDataPath);

        //confirmation window
        Utilities.instance.ConfirmationWindow("Import complete!", ReloadScene);
        //reload scene on complete of confirmation window
	}

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

    public void TogglePanel()
    {
        panel.SetActive(!panel.activeSelf);
	}
}

