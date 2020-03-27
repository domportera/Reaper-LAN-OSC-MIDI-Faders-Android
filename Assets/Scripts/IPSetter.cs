using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;
using System.Net;
using UnityEngine.UI;
using System.IO;

public class IPSetter : MonoBehaviour
{
    [SerializeField] List<OscPropertySender> senders = null;
    [SerializeField] Text errorText = null;

    [SerializeField] InputField ipAddressField = null;
    [SerializeField] InputField portField = null;

    string currentIP;
    int currentPort = int.MinValue;
    static bool ipSet = false;

    string fileName = "/IPAddress.txt";

    // Start is called before the first frame update
    void Start()
    {
        errorText.text = "";
        Load();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Load()
    {
        string ip = null;
        string port = null;
        try
        {
            StreamReader sr = new StreamReader(Application.persistentDataPath + fileName);
            ip = sr.ReadLine();
            port = sr.ReadLine();
            sr.Close();
        }
        catch
        {
            Debug.LogError("Problem Loading IP!");
        }

        if (ip != null)
        {
            SetIP(ip);
            ipAddressField.SetTextWithoutNotify(ip);
        }

        if (port != null)
        {
            SetPort(port);
            portField.SetTextWithoutNotify(port);
        }

    }

    void Save()
    {
        try
        {
            StreamWriter sw = new StreamWriter(Application.persistentDataPath + fileName);
            sw.WriteLine(currentIP);
            sw.WriteLine(currentPort);
            sw.Close();
        }
        catch
        {
            Debug.LogError("Problem writing IP!");
        }
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
            SetErrorText("Invalid IP Address");
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
            SetErrorText("Invalid Port");
        }
    }

    void TryConnect()
    {
        //only connect if we have a port and an IP
        if(currentPort != int.MinValue && currentIP != null)
        {
            foreach (OscPropertySender send in senders)
            {
                send.ChangeConnection(currentIP, currentPort);
            }

            Save();
        }
    }

    void SetErrorText(string _text)
    {
        errorText.text = _text;
        Invoke("ClearErrorText", 5f);
    }

    void ClearErrorText()
    {
        errorText.text = "";
    }
}
