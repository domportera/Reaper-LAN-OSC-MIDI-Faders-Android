using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject optionsPanel;

    // Start is called before the first frame update
    void Start()
    {
        optionsPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleOptionsMenu()
    {
        optionsPanel.SetActive(!optionsPanel.activeInHierarchy);
    }
}
