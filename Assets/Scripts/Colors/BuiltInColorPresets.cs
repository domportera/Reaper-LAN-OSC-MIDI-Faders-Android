using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Colors
{
    [ExecuteAlways]
    internal class BuiltInColorPresets : MonoBehaviour
    {
        internal static BuiltInColorPresets Instance;
        
        [field: SerializeField, Header("Built-in Themes")]
        internal ColorProfileStruct[] ColorProfiles { get; private set; }
        internal ColorProfileStruct Default => ColorProfiles[0];

        
        void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError($"Only one instance of {GetType()} can exist.");
                Destroy(this);
                return;
            }
            Instance = this;
        }
    }

    internal static class BuiltInPresetsExtensions
    {
        internal static ColorProfileStruct[] Alphabetical(this ColorProfileStruct[] profileArray)
        {
            return profileArray.OrderBy(x => x.Name).ToArray();
        }
    }
}
