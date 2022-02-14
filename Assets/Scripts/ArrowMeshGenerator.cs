using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArrowMeshGenerator : MonoBehaviour
{
    [SerializeField] private float _startLength;
    [SerializeField] private float _width;
    [SerializeField] private float _endLength;
    [Range(0, 1)] [SerializeField] private float _drawProgress;

    private Mesh _mesh;
    private List<Vector3> _vertices;
    private List<Vector2> _uv;
    private List<int> _triangles;
    private List<PathPoint> _pathPoints;
    private List<Vector2> _pathPositions;
    private List<float> _pathDistances;
    private float _halfWidth;
    private float _endLengthPercent;
    private float _startLengthPercent;
    private Vector2 _currentVector;
    private float _allDistance;
    private float _currentDistance;
    private int _currentTargetPositionIndex;
    private float _startLengthOld;
    private float _widthOld;
    private float _endLengthOld;
    private float _drawProgressOld;

    public void Init(List<PathPoint> pathPoints)
    {
        _pathPoints = pathPoints;
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        _endLengthPercent = 0.1f;
        _startLengthPercent = 0.23f;
        _halfWidth = _width / 2;
        _startLengthOld = _startLength;
        _widthOld = _width;
        _endLengthOld = _endLength;
        _drawProgressOld = _drawProgress;
        FindPath();
    }

    private void Update()
    {
        if (_startLengthOld == _startLength && _widthOld == _width && _endLengthOld == _endLength && _drawProgressOld == _drawProgress)
            return;
        _startLengthOld = _startLength;
        _widthOld = _width;
        _endLengthOld = _endLength;
        _drawProgressOld = _drawProgress;
        CreateShape();
    }
    public void CreateShape()
    {
        _vertices = new List<Vector3>();
        _uv = new List<Vector2>();
        _triangles = new List<int>();
        _currentDistance = _allDistance * _drawProgress;
        _currentTargetPositionIndex = 1;
        for (int i = 1; i < _pathDistances.Count; i++)
        {
            if (_currentDistance <= _pathDistances[i] && 
                _currentDistance >= _pathDistances[i - 1])
                _currentTargetPositionIndex = i;
        }
        CreateEndShape();
        UpdateMesh();
    }
    public void FindPath()
    {
        _pathPositions = new List<Vector2>();
        _pathDistances = new List<float>();
        _allDistance = 0;
        for (int i = 0; i < _pathPoints.Count; i++)
        {
            _pathPositions.Add(_pathPoints[i].transform.position);
            if (i > 0)
            {
                _allDistance += Vector2.Distance(_pathPositions[i], _pathPositions[i - 1]);                
            }
            _pathDistances.Add(_allDistance);
        }
        CreateShape();
    }
    public void AddPathPoint(List<PathPoint> pathPoints)
    {
        _pathPoints = pathPoints;
        FindPath();
    }
    private void CreateEndShape()
    {
        Vector2 vector = _pathPositions[1] - _pathPositions[0];
        var perpendicular = Vector2.Perpendicular(vector);
        var p1 = -perpendicular.normalized * _halfWidth + _pathPositions[0];
        var p2 = perpendicular.normalized * _halfWidth + _pathPositions[0];
        var p3 = vector.normalized * _endLength + p1;
        var p4 = vector.normalized * _endLength + p2;
        CreateShape(p1, p2, p3, p4, 0, 0, _endLengthPercent, _endLengthPercent);
        CreateMiddleShape(p3, p4);
    }
    private void CreateMiddleShape(Vector2 p1, Vector2 p2)
    {
        Vector2 p3 = Vector2.zero;
        Vector2 p4 = Vector2.zero;
        for (int i = 1; i <= _currentTargetPositionIndex; i++)
        {
            _currentVector = _pathPositions[i] - _pathPositions[i - 1];
            var distancePercent = (_currentDistance - _pathDistances[i - 1]) / (_pathDistances[i] - _pathDistances[i - 1]);
            if (distancePercent >= 1 && i + 1 != _pathPositions.Count)
            {
                var nextVector = _pathPositions[i + 1] - _pathPositions[i];
                var angleToCurrentVector = Vector2.SignedAngle(Vector2.right, _currentVector);
                var halfAngleFromCurrentToNextVector = Vector2.SignedAngle(_currentVector, nextVector) / 2;
                var direction = Vector2.Perpendicular(DirectionFromAngle(halfAngleFromCurrentToNextVector + angleToCurrentVector));
                var halfHypotenuse  = _width / (float)(2 * Math.Cos(halfAngleFromCurrentToNextVector * Mathf.Deg2Rad));
                p3 = _pathPositions[i] - direction * halfHypotenuse;
                p4 = _pathPositions[i] + direction * halfHypotenuse;
            }
            else
            {
                var perpendicular = Vector2.Perpendicular(_currentVector.normalized);
                p3 = _pathPositions[i - 1] - perpendicular * _halfWidth + (_currentVector - _currentVector.normalized * _endLength) * distancePercent;
                p4 = _pathPositions[i - 1] + perpendicular * _halfWidth + (_currentVector - _currentVector.normalized * _endLength) * distancePercent;
            }
            CreateShape(p1, p2, p3, p4, _endLengthPercent, _endLengthPercent, 1 - _startLengthPercent, 1 - _startLengthPercent);
            p1 = p3;
            p2 = p4;
        }

        CreateStartShape(p3, p4);
    }
    private void CreateStartShape(Vector2 p1, Vector2 p2)
    {
        var p3 = _currentVector.normalized * _startLength + p1;
        var p4 = _currentVector.normalized * _startLength + p2;
        CreateShape(p1, p2, p3, p4, 1 - _startLengthPercent, 1 - _startLengthPercent, 1, 1);
    }
    private void CreateShape(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float uv0, float uv1, float uv2, float uv3)
    {
        _vertices.AddRange(new List<Vector3>() { p1, p2, p3, p4 });
        _triangles.AddRange(new List<int>() { _vertices.Count - 4, _vertices.Count - 3, _vertices.Count - 2, _vertices.Count - 2, _vertices.Count - 3, _vertices.Count - 1 });
        _uv.AddRange(new List<Vector2>() { new Vector2(uv0, 0), new Vector2(uv1, 1), new Vector2(uv2, 0), new Vector2(uv3, 1) });
    }
    private void UpdateMesh()
    {
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.uv = _uv.ToArray();
        _mesh.RecalculateNormals();
    }
    private Vector3 FindClosestPoint(Vector3 from)
    {
        Vector3 to = new Vector3(9999, 9999);
        foreach (var point in _pathPoints)
            if (Vector2.Distance(from, point.transform.position) < Vector2.Distance(from, to) && !_pathPositions.Contains(point.transform.position))
                to = point.transform.position;
        return to;
    }
    private Vector2 DirectionFromAngle(float angleInDegrees)
    {
        return new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }
    private void TangensMethod()
    {
        //var katetOld = katetAdd;
        //katetAdd = (float)Math.Tan(Vector2.SignedAngle(_currentVector, nextVector) * Mathf.Deg2Rad / 2) * _width;
        //p3 = p1 + _currentVector + _currentVector.normalized * (katetAdd + katetOld) / 2;
        //p4 = p2 + _currentVector - _currentVector.normalized * (katetAdd + katetOld) / 2;
    }
}
