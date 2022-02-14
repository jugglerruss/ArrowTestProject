using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FreeMoveHandleExample : MonoBehaviour
{
    public Vector3 targetPosition { get { return m_TargetPosition; } set { m_TargetPosition = value; } }
    [SerializeField]
    private Vector3 m_TargetPosition = new Vector3(0f, 0f, 0f);
    private Vector3 old_TargetPosition = new Vector3(0f, 0f, 0f);
    public Action OnPathPointMove;
    public virtual void Update()
    {
        if (old_TargetPosition == m_TargetPosition)
            return;
        old_TargetPosition = m_TargetPosition;
        OnPathPointMove?.Invoke();
        transform.position = targetPosition;
    }
}