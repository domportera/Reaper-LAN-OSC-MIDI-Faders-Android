using System;
using UnityEngine;
using System.Net;
using UnityEngine.UI;
using UnityEngine.Serialization;
using PopUpWindows;

public class IPSetter : MonoBehaviour
{
    [FormerlySerializedAs("ipAddressField")] [SerializeField]
    private InputField _ipAddressField;
    [FormerlySerializedAs("portField")] [SerializeField]
    private InputField _portField;

    private const string IPAddressPlayerPref = "IP Address";
    private const string PortPlayerPref = "Port";

    public static IPSetter Instance;

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"There is a second IPSetter in the scene!", this);
            Debug.LogError($"This is the first one", Instance);
        }

        _ipAddressField.onEndEdit.AddListener(SetIP);
        _portField.onEndEdit.AddListener(SetPort);

        Load();
    }

    private void Load()
    {
        if(PlayerPrefs.HasKey(IPAddressPlayerPref))
        {
            var ip = PlayerPrefs.GetString(IPAddressPlayerPref);
            SetIP(ip);
            _ipAddressField.SetTextWithoutNotify(ip);
        }

        if (PlayerPrefs.HasKey(PortPlayerPref))
        {
            var port = PlayerPrefs.GetInt(PortPlayerPref);
            SetPort(port);
            _portField.SetTextWithoutNotify(port.ToString());
        }
    }

    private void SetIP(string ipString)
    {
        //validate ip
        ipString = ipString.Trim();
        var valid = IPAddress.TryParse(ipString, out var ip);

        if(valid)
        {
            OSCSystem.SetIp(ip);
            PlayerPrefs.SetString(IPAddressPlayerPref, ipString);
        }
        else
        {
            PopUpController.Instance.ErrorWindow("Invalid IP Address");
        }
    }

    private void SetPort(string portString)
    {
        //validate ip
        portString = portString.Trim();
        int port;
        var valid = int.TryParse(portString, out port);

        if(valid)
        {
            SetPort(port);
            PlayerPrefs.SetInt(PortPlayerPref, port);
        }
        else
        {
            Debug.LogError($"Couldn't parse integer from port field", this);
        }
    }

    private void SetPort(int port)
    {
        var valid = true;
        const int maxPort = ushort.MaxValue;

        if (port > maxPort)
        {
            valid = false;
        }

        if (valid)
        {
            OSCSystem.SetPort(port);
        }
        else
        {
            PopUpController.Instance.ErrorWindow($"Invalid Port - must be a positive integer less than {maxPort}.");
        }
    }
}
