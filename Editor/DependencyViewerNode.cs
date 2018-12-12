using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class DependencyViewerNode
{
    public string additionalInfo;

    public enum NodeInputSide { Left, Right }

    public string Name
    {
        get
        {
            if (_targetObject == null)
            {
                return "(null)";
            }

            if (_targetObject is UnityEditor.MonoScript)
            {
                return string.Format("{0} (Script)", _targetObject.name);
            }

            return _targetObject.name;
        }
    }
    
    private UnityEngine.Object _targetObject;
    public UnityEngine.Object TargetObject
    {
        get { return _targetObject; }
        private set { _targetObject = value; }
    }


    private Vector2 _position;
    public Vector2 Position
    {
        get { return _position; }
        set { _position = value; }
    }

    private List<DependencyViewerNode> _leftInputs;
    public List<DependencyViewerNode> LeftInputs
    {
        get { return _leftInputs; }
        set { _leftInputs = value; }
    }

    private List<DependencyViewerNode> _rightInputs;
    public List<DependencyViewerNode> RightInputs
    {
        get { return _rightInputs; }
        set { _rightInputs = value; }
    }

    public DependencyViewerNode(UnityEngine.Object targetObject)
    {
        _targetObject = targetObject;
        _leftInputs = new List<DependencyViewerNode>();
        _rightInputs = new List<DependencyViewerNode>();
    }

    public float GetHeight()
    {
        return DependencyViewerGraphDrawer.NodeHeight * 2; // TODO : Remove *2 since it is just for debug info
    }

    public float GetWidth()
    {
        return DependencyViewerGraphDrawer.NodeWidth;
    }

    public Vector2 GetSize()
    {
        return new Vector2(GetWidth(), GetHeight());
    }

    public Vector2 GetLeftInputAnchorPosition()
    {
        return new Vector2(_position.x, _position.y + GetHeight() / 2);
    }

    public Vector2 GetRightInputAnchorPosition()
    {
        return new Vector2(_position.x + GetWidth(), _position.y + GetHeight() / 2);
    }

    public List<DependencyViewerNode> GetInputNodesFromSide(NodeInputSide side)
    {
        return side == NodeInputSide.Left ? _leftInputs : _rightInputs;
    }

    public void SetPositionX(float x)
    {
        _position.x = x;
    }

    public void SetPositionY(float y)
    {
        _position.y = y;
    }
}
