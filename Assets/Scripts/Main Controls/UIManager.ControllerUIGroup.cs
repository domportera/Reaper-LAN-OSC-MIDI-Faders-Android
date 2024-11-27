using System;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager
{
    private class ControllerUIGroup
    {
        public RectTransform OptionButtonTransform { get; }
        public readonly ISortingMember SortingImpl;
        public ControllerData ControllerData { get; }
        private GameObject _optionsMenu;
        private readonly Func<GameObject> _createOptionsMenu;

        public event EventHandler DeletionRequested;

        public ControllerUIGroup(ControllerData config, GameObject optionsActivateButtonPrefab, RectTransform optionsButtonParent, ISortingMember controlObject, Func<GameObject> createOptionsMenu)
        {
            ControllerData = config;
            _createOptionsMenu = createOptionsMenu;
            SortingImpl = controlObject;
            SortingImpl.SetSortButtonVisibility(false);

            var buttonObj = Instantiate(optionsActivateButtonPrefab, optionsButtonParent, false);
            OptionButtonTransform = (RectTransform)buttonObj.transform;
            var activateOptionsButton = buttonObj.GetComponentInChildren<ButtonExtended>();
            activateOptionsButton.gameObject.name = config.Name + " Options Button";
            activateOptionsButton.GetComponentInChildren<Text>().text = config.Name; // change visible button title
            activateOptionsButton.OnClick.AddListener(() => { SetControllerOptionsActive(true); });
            activateOptionsButton.OnPointerHeld.AddListener(Delete);
            SetControllerOptionsActive(false);

            var activationToggle = buttonObj.GetComponentInChildren<Toggle>();
            activationToggle.onValueChanged.AddListener(ToggleControlVisibility);
            activationToggle.SetIsOnWithoutNotify(config.Enabled);
        }

        private void Delete()
        {
            DeletionRequested?.Invoke(this, EventArgs.Empty);
        }

        public void SetControllerOptionsActive(bool active)
        {
            if (active)
            {
                if (!_optionsMenu)
                    _optionsMenu = _createOptionsMenu();
                
                _optionsMenu.SetActive(true);
                return;
            }
            
            if(_optionsMenu)
                Destroy(_optionsMenu);
        }

        private void ToggleControlVisibility(bool b)
        {
            ControllerData.SetEnabled(b);

            if(!b)
            {
                SortingImpl.RectTransform.SetAsLastSibling();
            }
        }

        public void DestroySelf()
        {
            if(_optionsMenu)
                Destroy(_optionsMenu);
            Destroy(OptionButtonTransform.gameObject);
        }
    }
}