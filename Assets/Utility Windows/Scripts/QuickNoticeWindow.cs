using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class QuickNoticeWindow : MonoBehaviour
    {
        [FormerlySerializedAs("confirmationText")] [SerializeField] Text _confirmationText = null;
        [FormerlySerializedAs("closeButton")] [SerializeField] Button _closeButton = null;

        UnityAction _onHide = null;

        // Start is called before the first frame update
        void Awake()
        {
            _closeButton.onClick.AddListener(HideConfirmationWindow);
        }

        public void Initialize(string text, float hideDelay, UnityAction onHide)
        {
            this._onHide = onHide;
            _confirmationText.text = text;
            StartCoroutine(HideConfirmationWindowAfterDelay(hideDelay));
        }

        IEnumerator HideConfirmationWindowAfterDelay(float hideDelay)
        {
            yield return new WaitForSeconds(hideDelay);
            HideConfirmationWindow();
        }

        void HideConfirmationWindow()
        {
            _onHide?.Invoke();
            Destroy(gameObject);
        }
    }
}
