using UnityEngine;
using OscJack;
using System.Net;
using UnityEngine.UI;
using UnityEngine.Serialization;
using PopUpWindows;

public class IPSetter : MonoBehaviour
{
    [FormerlySerializedAs("ipAddressField")] [SerializeField] InputField _ipAddressField = null;
    [FormerlySerializedAs("portField")] [SerializeField] InputField _portField = null;

    string _currentIP;
    int _currentPort = int.MinValue;

    const string IPAddressPlayerPref = "IP Address";
    const string PortPlayerPref = "Port";

    public static IPSetter Instance;

    // Start is called before the first frame update
    void Awake()
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
    void Update()
    {

    }

    void Load()
    {
        if(PlayerPrefs.HasKey(IPAddressPlayerPref))
        {
            string ip = PlayerPrefs.GetString(IPAddressPlayerPref);
            SetIP(ip);
            _ipAddressField.SetTextWithoutNotify(ip);
        }

        if (PlayerPrefs.HasKey(PortPlayerPref))
        {
            int port = PlayerPrefs.GetInt(PortPlayerPref);
            SetPort(port);
            _portField.SetTextWithoutNotify(port.ToString());
        }
    }

    void SetIP(string _ip)
    {
        //validate ip
        string ipString = _ip.Trim();
        bool valid = IPAddress.TryParse(ipString, out var ip);

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

    void SetPort(string _port)
    {
        //validate ip
        string portString = _port.Trim();
        int port;
        bool valid = int.TryParse(portString, out port);

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

    void SetPort(int _port)
    {
        bool valid = true;
        const int MAX_PORT = 65535;

        if (_port > MAX_PORT)
        {
            valid = false;
        }

        if (valid)
        {
            _currentPort = _port;
            TryConnectAll();
        }
        else
        {
            PopUpController.Instance.ErrorWindow($"Invalid Port - must be a positive integer less than {MAX_PORT}.");
        }
    }

    public void TryConnectAll()
    {
        OscPropertySender[] senders = FindObjectsOfType<OscPropertySender>();

        foreach(OscPropertySender sender in senders)
        {
            TryConnect(sender);
        }
    }

    public void TryConnect(OscPropertySender _sender)
    {
        //only connect if we have a port and an IP
        if (_currentPort != int.MinValue && _currentIP != null)
        {
            _sender.ChangeConnection(_currentIP, _currentPort);
        }
    }

}
