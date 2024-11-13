using UnityEngine;
using OscJack;
using System.Net;
using UnityEngine.UI;
using UnityEngine.Serialization;
using PopUpWindows;

public class IPSetter : MonoBehaviour
{
    [FormerlySerializedAs("ipAddressField")] [SerializeField]
    private InputField _ipAddressField = null;
    [FormerlySerializedAs("portField")] [SerializeField]
    private InputField _portField = null;

    private string _currentIP;
    private int _currentPort = int.MinValue;

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

    // Update is called once per frame
    private void Update()
    {

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
            _currentIP = ipString;
            PlayerPrefs.SetString(IPAddressPlayerPref, _currentIP.ToString());
            TryConnectAll();
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
            PlayerPrefs.SetInt(PortPlayerPref, _currentPort);
        }
        else
        {
            Debug.LogError($"Couldn't parse integer from port field", this);
        }
    }

    private void SetPort(int port)
    {
        var valid = true;
        const int maxPort = 65535;

        if (port > maxPort)
        {
            valid = false;
        }

        if (valid)
        {
            _currentPort = port;
            TryConnectAll();
        }
        else
        {
            PopUpController.Instance.ErrorWindow($"Invalid Port - must be a positive integer less than {maxPort}.");
        }
    }

    public void TryConnectAll()
    {
        var senders = FindObjectsOfType<OscPropertySender>();

        foreach(var sender in senders)
        {
            TryConnect(sender);
        }
    }

    public void TryConnect(OscPropertySender sender)
    {
        //only connect if we have a port and an IP
        if (_currentPort != int.MinValue && _currentIP != null)
        {
            sender.ChangeConnection(_currentIP, _currentPort);
        }
    }

}
