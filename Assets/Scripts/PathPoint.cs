using System;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public Action OnPathPointMove;
    private void OnMouseDrag()
    {
       // transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
       // OnPathPointMove?.Invoke();
    }
    private void OnMouseDown()
    {
       // transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    public void SetPosition(Vector2 position)
    {
        transform.position = position;
        OnPathPointMove?.Invoke();
    }
}
