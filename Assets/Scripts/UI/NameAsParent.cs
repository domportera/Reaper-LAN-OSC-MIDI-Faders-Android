using System;
using UnityEngine;

[ExecuteInEditMode]
public class NameAsParent : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        MatchParentsName();
    }

    private void OnValidate()
    {
        MatchParentsName();
    }

    private void Awake()
    {
        MatchParentsName();
    }

    private void MatchParentsName()
    {
        var parent = transform.parent;
        if(!parent)
            return;
        gameObject.name = parent.name;
    }
}
