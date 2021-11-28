using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MultiOptionWindow : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform buttonParent;
    [SerializeField] Text text;
    [SerializeField] Button closeButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(CloseWindow);
    }

    public void SetActions(string _text, MultiOptionAction[] _actions)
    {
        text.text = _text;

        foreach(MultiOptionAction a in _actions)
        {
            GameObject newButtonObj = Instantiate(buttonPrefab, buttonParent);
            newButtonObj.SetActive(true);
            Button button = newButtonObj.GetComponent<Button>();
            button.GetComponentInChildren<Text>().text = a.name;
            button.onClick.AddListener(() => { a.action.Invoke(); });
            button.onClick.AddListener(CloseWindow);
        }
    }

    void CloseWindow()
    {
        Destroy(gameObject);
    }
}

public struct MultiOptionAction
{
    public string name;
    public Action action;

    public MultiOptionAction(string name, Action action)
    {
        this.name = name;
        this.action = action;
    }
}
