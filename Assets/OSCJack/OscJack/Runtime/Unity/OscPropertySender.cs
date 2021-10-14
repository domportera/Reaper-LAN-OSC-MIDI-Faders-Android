// OSC Jack - Open Sound Control plugin for Unity
// https://github.com/keijiro/OscJack

using UnityEngine;
using System;
using System.Reflection;

namespace OscJack
{
    [AddComponentMenu("OSC/Property Sender")]
    public sealed class OscPropertySender : MonoBehaviour
    {
        #region Editable fields

        [SerializeField] string _ipAddress = "127.0.0.1";
        [SerializeField] int _udpPort = 9000;
        [SerializeField] string _oscAddress = "/unity";
        [SerializeField] bool _keepSending = false;

        #endregion

        #region Internal members

        OscClient _client;
        PropertyInfo _propertyInfo;

        public 

        void UpdateSettings()
        {
            try
            {
                _client = OscMaster.GetSharedClient(_ipAddress, _udpPort);
            }
            catch
            {
                _client = null;
                Debug.LogError($"OSC connection failed with address {_ipAddress}:{_udpPort}");
            }
        }

        #endregion

        #region MonoBehaviour implementation
        
        void Update()
        {
            
        }

        #endregion

        #region Sender methods

        int _intValue = Int32.MaxValue;

        public void Send(int data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _intValue) return;
            _client.Send(_oscAddress, data);
            _intValue = data;
        }

        public void Send(int data1, int data2)
        {
            if (_client is null) return;
            if (!_keepSending && data1 == _intValue) return;
            _client.Send(_oscAddress, data1, data2);
            _intValue = data1;
        }

        float _floatValue = Single.MaxValue;

        public void Send(float data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _floatValue) return;
            _client.Send(_oscAddress, data);
            _floatValue = data;
        }
        public void Send(float data1, float data2)
        {
            if (_client is null) return;
            if (!_keepSending && data1 == _floatValue) return;
            _client.Send(_oscAddress, data1, data2);
            _floatValue = data1;
        }

        Vector2 _vector2Value = new Vector2(Single.MaxValue, 0);

        public void Send(Vector2 data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _vector2Value) return;
            _client.Send(_oscAddress, data.x, data.y);
            _vector2Value = data;
        }

        Vector3 _vector3Value = new Vector3(Single.MaxValue, 0, 0);

        public void Send(Vector3 data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _vector3Value) return;
            _client.Send(_oscAddress, data.x, data.y, data.z);
            _vector3Value = data;
        }

        Vector4 _vector4Value = new Vector4(Single.MaxValue, 0, 0, 0);

        public void Send(Vector4 data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _vector4Value) return;
            _client.Send(_oscAddress, data.x, data.y, data.z, data.w);
            _vector4Value = data;
        }

        Vector2Int _vector2IntValue = new Vector2Int(Int32.MaxValue, 0);

        public void Send(Vector2Int data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _vector2IntValue) return;
            _client.Send(_oscAddress, data.x, data.y);
            _vector2IntValue = data;
        }

        Vector3Int _vector3IntValue = new Vector3Int(Int32.MaxValue, 0, 0);

        public void Send(Vector3Int data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _vector3IntValue) return;
            _client.Send(_oscAddress, data.x, data.y, data.z);
            _vector3IntValue = data;
        }

        string _stringValue = string.Empty;

        public void Send(string data)
        {
            if (_client is null) return;
            if (!_keepSending && data == _stringValue) return;
            _client.Send(_oscAddress, data);
            _stringValue = data;
        }

        #endregion

        #region Added Methods

        public void ChangeConnection(string _ip, int _port)
        {
            _ipAddress = _ip;
            _udpPort = _port;
            UpdateSettings();
        }

        public void SetAddress(string _address)
        {
            _oscAddress = _address;
        }

        #endregion

    }
}
