using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class ConfirmationWindow : MonoBehaviour
    {
        [FormerlySerializedAs("root")] [SerializeField] GameObject _root;
        [FormerlySerializedAs("confirmButton")] [SerializeField] Button _confirmButton;
        [FormerlySerializedAs("cancelButton")] [SerializeField] Button _cancelButton;
        [FormerlySerializedAs("confirmButtonText")] [SerializeField] Text _confirmButtonText;
        [FormerlySerializedAs("cancelButtonText")] [SerializeField] Text _cancelButtonText;
        [FormerlySerializedAs("descriptionText")] [SerializeField] Text _descriptionText;

        [FormerlySerializedAs("inputText")] [SerializeField] InputField _inputText;
        [FormerlySerializedAs("inputTitle")] [SerializeField] Text _inputTitle;
        UnityAction<string> _inputAction;

        public UnityEvent OnClose = new UnityEvent();

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

        void ConfirmTextEntry()
        {
            _inputAction(_inputText.text);
        }

        void ActivateInputVersion()
        {
            _inputText.gameObject.SetActive(true);
            _inputTitle.gameObject.SetActive(true);
            _descriptionText.gameObject.SetActive(false);
        }

        void ActivateBasicVersion()
        {
            _inputText.gameObject.SetActive(false);
            _inputTitle.gameObject.SetActive(false);
            _descriptionText.gameObject.SetActive(true);
        }


        void Close()
        {
            OnClose.Invoke();
            Destroy(_root);
        }
    }
}
