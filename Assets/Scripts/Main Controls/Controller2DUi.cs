using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public sealed class Controller2DUi : MonoBehaviour, ISortingMember
{
    [Header("Visuals")]
    [SerializeField]
    private RectTransform _horizontalLine;
    [SerializeField] private RectTransform _verticalLine;
    [SerializeField] private RectTransform _centerDot;

    private RangeController _horizontalRangeController;
    private RangeController _verticalRangeController;

    [Header("Interaction")]
    [SerializeField]
    private RectTransform _buttonTransform;
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

    private readonly List<int> _touchIds = new();
    private bool HasTouch => _touchIds.Count > 0;

    [SerializeField] private float _interactionPadding = 20f;

    private CanvasScaler _canvasScaler;
    private bool _hasCanvasScaler;

    private void Awake()
    {
        _border.SetActive(_showBorder);
        
        var parent = _buttonTransform.parent;
        while (parent != null)
        {
            if (parent.TryGetComponent(out _canvasScaler))
            {
                _hasCanvasScaler = true;
                break;
            }
            
            parent = parent.parent;
        }
    }

    private void Start()
    {
        InitializeButtonInteraction();
    }

    private enum RectBounds { Left, Right, Top, Bottom }

    // Update is called once per frame
    private void Update()
    {
        SetTargetPosition();
        var dt = Time.deltaTime;
        _verticalRangeController.Update(dt);
        _horizontalRangeController.Update(dt);
        MoveSliders();
    }

    public void Initialize(Controller2DData data)
    {
        _verticalRangeController = new RangeController(data.VerticalController);
        _horizontalRangeController = new RangeController(data.HorizontalController);
        _title.text = data.GetName();
        var rectTransform = GetComponent<RectTransform>();
        var initialSizeDelta = rectTransform.sizeDelta;
        rectTransform.sizeDelta = new Vector2(initialSizeDelta.x * data.GetWidth(), initialSizeDelta.y);
        InitializeSorting();
    }

    private void InitializeButtonInteraction()
    {
        var startEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerDown
        };
        
        startEntry.callback.AddListener(StartTouch);
        _eventTrigger.triggers.Add(startEntry);
        
        var endEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerUp
        };

        endEntry.callback.AddListener(EndTouch);
        _eventTrigger.triggers.Add(endEntry);
        return;

        void StartTouch(BaseEventData arg0)
        {
            // pointer down
            var pointerEventData = (PointerEventData)arg0;
            _touchIds.Add(pointerEventData.pointerId);
        }
        
        void EndTouch(BaseEventData arg0)
        {
            // pointer up
            var pointerEventData = (PointerEventData)arg0;
            _touchIds.Remove(pointerEventData.pointerId);
        }
    }


    private void MoveSliders()
    {
        //this inverse lerp is technically unnecessary at time of writing, but if for whatever reason the min and max controller value changes from 0-1, this will handle that
        var xPercent = Mathf.InverseLerp(RangeController.MinControllerValue, RangeController.MaxControllerValue, _horizontalRangeController.SmoothValue);
        var yPercent = Mathf.InverseLerp(RangeController.MinControllerValue, RangeController.MaxControllerValue, _verticalRangeController.SmoothValue);

        var interactionPadding = _interactionPadding;
        
        var xMin = GetRectLocalBounds(RectBounds.Left, _buttonTransform) + interactionPadding;
        var xMax = GetRectLocalBounds(RectBounds.Right, _buttonTransform) - interactionPadding;
        var yMin = GetRectLocalBounds(RectBounds.Bottom, _buttonTransform) + interactionPadding;
        var yMax = GetRectLocalBounds(RectBounds.Top, _buttonTransform) - interactionPadding;

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
    }

    private void SetTargetPosition()
    {
        if (!HasTouch)
            return;

        if (!TryGetExistingTouch(out var touchPos))
        {
            _horizontalRangeController.Release();
            _verticalRangeController.Release();
            return;
        }

        var touchPositionAsPercentage = GetNormalizedPositionWithinButton(touchPos);

        Vector2 mappedTouchPosition = new()
        {
        //this mapping is technically unnecessary at time of writing, but if for whatever reason the min and max controller value changes from 0-1, this will handle that
            x = touchPositionAsPercentage.x.Map(0f, 1f, RangeController.MinControllerValue, RangeController.MaxControllerValue),
            y = touchPositionAsPercentage.y.Map(0f, 1f, RangeController.MinControllerValue, RangeController.MaxControllerValue)
        };

        _horizontalRangeController.SetValue(mappedTouchPosition.x);
        _verticalRangeController.SetValue(mappedTouchPosition.y);
    }

    private bool TryGetExistingTouch(out Vector2 o)
    {
        var touches = Input.touches;
        for (int i = 0; i < touches.Length; i++)
        {
            for (int j = 0; j < _touchIds.Count; j++)
            {
                if (touches[i].fingerId != _touchIds[j]) continue;
                o = touches[i].position;
                return true;

            }
        }
        
        //https://docs.unity3d.com/2019.1/Documentation/ScriptReference/EventSystems.PointerEventData-pointerId.html
        o = Input.mousePosition;
        return Input.GetMouseButton(0) && _touchIds.Any(x => x== -1); // left click
    }

    /// <summary>
    /// Returns touch position on button as a percentage of its x and y dimensions
    /// </summary>
    /// <returns></returns>
    private Vector2 GetNormalizedPositionWithinButton(Vector2 touchPos)
    {
        var rectTransform = _buttonTransform;
        var rectScreenPosition = GetRectScreenPosition(rectTransform);
        var position = rectTransform.position;

        var scale = 1f;

        if (_hasCanvasScaler)
        {
            var currentResolution = new Vector2(Screen.width, Screen.height);
            var resolution = _canvasScaler.referenceResolution;
            var ratio = currentResolution / resolution;
            scale = _canvasScaler.scaleFactor * Mathf.Min(ratio.x, ratio.y);
        }
        
        var rect = rectTransform.rect;
        var rectPivot = rectTransform.pivot;
        var width = rect.width * scale;
        var height = rect.height * scale;

        var rawLeft = new Vector3(position.x - width * rectPivot.x, position.y, position.z);
        var rawRight = new Vector3(position.x + width * (1 - rectPivot.x), position.y, position.z);
        var rawTop = new Vector3(position.x, rectScreenPosition.y + height * (1 - rectPivot.y), position.z);
        var rawBottom = new Vector3(position.x, rectScreenPosition.y - height * rectPivot.y, position.z);
        
        var xMin = RectTransformUtility.WorldToScreenPoint(null, rawLeft).x + _interactionPadding;
        var xMax = RectTransformUtility.WorldToScreenPoint(null, rawRight).x - _interactionPadding;
        
        var yMax = RectTransformUtility.WorldToScreenPoint(null, rawTop).y - _interactionPadding;
        var yMin = RectTransformUtility.WorldToScreenPoint(null, rawBottom).y + _interactionPadding;
        
        var xPercent = Mathf.InverseLerp(xMin, xMax, touchPos.x);
        var yPercent = Mathf.InverseLerp(yMin, yMax, touchPos.y);

        return new Vector2(xPercent, yPercent);
    }

    private float GetRectLocalBounds(RectBounds side, RectTransform rect)
    {
        var pivot = rect.pivot;
        var width = rect.rect.width;
        var height = rect.rect.height;
        
        switch (side)
        {
            case RectBounds.Left:
                return -width * pivot.x;
            case RectBounds.Right:
                return width * (1 - pivot.x);
            case RectBounds.Top:
                return height * (1 - pivot.y); ;
            case RectBounds.Bottom:
                return -height * pivot.y; ;
            default:
                Debug.LogError($"Rect bound {side} not implemented", this);
                return 0;
        }
    }

    private static Vector2 GetRectScreenPosition(RectTransform rect)
    {
        return RectTransformUtility.WorldToScreenPoint(null, rect.position);
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
