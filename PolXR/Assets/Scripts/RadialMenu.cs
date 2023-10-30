using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [Header("Scene")]
    public Transform selectionTransform = null;
    public Transform cursorTransform = null;

    [Header("Events")]
    public RadialSelection top = null;
    public RadialSelection bot = null;
    public RadialSelection left = null;
    public RadialSelection right = null;

    private Vector2 touchPosition = Vector2.zero;
    private List<RadialSelection> radialSelectionList = null;
    private RadialSelection highlighted = null;

    private void Start()
    {
        Show(true);
    }

    private void Show(bool enabled)
    {
        gameObject.SetActive(enabled);
    }

    private void Update()
    {
        Vector2 direction = Vector2.zero + touchPosition;
        float rotation = GetDegree(direction);

        SetCursorPosition();
    }

    private float GetDegree(Vector2 direction)
    {
        float directionAngle = Mathf.Atan2(direction.x, direction.y);
        directionAngle *= Mathf.Rad2Deg;

        if (directionAngle < 0)
            directionAngle += 360.0f;

        return directionAngle;
    }

    private void SetCursorPosition()
    {
        cursorTransform.localPosition = touchPosition;
    }

    public void SetTouchPosition(Vector2 newTouchPosition)
    {
       touchPosition = newTouchPosition;
    }
}
