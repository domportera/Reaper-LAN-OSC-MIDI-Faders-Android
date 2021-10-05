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

    bool moving = false;

    readonly Vector2 NULL_VEC = Vector2.negativeInfinity;
    Vector2 currentTouchPosition;

    private void Awake()
    {
        currentTouchPosition = NULL_VEC;

        horizontalController.Initialize(ControlsManager.defaultControllers[0]);
        verticalController.Initialize(ControlsManager.defaultControllers[1]);
        InitializeButtonInteraction();
    }

    // Update is called once per frame
    protected void Update()
    {
        if(moving)
        {
            SetTargetPosition();
        }

        MoveComponentsWithMIDI();
    }

    void InitializeButtonInteraction()
    {
        EventTrigger.Entry startEntry = new EventTrigger.Entry();
        startEntry.eventID = EventTriggerType.PointerDown;
        startEntry.callback.AddListener((data) => { EnableMovement(); });
        eventTrigger.triggers.Add(startEntry);

        EventTrigger.Entry endEntry = new EventTrigger.Entry();
        endEntry.eventID = EventTriggerType.PointerUp;
        endEntry.callback.AddListener((data) => { DisableMovement(); });
        eventTrigger.triggers.Add(endEntry);
    }

    void EnableMovement()
    {
        moving = true;
    }

    void DisableMovement()
    {
        moving = false;
    }

    void MoveComponentsWithMIDI()
    {
        
        
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
        Vector2 buttonPos = GetButtonScreenPosition();
        float xMin = buttonPos.x - buttonRect.rect.width * buttonRect.pivot.x;
        float xMax = buttonPos.x + buttonRect.rect.width * (1 - buttonRect.pivot.x);
        float yMin = buttonPos.y - buttonRect.rect.height * buttonRect.pivot.y;
        float yMax = buttonPos.y + buttonRect.rect.height * (1 - buttonRect.pivot.y);

        float xPercent = Mathf.InverseLerp(xMin, xMax, _touchPos.x);
        float yPercent = Mathf.InverseLerp(yMin, yMax, _touchPos.y);

        //Debug.Log($"Button info - X: ({xMin}, {xMax}), Y: ({yMin}, {yMax}) - Rect width and height: {buttonRect.rect.width}, {buttonRect.rect.height}");
        //Debug.Log($"Button position: {buttonPos.ToString("f1")}");
        return new Vector2(xPercent, yPercent);
    }

    Vector2 GetButtonScreenPosition()
    {
        return RectTransformUtility.WorldToScreenPoint(null, buttonRect.position);
    }

    Vector2 GetTouchNearestToCenter()
    {
        Vector2 buttonPos = GetButtonScreenPosition();
        return GetTouchNearestTo(buttonPos);
    }

    Vector2 GetTouchNearestToTarget()
    {
        return GetTouchNearestTo(currentTouchPosition);
    }

    Vector2 GetTouchNearestTo(Vector2 _pos)
    {
#if UNITY_EDITOR
        return Input.mousePosition;
#endif
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

}
