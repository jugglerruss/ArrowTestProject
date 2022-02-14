using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PathPointSpawner : MonoBehaviour
{
    [SerializeField] private PathPoint _pathPointPrefab;
    [FormerlySerializedAs("HandleRadius")] [SerializeField] public float _handleRadius = 20;
    public List<PathPoint> PathPoints { get; private set; }
    public UnityEvent<List<PathPoint>> OnPathPointCreate;
    public UnityEvent OnPathPointMove;
    public UnityEvent<List<PathPoint>> OnStart;
    private void Start()
    {
        PathPoints = FindObjectsOfType<PathPoint>().ToList();
        foreach (var point in PathPoints)
        {
            point.OnPathPointMove += PathPointMove;
        }
        OnStart?.Invoke(PathPoints);
    }
    private void OnMouseDown()
    {
        if (Camera.main != null)
            InstantiatePoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }
    private void PathPointMove()
    {
        OnPathPointMove?.Invoke();
    }
    public void InstantiatePoint(Vector2 pos)
    {
        var pathPoint = Instantiate(_pathPointPrefab, transform);
        pathPoint.transform.position = pos;
        pathPoint.OnPathPointMove += PathPointMove;
        pathPoint.SetPosition(pos);
        PathPoints.Add(pathPoint);
        OnPathPointCreate?.Invoke(PathPoints);
    }
}
