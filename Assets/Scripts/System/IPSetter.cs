using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using System.Net;
using UnityEngine.UI;
using System.IO;

public class IPSetter : MonoBehaviour
{
    [SerializeField] InputField ipAddressField = null;
    [SerializeField] InputField portField = null;

    string currentIP;
    int currentPort = int.MinValue;

    const string IP_ADDRESS_PLAYER_PREF = "IP Address";
    const string PORT_PLAYER_PREF = "Port";

    public static IPSetter instance;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError($"There is a second IPSetter in the scene!", this);
            Debug.LogError($"This is the first one", instance);
        }

        ipAddressField.onEndEdit.AddListener(SetIP);
        portField.onEndEdit.AddListener(SetPort);

        Load();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Load()
    {
        if(PlayerPrefs.HasKey(IP_ADDRESS_PLAYER_PREF))
        {
            string ip = PlayerPrefs.GetString(IP_ADDRESS_PLAYER_PREF);
            SetIP(ip);
            ipAddressField.SetTextWithoutNotify(ip);
        }

        if (PlayerPrefs.HasKey(PORT_PLAYER_PREF))
        {
            int port = PlayerPrefs.GetInt(PORT_PLAYER_PREF);
            SetPort(port);
            portField.SetTextWithoutNotify(port.ToString());
        }
    }

    void Save()
    {
        PlayerPrefs.SetString(IP_ADDRESS_PLAYER_PREF, currentIP.ToString());
        PlayerPrefs.SetInt(PORT_PLAYER_PREF, currentPort);
    }

    void SetIP(string _ip)
    {
        //validate ip
        string ipString = _ip.Trim();
        bool valid = IPAddress.TryParse(ipString, out var ip);

        if(valid)
        {
            currentIP = ipString;
            TryConnectAll();
        }
        else
        {
            Utilities.instance.ErrorWindow("Invalid IP Address");
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
            currentPort = _port;
            TryConnectAll();
        }
        else
        {
            Utilities.instance.ErrorWindow($"Invalid Port - must be a positive integer less than {MAX_PORT}.");
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
        if (currentPort != int.MinValue && currentIP != null)
        {
            _sender.ChangeConnection(currentIP, currentPort);
            Save();
        }
    }

}
