using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Colors
{
    public class ColorPresetSelectorSorter : MonoBehaviour
    {
        [FormerlySerializedAs("objectsOnTop")] [SerializeField] GameObject[] _objectsOnTop;

        public void SortChildren(List<ColorPresetSelector> _children)
        {
            _children = _children.OrderBy(child => child.Preset.Name, System.StringComparer.CurrentCultureIgnoreCase).ToList();
            for(int i = 0; i < _children.Count; i++)
            {
                _children[i].transform.SetSiblingIndex(i + _objectsOnTop.Length);
            }
        }
    }
}