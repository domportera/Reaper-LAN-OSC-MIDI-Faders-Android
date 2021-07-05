using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FaderColors : MonoBehaviour
{
    [SerializeField] Image[] backgrounds;
    [SerializeField] Image handle;
    [SerializeField] Text[] texts;
    [SerializeField] Image[] buttonImages;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHandleColor(Color _color)
    {
        handle.color = _color;
	}

    public void SetBackgroundColor(Color _color)
    {
        foreach (Image i in backgrounds)
        {
            i.color = _color;
        }

    }

    public void SetTextColor(Color _color)
    {
        foreach (Text t in texts)
        {
            t.color = _color;
        }
	}
}
