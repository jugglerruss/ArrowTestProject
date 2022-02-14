using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PathPointSpawner))]
    public sealed class FreeMoveHandleExampleEditor : UnityEditor.Editor
    {
        private PathPointSpawner _pointsSpawner;
        private SelectionInfo _selectionInfo;
        private bool _needsRepaint;
        private void OnEnable()
        {
            _pointsSpawner = target as PathPointSpawner;
            _selectionInfo = new SelectionInfo();
        }

        private void OnSceneGUI()
        {
            Event guiEvent = Event.current;
            if(guiEvent.type == EventType.Repaint)
            {
                Draw();
            }
            else if(guiEvent.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }
            else
            {
                HandleInput(guiEvent);
                if (_needsRepaint)
                    HandleUtility.Repaint();
            }

        }
        private void UpdateMouseOverSelection(Vector3 mousePosition)
        {
            int mouseOverPointIndex = -1;
            for (int i = 0; i < _pointsSpawner.PathPoints.Count; i++)
            {
                if(Vector3.Distance(mousePosition, _pointsSpawner.PathPoints[i].transform.position) < _pointsSpawner._handleRadius)
                {
                    mouseOverPointIndex = i;
                    break;
                }
            }
            if(mouseOverPointIndex != _selectionInfo.PointIndex)
            {
                _selectionInfo.PointIndex = mouseOverPointIndex;
                _selectionInfo.MouseIsOverPoint = mouseOverPointIndex != -1;
                _needsRepaint = true;
            }
        }
        private void HandleInput(Event guiEvent)
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
            float dstToDraw = -mouseRay.origin.z / mouseRay.direction.z;
            var position = mouseRay.GetPoint(dstToDraw);
            if(guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            {
                if (guiEvent.type == EventType.MouseDown)
                    HandleLeftMouseDown(position);
                else if (guiEvent.type == EventType.MouseUp)
                    HandleLeftMouseUp(position);
                else if (guiEvent.type == EventType.MouseDrag)
                    HandleLeftMouseDrag(position);
            }
            if( !_selectionInfo.PointIsSelected )
                UpdateMouseOverSelection(position);
        }
        private void HandleLeftMouseDown(Vector3 mousePosition) 
        {
            if( !_selectionInfo.MouseIsOverPoint)
            {
                Undo.RecordObject(_pointsSpawner, "Add point");
                _pointsSpawner.InstantiatePoint(mousePosition);
                _selectionInfo.PointIndex = _pointsSpawner.PathPoints.Count - 1;
            }
            _selectionInfo.PointIsSelected = true;
            _needsRepaint = true;
        }
        private void HandleLeftMouseUp(Vector3 mousePosition) 
        {
            if (!_selectionInfo.PointIsSelected)
                return;
            _selectionInfo.PointIsSelected = false;
            _selectionInfo.PointIndex = -1;
            _needsRepaint = true;
        }
        private void HandleLeftMouseDrag(Vector3 mousePosition)
        {
            if (!_selectionInfo.PointIsSelected)
                return;
            _pointsSpawner.PathPoints[_selectionInfo.PointIndex].SetPosition(mousePosition);
            _needsRepaint = true;
        }
        private void Draw()
        {
            for (int i = 0; i < _pointsSpawner.PathPoints.Count; i++)
            {
                Vector2 nextPoint = _pointsSpawner.PathPoints[(i + 1) % _pointsSpawner.PathPoints.Count].transform.position;
                if( i+1 != _pointsSpawner.PathPoints.Count)
                {
                    Handles.color = Color.black;
                    Handles.DrawDottedLine(_pointsSpawner.PathPoints[i].transform.position, nextPoint, 4);
                }
                if( i == _selectionInfo.PointIndex)
                    Handles.color = _selectionInfo.PointIsSelected ? Color.black : Color.red;
                else
                    Handles.color = Color.white;
                Handles.DrawSolidDisc(_pointsSpawner.PathPoints[i].transform.position, Vector3.forward, _pointsSpawner._handleRadius);
            }
            _needsRepaint = false;
        }
        class SelectionInfo
        {
            public int PointIndex = -1;
            public bool MouseIsOverPoint;
            public bool PointIsSelected;
        }
    }
}