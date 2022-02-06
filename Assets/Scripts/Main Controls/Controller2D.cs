using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DomsUnityHelper;

public class Controller2D : MonoBehaviour, ISortingMember
{
    [Header("Visuals")]
    [SerializeField] RectTransform horizontalLine;
    [SerializeField] RectTransform verticalLine;
    [SerializeField] RectTransform centerDot;

    [Header("Controllers")]
    [SerializeField] Controller horizontalController;
    [SerializeField] Controller verticalController;

    [Header("Interaction")]
    [SerializeField] RectTransform buttonRect;
    [SerializeField] Button button;
    [SerializeField] EventTrigger eventTrigger;

    [Header("Aesthetics")]
    [SerializeField] bool showBorder;
    [SerializeField] GameObject border;
    [SerializeField] Text title;

    [SerializeField] List<Image> controlImages = new List<Image>();

    [Header("Sorting")]
    [SerializeField] Button sortLeftButton;
    [SerializeField] Button sortRightButton;

    bool moving = false;

    static readonly Vector2 NULL_VEC = Vector2.one * float.MinValue;
    Vector2 currentTouchPosition = NULL_VEC;

    float originalWidth;

    Controller2DData controlData;

    [SerializeField] float interactionPadding = 20f;

    bool isUnityEditor = false;

    private void Awake()
    {
#if UNITY_EDITOR
        isUnityEditor = true;
#endif
        originalWidth = buttonRect.rect.width;

        border.SetActive(showBorder);
    }

    private void Start()
    {
        InitializeButtonInteraction();
    }

    enum RectBounds { Left, Right, Top, Bottom }

    // Update is called once per frame
    protected void Update()
    {
        if(moving)
        {
            SetTargetPosition();
        }

        MoveComponentsWithMIDI();
    }

    public void Initialize(Controller2DData _data)
    {
        verticalController.Initialize(_data, 0);
        horizontalController.Initialize(_data, 1);
        controlData = _data;
        title.text = _data.GetName();
        InitializeSorting();
    }

    void InitializeButtonInteraction()
    {
        EventTrigger.Entry startEntry = new EventTrigger.Entry();
        startEntry.eventID = EventTriggerType.PointerDown;
        startEntry.callback.AddListener((data) => { StartTouch(); });
        eventTrigger.triggers.Add(startEntry);

        EventTrigger.Entry endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) => { EndTouch(); });
        eventTrigger.triggers.Add(endEntry);
    }

    void StartTouch()
    {
        moving = true;
    }

    void EndTouch()
    {
        moving = false;
        horizontalController.ReturnToCenter();
        verticalController.ReturnToCenter();
    }

    void MoveComponentsWithMIDI()
    {
        //this inverse lerp is technically unnecessary at time of writing, but if for whatever reason the min and max controller value changes from 0-1, this will handle that
        float xPercent = Mathf.InverseLerp(Controller.MIN_CONTROLLER_VALUE, Controller.MAX_CONTROLLER_VALUE, horizontalController.SmoothValue);
        float yPercent = Mathf.InverseLerp(Controller.MIN_CONTROLLER_VALUE, Controller.MAX_CONTROLLER_VALUE, verticalController.SmoothValue);

        float xMin = GetRectLocalBounds(RectBounds.Left, buttonRect) + interactionPadding;
        float xMax = GetRectLocalBounds(RectBounds.Right, buttonRect) - interactionPadding;
        float yMin = GetRectLocalBounds(RectBounds.Bottom, buttonRect) + interactionPadding;
        float yMax = GetRectLocalBounds(RectBounds.Top, buttonRect) - interactionPadding;

        verticalLine.localPosition = new Vector3(Mathf.Lerp(xMin, xMax, xPercent),
            verticalLine.localPosition.y,
            verticalLine.localPosition.z);
        horizontalLine.localPosition = new Vector3(horizontalLine.localPosition.x,
            Mathf.Lerp(yMin, yMax, yPercent),
            horizontalLine.localPosition.z);
        centerDot.localPosition = new Vector3(verticalLine.localPosition.x, horizontalLine.localPosition.y, centerDot.localPosition.z);

        //Debug.Log($"modValue: {horizontalController.modValue} | xPercent: {xPercent} | xMin and max ({xMin}, {xMax})");
    }

    void SetTargetPosition()
    {
        bool noCurrentTouch = currentTouchPosition == NULL_VEC;
        if (noCurrentTouch)
        {
            currentTouchPosition = GetTouchNearestToCenter();
        }
        else
        {
            currentTouchPosition = GetTouchNearestToTarget();
        }

        Vector2 touchPositionAsPercentage = GetTouchPositionWithinButton(currentTouchPosition);
        // Debug.Log($"Touch position: {currentTouchPosition.ToString("f0")} Percentage: {touchPositionAsPercentage.ToString("f2")}");

        //this mapping is technically unnecessary at time of writing, but if for whatever reason the min and max controller value changes from 0-1, this will handle that
        Vector2 mappedTouchPosition;
        mappedTouchPosition.x = touchPositionAsPercentage.x.Map(0f, 1f, Controller.MIN_CONTROLLER_VALUE, Controller.MAX_CONTROLLER_VALUE);
        mappedTouchPosition.y = touchPositionAsPercentage.y.Map(0f, 1f, Controller.MIN_CONTROLLER_VALUE, Controller.MAX_CONTROLLER_VALUE);

        horizontalController.SetValue(mappedTouchPosition.x);
        verticalController.SetValue(mappedTouchPosition.y);
    }

    /// <summary>
    /// Returns touch position on button as a percentage of its x and y dimensions
    /// </summary>
    /// <param name="_touchPos"></param>
    /// <returns></returns>
    Vector2 GetTouchPositionWithinButton(Vector2 _touchPos)
    {
        float xMin = GetRectScreenBounds(RectBounds.Left, buttonRect) + interactionPadding;
        float xMax = GetRectScreenBounds(RectBounds.Right, buttonRect) - interactionPadding;
        float yMin = GetRectScreenBounds(RectBounds.Bottom, buttonRect) + interactionPadding;
        float yMax = GetRectScreenBounds(RectBounds.Top, buttonRect) - interactionPadding;

        float xPercent = Mathf.InverseLerp(xMin, xMax, _touchPos.x);
        float yPercent = Mathf.InverseLerp(yMin, yMax, _touchPos.y);

        //Debug.Log($"Button info - X: ({xMin}, {xMax}), Y: ({yMin}, {yMax}) - Rect width and height: {buttonRect.rect.width}, {buttonRect.rect.height}");
        //Debug.Log($"Button position: {buttonPos.ToString("f1")}");
        return new Vector2(xPercent, yPercent);
    }

    float GetRectScreenBounds(RectBounds _side, RectTransform _rect)
    {
        Vector2 _rectPos = GetRectScreenPosition(_rect);
        switch (_side)
        { 
            case RectBounds.Left:
                Vector3 rawLeft = new Vector3(_rect.position.x - buttonRect.rect.width * buttonRect.pivot.x, _rect.position.y, _rect.position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawLeft).x;
            case RectBounds.Right:
                Vector3 rawRight = new Vector3(_rect.position.x + buttonRect.rect.width * (1 - buttonRect.pivot.x), _rect.position.y, _rect.position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawRight).x;
            case RectBounds.Top:
                Vector3 rawTop = new Vector3(_rect.position.x, _rectPos.y + buttonRect.rect.height * (1 - buttonRect.pivot.y), _rect.position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawTop).y;
            case RectBounds.Bottom:
                Vector3 rawBottom = new Vector3(_rect.position.x, _rectPos.y - buttonRect.rect.height * buttonRect.pivot.y, _rect.position.z);
                return RectTransformUtility.WorldToScreenPoint(null, rawBottom).y;
            default:
                Debug.LogError($"Button bound {_side} not implemented", this);
                return 0;
        }
    }

    float GetRectLocalBounds(RectBounds _side, RectTransform _rect)
    {
        switch (_side)
        {
            case RectBounds.Left:
                return -_rect.rect.width * _rect.pivot.x;
            case RectBounds.Right:
                return _rect.rect.width * (1 - _rect.pivot.x);
            case RectBounds.Top:
                return _rect.rect.height * (1 - _rect.pivot.y); ;
            case RectBounds.Bottom:
                return -_rect.rect.height * _rect.pivot.y; ;
            default:
                Debug.LogError($"Rect bound {_side} not implemented", this);
                return 0;
        }
    }

    Vector2 GetRectScreenPosition(RectTransform _rect)
    {
        return RectTransformUtility.WorldToScreenPoint(null, _rect.position);
    }

    Vector2 GetTouchNearestToCenter()
    {
        Vector2 buttonPos = GetRectScreenPosition(buttonRect);
        return GetTouchNearestTo(buttonPos);
    }

    Vector2 GetTouchNearestToTarget()
    {
        return GetTouchNearestTo(currentTouchPosition);
    }

    Vector2 GetTouchNearestTo(Vector2 _pos)
    {
        if (isUnityEditor)
        {
            return Input.mousePosition;
        }


        int touchCount = Input.touches.Length;
        Vector2 nearest = NULL_VEC;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < touchCount; i++)
        {
            Vector2 pos = Input.touches[i].position;

            if (nearest == NULL_VEC)
            {
                nearest = pos;
                continue;
            }

            if(Vector2.Distance(pos, _pos) < nearestDistance)
            {
                nearest = pos;
            }
        }

        if(nearest == NULL_VEC)
        {
            Debug.LogError($"Touch position is null!", this);
        }
        return nearest;
    }

    public struct Controller2DFaderProperties
    {
        public bool showBorder;
        public float widthModifier;
        public ControllerSettings horizontalController;
        public ControllerSettings verticalController;
    }

    #region Sorting
    public void InitializeSorting()
    {
        sortLeftButton.onClick.AddListener(SortLeft);
        sortRightButton.onClick.AddListener(SortRight);
        SetSortButtonVisibility(false);
    }

    public void SetSortButtonVisibility(bool _visible)
    {
        sortLeftButton.gameObject.SetActive(_visible);
        sortRightButton.gameObject.SetActive(_visible);
        foreach (Image i in controlImages)
        {
            i.enabled = !_visible;
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

    public void SortPosition(bool _right)
    {
        transform.SetSiblingIndex(_right ? transform.GetSiblingIndex() + 1 : transform.GetSiblingIndex() - 1);
    }
    #endregion Sorting

}
