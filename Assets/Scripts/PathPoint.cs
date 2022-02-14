using System;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    public Action OnPathPointMove;
    public void SetPosition(Vector2 position)
    {
        transform.position = position;
        OnPathPointMove?.Invoke();
    }
}
