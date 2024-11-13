using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Colors
{
    public class ColorPresetSelectorSorter : MonoBehaviour
    {
        [FormerlySerializedAs("objectsOnTop")] [SerializeField]
        private GameObject[] _objectsOnTop;

        public void SortChildren(List<ColorPresetSelector> children)
        {
            children = children.OrderBy(child => child.Preset.Name, System.StringComparer.CurrentCultureIgnoreCase).ToList();
            for(var i = 0; i < children.Count; i++)
            {
                children[i].transform.SetSiblingIndex(i + _objectsOnTop.Length);
            }
        }
    }
}