using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller2D : MonoBehaviour
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
    [SerializeField] GameObject border;

    bool moving = false;

    readonly Vector2 NULL_VEC = Vector2.negativeInfinity;
    Vector2 currentTouchPosition;

    float originalWidth;

    ControlsManager.Controller2DData controlData;

    [SerializeField] float interactionPadding = 20f;

    bool isUnityEditor = false;

    private void Awake()
    {
#if UNITY_EDITOR
        isUnityEditor = true;
#endif
        currentTouchPosition = NULL_VEC;
        originalWidth = buttonRect.rect.width;
        InitializeButtonInteraction();
    }

    private void Start()
    {
        
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

    public void Initialize(ControlsManager.Controller2DData _data)
    {
        verticalController.Initialize(_data, 0);
        horizontalController.Initialize(_data, 1);
        controlData = _data;
    }

    public void SetSortButtonVisibility(bool _visible)
    {
        throw new NotImplementedException();
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
        float xPercent = Mathf.InverseLerp(horizontalController.controllerSettings.min,
            horizontalController.controllerSettings.max,
            horizontalController.modValue);
        float yPercent = Mathf.InverseLerp(verticalController.controllerSettings.min,
            verticalController.controllerSettings.max,
            verticalController.modValue);

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
        if (currentTouchPosition == NULL_VEC)
        {
            currentTouchPosition = GetTouchNearestToCenter();
        }
        else
        {
            currentTouchPosition = GetTouchNearestToTarget();
        }

        Vector2 touchPositionAsPercentage = GetTouchPositionWithinButton(currentTouchPosition);
       // Debug.Log($"Touch position: {currentTouchPosition.ToString("f0")} Percentage: {touchPositionAsPercentage.ToString("f2")}");
        horizontalController.SetValueAsPercentage(touchPositionAsPercentage.x);
        verticalController.SetValueAsPercentage(touchPositionAsPercentage.y);
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
                Debug.LogError($"Button bound {_side} not implemented", this);
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

        int touchCount = Input.touchCount;
        Vector2 nearest = NULL_VEC;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < touchCount; i++)
        {
            Vector2 pos = Input.GetTouch(i).position;

            if(nearest == NULL_VEC)
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


}
