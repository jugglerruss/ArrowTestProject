using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public sealed class FreeMoveHandleExample : MonoBehaviour
{
    public Vector3 TargetPosition { get => _mTargetPosition; set => _mTargetPosition = value; }
    [SerializeField]
    private Vector3 _mTargetPosition = new Vector3(0f, 0f, 0f);
    private Vector3 _oldTargetPosition = new Vector3(0f, 0f, 0f);
    public Action OnPathPointMove;
    public void Update()
    {
        if (_oldTargetPosition == _mTargetPosition)
            return;
        _oldTargetPosition = _mTargetPosition;
        OnPathPointMove?.Invoke();
        transform.position = TargetPosition;
    }
}