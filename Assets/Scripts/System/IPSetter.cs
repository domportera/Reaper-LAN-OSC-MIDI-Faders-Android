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
    static bool ipSet = false;

    const string IP_ADDRESS_PLAYER_PREF = "IP Address";
    const string PORT_PLAYER_PREF = "Port";

    Utilities util;

    // Start is called before the first frame update
    void Awake()
    {
        util = FindObjectOfType<Utilities>();
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

    public static void SetConnected()
    {
        ipSet = true;
    }

    public static bool IsConnected()
    {
        return ipSet;
    }

    public static void InvalidClient()
    {
        //error message for invalid client
        Debug.LogError("Error creating client - check IP");
    }

    public void SetIP(string _ip)
    {
        //validate ip
        string ipString = _ip.Trim();
        bool valid = IPAddress.TryParse(ipString, out var ip);

        if(valid)
        {
            currentIP = ipString;
            TryConnect();
        }
        else
        {
            util.SetErrorText("Invalid IP Address");
        }
    }

    public void SetPort(string _port)
    {
        //validate ip
        string portString = _port.Trim();
        int port;
        bool valid = int.TryParse(_port, out port);

        if(port > 65535)
        {
            valid = false;
        }

        if (valid)
        {
            currentPort = port;
            TryConnect();
        }
        else
        {
            util.SetErrorText("Invalid Port");
        }
    }

    void SetPort(int _port)
    {
        bool valid = true;

        if (_port > 65535)
        {
            valid = false;
        }

        if (valid)
        {
            currentPort = _port;
            TryConnect();
        }
        else
        {
            util.SetErrorText("Invalid Port");
        }
    }

    public void TryConnect()
    {
        OscPropertySender[] senders = FindObjectsOfType<OscPropertySender>();

        //only connect if we have a port and an IP
        if (currentPort != int.MinValue && currentIP != null)
        {
            foreach (OscPropertySender send in senders)
            {
                send.ChangeConnection(currentIP, currentPort);
            }

            Save();
        }
    }

}
