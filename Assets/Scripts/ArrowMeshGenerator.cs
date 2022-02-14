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
    private List<Vector2> _pathPositons;
    private List<float> _pathDistances;
    private float _halfWidth;
    private float _endLengthProcent;
    private float _startLengthProcent;
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
        _endLengthProcent = 0.1f;
        _startLengthProcent = 0.23f;
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
        _pathPositons = new List<Vector2>();
        _pathDistances = new List<float>();
        _allDistance = 0;
        for (int i = 0; i < _pathPoints.Count; i++)
        {
            _pathPositons.Add(_pathPoints[i].transform.position);
            if (i > 0)
            {
                _allDistance += Vector2.Distance(_pathPositons[i], _pathPositons[i - 1]);                
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
        Vector2 vector = _pathPositons[1] - _pathPositons[0];
        var perpendicular = Vector2.Perpendicular(vector);
        var p1 = -perpendicular.normalized * _halfWidth + _pathPositons[0];
        var p2 = perpendicular.normalized * _halfWidth + _pathPositons[0];
        var p3 = vector.normalized * _endLength + p1;
        var p4 = vector.normalized * _endLength + p2;
        CreateShape(p1, p2, p3, p4, 0, 0, _endLengthProcent, _endLengthProcent);
        CreateMiddleShape(p3, p4);
    }
    private void CreateMiddleShape(Vector2 p1, Vector2 p2)
    {
        Vector2 p3 = Vector2.zero;
        Vector2 p4 = Vector2.zero;
        //float katetAdd = 0;
        for (int i = 1; i <= _currentTargetPositionIndex; i++)
        {
            _currentVector = _pathPositons[i] - _pathPositons[i - 1];
            var distancePercent = (_currentDistance - _pathDistances[i - 1]) / (_pathDistances[i] - _pathDistances[i - 1]);
            Vector2 direction = Vector2.zero;
            float halfGipotenuza = 0; 
            float katetAdd = 0; 
            var nextVector = Vector2.zero;
            if (distancePercent >= 1 && i + 1 != _pathPositons.Count)
            {
                nextVector = _pathPositons[i + 1] - _pathPositons[i];
                var angleToCurrentVector = Vector2.SignedAngle(Vector2.right, _currentVector);
                var halfAngleFromCuurrentToNextVector = Vector2.SignedAngle(_currentVector, nextVector) / 2;
                direction = Vector2.Perpendicular(DirectionFromAngle(halfAngleFromCuurrentToNextVector + angleToCurrentVector));
                halfGipotenuza = _width / (float)(2 * Math.Cos(halfAngleFromCuurrentToNextVector * Mathf.Deg2Rad));
                p3 = _pathPositons[i] - direction * halfGipotenuza;
                p4 = _pathPositons[i] + direction * halfGipotenuza;
            }
            else
            {
                var katetOld = katetAdd;
                katetAdd = (float)Math.Tan(Vector2.SignedAngle(_currentVector, nextVector) * Mathf.Deg2Rad / 2) * _width;
                //p3 = p1 + _currentVector + _currentVector.normalized * (katetAdd + katetOld) / 2;
                //p4 = p2 + _currentVector - _currentVector.normalized * (katetAdd + katetOld) / 2;
                var perpendicular = Vector2.Perpendicular(_currentVector.normalized);
                p3 = _pathPositons[i - 1] - perpendicular * _halfWidth + (_currentVector - _currentVector.normalized * _endLength) * distancePercent;
                p4 = _pathPositons[i - 1] + perpendicular * _halfWidth + (_currentVector - _currentVector.normalized * _endLength) * distancePercent;
            }
            CreateShape(p1, p2, p3, p4, _endLengthProcent, _endLengthProcent, 1 - _startLengthProcent, 1 - _startLengthProcent);
            p1 = p3;
            p2 = p4;
        }

        CreateStartShape(p3, p4);
    }
    private void CreateStartShape(Vector2 p1, Vector2 p2)
    {
        var p3 = _currentVector.normalized * _startLength + p1;
        var p4 = _currentVector.normalized * _startLength + p2;
        CreateShape(p1, p2, p3, p4, 1 - _startLengthProcent, 1 - _startLengthProcent, 1, 1);
    }
    private void CreateShape(Vector2 p1, Vector2 p2, Vector3 p3, Vector3 p4, float uv0, float uv1, float uv2, float uv3)
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
            if (Vector2.Distance(from, point.transform.position) < Vector2.Distance(from, to) && !_pathPositons.Contains(point.transform.position))
                to = point.transform.position;
        return to;
    }
    private Vector2 DirectionFromAngle(float angleInDegrees)
    {
        return new Vector2(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad));
    }
}
