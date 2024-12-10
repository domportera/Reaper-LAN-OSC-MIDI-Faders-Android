using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Colors
{
    [ExecuteAlways]
    [CreateAssetMenu(fileName = "BuiltInColorPresets", menuName = "Colors/BuiltInColorPresets", order = 0)]
    public class BuiltInColorPresets : ScriptableObject, IReadOnlyList<ColorProfile>
    {
        [field: SerializeField, Header("Built-in Themes")]
        public ColorProfile[] ColorProfiles { get; private set; }
        public ColorProfile Default => ColorProfiles[0];
        public IEnumerator<ColorProfile> GetEnumerator() => ((IEnumerable<ColorProfile>)ColorProfiles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ColorProfiles.GetEnumerator();

        public int Count => ColorProfiles.Length;

        public ColorProfile this[int index] => ColorProfiles[index];
    }
}
