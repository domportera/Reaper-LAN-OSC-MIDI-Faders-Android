using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class MultiOptionWindow : MonoBehaviour
    {
        [FormerlySerializedAs("buttonPrefab")] [SerializeField] GameObject _buttonPrefab;
        [FormerlySerializedAs("buttonParent")] [SerializeField] Transform _buttonParent;
        [FormerlySerializedAs("text")] [SerializeField] Text _text;
        [FormerlySerializedAs("closeButton")] [SerializeField] Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseWindow);
        }

        public void SetActions(string text, MultiOptionAction[] actions)
        {
            this._text.text = text;

            foreach(MultiOptionAction a in actions)
            {
                GameObject newButtonObj = Instantiate(_buttonPrefab, _buttonParent);
                newButtonObj.SetActive(true);
                Button button = newButtonObj.GetComponent<Button>();
                button.GetComponentInChildren<Text>().text = a.Name;
                button.onClick.AddListener(() => { a.Action.Invoke(); });
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
        public string Name;
        public Action Action;

        public MultiOptionAction(string name, Action action)
        {
            this.Name = name;
            this.Action = action;
        }
    }
}