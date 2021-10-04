using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Controller2D : FaderControl
{
    [SerializeField] RectTransform horizontalLine;
    [SerializeField] RectTransform verticalLine;
    [SerializeField] RectTransform centerDot;
    [SerializeField] RectTransform buttonRect;
    [SerializeField] Button button;
    [SerializeField] Canvas canvas;

    bool moving = false;

    readonly Vector2 NULL_VEC = Vector2.negativeInfinity;
    Vector2 currentTargetPosition;

    private void Awake()
    {
        currentTargetPosition = NULL_VEC;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(moving)
        {
            SetTargetPosition();
        }

        MoveComponentsToFinger();
    }

    void EnableMovement()
    {
        moving = true;
    }

    void DisableMovement()
    {
        moving = false;
    }

    void MoveComponentsToFinger()
    {
        
        
    }

    void SetTargetPosition()
    {
        if (currentTargetPosition == NULL_VEC)
        {
            currentTargetPosition = GetTouchNearestToCenter();
        }
        else
        {
            currentTargetPosition = GetTouchNearestToTarget();
        }
    }

    Vector2 GetTouchNearestToCenter()
    {
        Vector2 buttonPos = Camera.main.ViewportToScreenPoint(buttonRect.position);
        return GetTouchNearestTo(buttonPos);
    }

    Vector2 GetTouchNearestToTarget()
    {
        return GetTouchNearestTo(currentTargetPosition);
    }

    Vector2 GetTouchNearestTo(Vector2 _pos)
    {
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

        return nearest;
    }

}
