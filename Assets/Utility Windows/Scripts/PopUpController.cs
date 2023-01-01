using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class PopUpController : MonoBehaviour
    {
        [FormerlySerializedAs("windowParent")] [SerializeField] RectTransform _windowParent = null;
        [FormerlySerializedAs("toastParent")] [SerializeField] RectTransform _toastParent = null;
        [FormerlySerializedAs("confirmationDisplayTime")] [SerializeField] float _confirmationDisplayTime = 3f;

        [FormerlySerializedAs("confirmationWindowPrefab")]
        [Space(20)]
        [SerializeField] GameObject _confirmationWindowPrefab = null;
        [FormerlySerializedAs("multiOptionWindowPrefab")] [SerializeField] GameObject _multiOptionWindowPrefab = null;
        [FormerlySerializedAs("sliderWindowPrefab")] [SerializeField] GameObject _sliderWindowPrefab = null;
        [FormerlySerializedAs("errorWindowPrefab")] [SerializeField] GameObject _errorWindowPrefab = null;
        [FormerlySerializedAs("quickNoticeWindowPrefab")] [SerializeField] GameObject _quickNoticeWindowPrefab = null;
        [FormerlySerializedAs("toastPrefab")] [SerializeField] GameObject _toastPrefab = null;

        [FormerlySerializedAs("toastColors")] [SerializeField] ToastColor[] _toastColors = Array.Empty<ToastColor>();
        public ObjectPool ToastPool { get; private set; }

        [System.Serializable]
        public struct WindowColors
        {
            [FormerlySerializedAs("borderColor")] public Color BorderColor;
            [FormerlySerializedAs("titleColor")] public Color TitleColor;
            [FormerlySerializedAs("descriptionColor")] public Color DescriptionColor;
            [FormerlySerializedAs("buttonColor")] public Color ButtonColor;
            [FormerlySerializedAs("buttonTextColor")] public Color ButtonTextColor;
            [FormerlySerializedAs("backgroundColor")] public Color BackgroundColor;
        }

        [System.Serializable]
        public struct ToastColor
        {
            [FormerlySerializedAs("type")] public LogType Type;
            [FormerlySerializedAs("color")] public Color Color;
        }

        public static PopUpController Instance;

        readonly List<Action> _actionsToPerformOnUpdate = new();
        [FormerlySerializedAs("toastLifespan")] public float ToastLifespan;
        [FormerlySerializedAs("toastFadeCurve")] public AnimationCurve ToastFadeCurve;

        public Dictionary<LogType, Color> ToastColors { get; private set; }

        private void Awake()
        {
            ToastColors = _toastColors.ToDictionary(x => x.Type, x => x.Color);
            ToastPool = new ObjectPool(_toastPrefab, _toastParent, 0);
            SingletonSetup();
        }

        private void Update()
        {
            int count = _actionsToPerformOnUpdate.Count;
            while(_actionsToPerformOnUpdate.Count > 0)
            {
                _actionsToPerformOnUpdate[0].Invoke();
                _actionsToPerformOnUpdate.RemoveAt(0);
            }
        }

        public void ErrorWindowOnThread(string text)
        {
            _actionsToPerformOnUpdate.Add(() => ErrorWindow(text));
        }

        public void ErrorWindow(string text, UnityAction onClose = null)
        {
            GameObject window = InstantiateWindow(_errorWindowPrefab);
            ErrorWindow error = window.GetComponent<ErrorWindow>();
            error.Initialize(text, onClose);
        }

        public void QuickNoticeWindowOnThread(string text, UnityAction onComplete = null)
        {
            _actionsToPerformOnUpdate.Add(() => QuickNoticeWindow(text, onComplete));
        }

        public void QuickNoticeWindow(string text, UnityAction onComplete = null)
        {
            GameObject window = InstantiateWindow(_quickNoticeWindowPrefab);
            QuickNoticeWindow confirm = window.GetComponent<QuickNoticeWindow>();
            confirm.Initialize(text, _confirmationDisplayTime, onComplete);
        }

        public void ToastOnThread(string text, LogType type = LogType.Log, bool log = false)
        {
            _actionsToPerformOnUpdate.Add(() => Toast(text, type, log));
        }

        public void Toast(string text, LogType type = LogType.Log, bool log = false)
        {
            Toast toast = ToastPool.Instantiate().GetComponent<Toast>();
            toast.Initialize(text, type, log);
        }

        public void ConfirmationWindow(string text, UnityAction confirm, UnityAction cancel = null, string confirmButtonLabel = null, string cancelButtonLabel = null)
        {
            GameObject window = InstantiateWindow(_confirmationWindowPrefab);
            ConfirmationWindow verify = window.GetComponent<ConfirmationWindow>();
            verify.SetActions(text, confirm, cancel, confirmButtonLabel, cancelButtonLabel);
        }

        public void TextInputWindow(string inputLabel, UnityAction<string> confirm, UnityAction cancel = null, string confirmButtonLabel = null, string cancelButtonLabel = null, InputField.ContentType contentType = InputField.ContentType.Standard)
        {
            GameObject window = InstantiateWindow(_confirmationWindowPrefab);
            ConfirmationWindow verify = window.GetComponent<ConfirmationWindow>();
            verify.SetActionsInputField(inputLabel, confirm, cancel, confirmButtonLabel, cancelButtonLabel);
        }

        public void MultiOptionWindow(string text, params MultiOptionAction[] actions)
        {
            GameObject window = InstantiateWindow(_multiOptionWindowPrefab);
            MultiOptionWindow multiWindow = window.GetComponent<MultiOptionWindow>();
            multiWindow.SetActions(text, actions);
        }

        public void SliderWindow(string text, string description, float defaultValue, float min, float max, bool roundToInt, UnityAction<float> sliderAction, UnityAction onCancel = null)
        {
            GameObject window = InstantiateWindow(_sliderWindowPrefab);
            SliderWindow sliderWindow = window.GetComponent<SliderWindow>();
            sliderWindow.SetActions(text, description, defaultValue, min, max, roundToInt, sliderAction, onCancel);
        }


        GameObject InstantiateWindow(GameObject prefab)
        {
            GameObject window = Instantiate(prefab, _windowParent);
            return window;
        }

        void SingletonSetup()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning($"Can't reference more than one Utility Windows manager as a singleton (\"instance\"). Ensure you are using direct references.", this);
            }
        }
    }
}
