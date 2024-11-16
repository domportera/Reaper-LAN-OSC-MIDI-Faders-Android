using System.Linq;
using UnityEngine;

namespace Colors
{
    [ExecuteAlways]
    internal class BuiltInColorPresets : MonoBehaviour
    {
        private static BuiltInColorPresets _instance;
        internal static BuiltInColorPresets Instance
        {
            get
            {
                if(_instance == null)
                    _instance = Resources.FindObjectsOfTypeAll<BuiltInColorPresets>().Single();
                
                return _instance;
            }

            private set
            {
                if(value == _instance) return;
                
                if (value != null && _instance != null)
                {
                    const string message = "Only one instance of BuiltInColorPresets can exist.";
                    Debug.LogError(message, _instance);
                    Debug.LogError(message, value);
                    throw new System.Exception(message);
                }
                
                _instance = value;
            }
        }
        
        [field: SerializeField, Header("Built-in Themes")]
        internal ColorProfile[] ColorProfiles { get; private set; }
        internal ColorProfile Default => ColorProfiles[0];


        private void Awake()
        {
            Instance = this;
        }
    }
}
