using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorPresetSelectorSorter : MonoBehaviour
{
    [SerializeField] GameObject[] objectsOnTop;

    public void SortChildren(List<ColorPresetSelector> _children)
    {
        _children = _children.OrderBy(child => child.Preset.Name, System.StringComparer.CurrentCultureIgnoreCase).ToList();
        for(int i = 0; i < _children.Count; i++)
        {
            _children[i].transform.SetSiblingIndex(i + objectsOnTop.Length);
        }
    }
}