using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class ConfirmationWindow : MonoBehaviour
    {
        [FormerlySerializedAs("root")] [SerializeField]
        private GameObject _root;
        [FormerlySerializedAs("confirmButton")] [SerializeField]
        private Button _confirmButton;
        [FormerlySerializedAs("cancelButton")] [SerializeField]
        private Button _cancelButton;
        [FormerlySerializedAs("confirmButtonText")] [SerializeField]
        private Text _confirmButtonText;
        [FormerlySerializedAs("cancelButtonText")] [SerializeField]
        private Text _cancelButtonText;
        [FormerlySerializedAs("descriptionText")] [SerializeField]
        private Text _descriptionText;

        [FormerlySerializedAs("inputText")] [SerializeField]
        private InputField _inputText;
        [FormerlySerializedAs("inputTitle")] [SerializeField]
        private Text _inputTitle;

        private UnityAction<string> _inputAction;

        public event Action OnClose;

        public void SetActions(string text, UnityAction confirm, UnityAction cancel = null, string confirmButtonLabel = null, string cancelButtonLabel = null)
        {
            ActivateBasicVersion();
            _descriptionText.text = text;
            _confirmButton.onClick.AddListener(confirm);
            _confirmButton.onClick.AddListener(Close);


            if (cancel != null)
            {
                _cancelButton.onClick.AddListener(cancel);
            }

            _cancelButton.onClick.AddListener(Close);

            if(confirmButtonLabel != null)
            {
                _confirmButtonText.text = confirmButtonLabel;
            }

            if (cancelButtonLabel != null)
            {
                _cancelButtonText.text = cancelButtonLabel;
            }
        }
        public void SetActionsInputField(string inputLabel, UnityAction<string> confirm, UnityAction cancel = null, string confirmButtonLabel = null, string cancelButtonLabel = null, InputField.ContentType contentType = InputField.ContentType.Standard)
        {
            ActivateInputVersion();
            _inputTitle.text = inputLabel;
            _inputAction = confirm;
            _inputText.contentType = contentType;
            _confirmButton.onClick.AddListener(ConfirmTextEntry);
            _confirmButton.onClick.AddListener(Close);

            if (cancel != null)
            {
                _cancelButton.onClick.AddListener(cancel);
            }

            _cancelButton.onClick.AddListener(() => { Close(); });

            if (confirmButtonLabel != null)
            {
                _confirmButtonText.text = confirmButtonLabel;
                Debug.Log($"Confirm button label {confirmButtonLabel}");
            }

            if (cancelButtonLabel != null)
            {
                _cancelButtonText.text = cancelButtonLabel;
            }
        }

        private void ConfirmTextEntry()
        {
            _inputAction(_inputText.text);
        }

        private void ActivateInputVersion()
        {
            _inputText.gameObject.SetActive(true);
            _inputTitle.gameObject.SetActive(true);
            _descriptionText.gameObject.SetActive(false);
        }

        private void ActivateBasicVersion()
        {
            _inputText.gameObject.SetActive(false);
            _inputTitle.gameObject.SetActive(false);
            _descriptionText.gameObject.SetActive(true);
        }


        private void Close()
        {
            OnClose?.Invoke();
            Destroy(_root);
        }
    }
}
