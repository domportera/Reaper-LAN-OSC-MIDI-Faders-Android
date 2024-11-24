using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class QuickNoticeWindow : MonoBehaviour
    {
        [FormerlySerializedAs("confirmationText")] [SerializeField]
        private Text _confirmationText;
        [FormerlySerializedAs("closeButton")] [SerializeField]
        private Button _closeButton;

        private UnityAction _onHide;

        // Start is called before the first frame update
        private void Awake()
        {
            _closeButton.onClick.AddListener(HideConfirmationWindow);
        }

        public void Initialize(string text, float hideDelay, UnityAction onHide)
        {
            this._onHide = onHide;
            _confirmationText.text = text;
            StartCoroutine(HideConfirmationWindowAfterDelay(hideDelay));
        }

        private IEnumerator HideConfirmationWindowAfterDelay(float hideDelay)
        {
            yield return new WaitForSeconds(hideDelay);
            HideConfirmationWindow();
        }

        private void HideConfirmationWindow()
        {
            _onHide?.Invoke();
            Destroy(gameObject);
        }
    }
}
