using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller2D : MonoBehaviour, ISortingMember
{
    [Header("Visuals")]
    [SerializeField]
    private RectTransform _horizontalLine;
    [SerializeField] private RectTransform _verticalLine;
    [SerializeField] private RectTransform _centerDot;

    [Header("Controllers")]
    [SerializeField]
    private Controller _horizontalController;
    [SerializeField] private Controller _verticalController;

    [Header("Interaction")]
    [SerializeField]
    private RectTransform _buttonTransform;
    [SerializeField] private Button _button;
    [SerializeField] private EventTrigger _eventTrigger;

    [Header("Aesthetics")]
    [SerializeField]
    private bool _showBorder;
    [SerializeField] private GameObject _border;
    [SerializeField] private Text _title;

    [SerializeField] private List<Image> _controlImages = new();

    [Header("Sorting")]
    [SerializeField]
    private Button _sortLeftButton;
    [SerializeField] private Button _sortRightButton;

    private bool _moving = false;

    private static readonly Vector2 NullVec = Vector2.one * float.MinValue;
    private Vector2 _currentTouchPosition = NullVec;

    [SerializeField] private float _interactionPadding = 20f;

    private bool _isUnityEditor;

    private void Awake()
    {
#if UNITY_EDITOR
        _isUnityEditor = true;
#endif

        _border.SetActive(_showBorder);
    }

    private void Start()
    {
        InitializeButtonInteraction();
    }

    private enum RectBounds { Left, Right, Top, Bottom }

    // Update is called once per frame
    protected void Update()
    {
        if(_moving)
        {
            SetTargetPosition();
        }

        MoveComponentsWithMidi();
    }

    public void Initialize(Controller2DData data)
    {
        _verticalController.Initialize(data, 0);
        _horizontalController.Initialize(data, 1);
        _title.text = data.GetName();
        InitializeSorting();
    }

    private void InitializeButtonInteraction()
    {
        var startEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        
        startEntry.callback.AddListener((data) => { StartTouch(); });
        _eventTrigger.triggers.Add(startEntry);

        var endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) => { EndTouch(); });
        _eventTrigger.triggers.Add(endEntry);
    }

    private void StartTouch()
    {
        _moving = true;
    }

    private void EndTouch()
    {
        _moving = false;
        _horizontalController.ReturnToCenter();
        _verticalController.ReturnToCenter();
    }

    private void MoveComponentsWithMidi()
    {
        //this inverse lerp is technically unnecessary at time of writing, but if for whatever reason the min and max controller value changes from 0-1, this will handle that
        var xPercent = Mathf.InverseLerp(Controller.MinControllerValue, Controller.MaxControllerValue, _horizontalController.SmoothValue);
        var yPercent = Mathf.InverseLerp(Controller.MinControllerValue, Controller.MaxControllerValue, _verticalController.SmoothValue);

        var xMin = GetRectLocalBounds(RectBounds.Left, _buttonTransform) + _interactionPadding;
        var xMax = GetRectLocalBounds(RectBounds.Right, _buttonTransform) - _interactionPadding;
        var yMin = GetRectLocalBounds(RectBounds.Bottom, _buttonTransform) + _interactionPadding;
        var yMax = GetRectLocalBounds(RectBounds.Top, _buttonTransform) - _interactionPadding;

        var verticalPosition = _verticalLine.localPosition;
        verticalPosition = new Vector3(Mathf.Lerp(xMin, xMax, xPercent),
            verticalPosition.y,
            verticalPosition.z);
        
        var horizontalPosition = _horizontalLine.localPosition;
        horizontalPosition = new Vector3(horizontalPosition.x,
            Mathf.Lerp(yMin, yMax, yPercent),
            horizontalPosition.z);
        
        _verticalLine.localPosition = verticalPosition;
        _horizontalLine.localPosition = horizontalPosition;
        _centerDot.localPosition = new Vector3(verticalPosition.x, horizontalPosition.y, _centerDot.localPosition.z);

        //Debug.Log($"modValue: {horizontalController.modValue} | xPercent: {xPercent} | xMin and max ({xMin}, {xMax})");
    }

    private void SetTargetPosition()
    {
        var noCurrentTouch = _currentTouchPosition == NullVec;
        _currentTouchPosition = noCurrentTouch ? GetTouchNearestToCenter() : GetTouchNearestToTarget();

        var touchPositionAsPercentage = GetTouchPositionWithinButton(_currentTouchPosition);
        // Debug.Log($"Touch position: {currentTouchPosition.ToString("f0")} Percentage: {touchPositionAsPercentage.ToString("f2")}");

        //this mapping is technically unnecessary at time of writing, but if for whatever reason the min and max controller value changes from 0-1, this will handle that
        Vector2 mappedTouchPosition;
        mappedTouchPosition.x = touchPositionAsPercentage.x.Map(0f, 1f, Controller.MinControllerValue, Controller.MaxControllerValue);
        mappedTouchPosition.y = touchPositionAsPercentage.y.Map(0f, 1f, Controller.MinControllerValue, Controller.MaxControllerValue);

        _horizontalController.SetValue(mappedTouchPosition.x);
        _verticalController.SetValue(mappedTouchPosition.y);
    }

    /// <summary>
    /// Returns touch position on button as a percentage of its x and y dimensions
    /// </summary>
    /// <param name="_touchPos"></param>
    /// <returns></returns>
    private Vector2 GetTouchPositionWithinButton(Vector2 touchPos)
    {
        var xMin = GetRectScreenBounds(RectBounds.Left, _buttonTransform) + _interactionPadding;
        var xMax = GetRectScreenBounds(RectBounds.Right, _buttonTransform) - _interactionPadding;
        var yMin = GetRectScreenBounds(RectBounds.Bottom, _buttonTransform) + _interactionPadding;
        var yMax = GetRectScreenBounds(RectBounds.Top, _buttonTransform) - _interactionPadding;

        var xPercent = Mathf.InverseLerp(xMin, xMax, touchPos.x);
        var yPercent = Mathf.InverseLerp(yMin, yMax, touchPos.y);

        //Debug.Log($"Button info - X: ({xMin}, {xMax}), Y: ({yMin}, {yMax}) - Rect width and height: {buttonRect.rect.width}, {buttonRect.rect.height}");
        //Debug.Log($"Button position: {buttonPos.ToString("f1")}");
        return new Vector2(xPercent, yPercent);
    }

    private float GetRectScreenBounds(RectBounds side, RectTransform rect)
    {
        var rectScreenPosition = GetRectScreenPosition(rect);
        var position = rect.position;
        switch (side)
        { 
            case RectBounds.Left:
                var rawLeft = new Vector3(position.x - _buttonTransform.rect.width * _buttonTransform.pivot.x, position.y, position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawLeft).x;
            case RectBounds.Right:
                var rawRight = new Vector3(rect.position.x + _buttonTransform.rect.width * (1 - _buttonTransform.pivot.x), position.y, position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawRight).x;
            case RectBounds.Top:
                var rawTop = new Vector3(rect.position.x, rectScreenPosition.y + _buttonTransform.rect.height * (1 - _buttonTransform.pivot.y), position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawTop).y;
            case RectBounds.Bottom:
                var rawBottom = new Vector3(rect.position.x, rectScreenPosition.y - _buttonTransform.rect.height * _buttonTransform.pivot.y, position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawBottom).y;
            default:
                Debug.LogError($"Button bound {side} not implemented", this);
                return 0;
        }
    }

    private float GetRectLocalBounds(RectBounds side, RectTransform rect)
    {
        switch (side)
        {
            case RectBounds.Left:
                return -rect.rect.width * rect.pivot.x;
            case RectBounds.Right:
                return rect.rect.width * (1 - rect.pivot.x);
            case RectBounds.Top:
                return rect.rect.height * (1 - rect.pivot.y); ;
            case RectBounds.Bottom:
                return -rect.rect.height * rect.pivot.y; ;
            default:
                Debug.LogError($"Rect bound {side} not implemented", this);
                return 0;
        }
    }

    private Vector2 GetRectScreenPosition(RectTransform rect)
    {
        return RectTransformUtility.WorldToScreenPoint(null, rect.position);
    }

    private Vector2 GetTouchNearestToCenter()
    {
        var buttonPos = GetRectScreenPosition(_buttonTransform);
        return GetTouchNearestTo(buttonPos);
    }

    private Vector2 GetTouchNearestToTarget()
    {
        return GetTouchNearestTo(_currentTouchPosition);
    }

    private Vector2 GetTouchNearestTo(Vector2 position)
    {
        if (_isUnityEditor)
        {
            return Input.mousePosition;
        }


        var touchCount = Input.touches.Length;
        var nearest = NullVec;
        var nearestDistance = float.PositiveInfinity;

        for (var i = 0; i < touchCount; i++)
        {
            var pos = Input.touches[i].position;

            if (nearest == NullVec)
            {
                nearest = pos;
                continue;
            }

            if(Vector2.Distance(pos, position) < nearestDistance)
            {
                nearest = pos;
            }
        }

        if(nearest == NullVec)
        {
            Debug.LogError($"Touch position is null!", this);
        }
        return nearest;
    }

    #region Sorting
    public void InitializeSorting()
    {
        _sortLeftButton.onClick.AddListener(SortLeft);
        _sortRightButton.onClick.AddListener(SortRight);
        SetSortButtonVisibility(false);
    }

    public void SetSortButtonVisibility(bool visible)
    {
        _sortLeftButton.gameObject.SetActive(visible);
        _sortRightButton.gameObject.SetActive(visible);
        foreach (var i in _controlImages)
        {
            i.enabled = !visible;
        }
    }

    public void SortLeft()
    {
        SortPosition(false);
    }

    public void SortRight()
    {
        SortPosition(true);
    }

    public void SortPosition(bool right)
    {
        transform.SetSiblingIndex(right ? transform.GetSiblingIndex() + 1 : transform.GetSiblingIndex() - 1);
    }
    #endregion Sorting

}
