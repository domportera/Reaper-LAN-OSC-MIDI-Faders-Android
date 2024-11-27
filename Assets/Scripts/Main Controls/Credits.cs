using System;
using System.Collections.Generic;
using PopUpWindows;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Main_Controls
{
    public class Credits : MonoBehaviour
    {
        [Header("Credits")] [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _closeCreditsButton;
        [SerializeField] private GameObject _creditsPanel;
        [SerializeField] private List<CreditsButton> _creditsButtons = new();
        [SerializeField] private List<DonationButton> _donationButtons = new();

        [Serializable]
        private struct CreditsButton
        {
            public Button Button;
            public string Link;
        }

        [Serializable]
        private struct DonationButton
        {
            public Button Button;
            public string Address;
        }

        private void Awake()
        {
            UnityAction toggleCredits = () => _creditsPanel.SetActive(!_creditsPanel.activeSelf);
            _creditsButton.onClick.AddListener(toggleCredits);
            _closeCreditsButton.onClick.AddListener(toggleCredits);

            InitializeCreditsButtons();
            InitializeDonationButtons();
            return;

            void InitializeCreditsButtons()
            {
                foreach (var b in _creditsButtons)
                {
                    b.Button.onClick.AddListener(() => Application.OpenURL(b.Link));
                }
            }

            void InitializeDonationButtons()
            {
                foreach (var b in _donationButtons)
                {
                    b.Button.onClick.AddListener(() =>
                    {
                        UniClipboard.SetText(b.Address);
                        PopUpController.Instance.QuickNoticeWindow($"Copied {b.Address} to clipboard!");
                    });
                }
            }
        }
    }
}