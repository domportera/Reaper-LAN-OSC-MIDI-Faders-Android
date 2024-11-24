using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class ErrorWindow : MonoBehaviour
    {

        [FormerlySerializedAs("errorText")] [SerializeField]
        private Text _errorText;
        [FormerlySerializedAs("closeButton")] [SerializeField]
        private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(HideErrorWindow);
        }

        public void Initialize(string text, UnityAction onClose)
        {
            _errorText.text = text;
            if(onClose != null)
            {
                _closeButton.onClick.AddListener(onClose);
            }
        }

        private void HideErrorWindow()
        {
            Destroy(gameObject);
        }
    }
}
