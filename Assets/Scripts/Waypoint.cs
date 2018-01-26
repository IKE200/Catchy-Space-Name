using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

    [HideInInspector]
    public bool hideGizmo = false;
    public Vector3 incTangent;
    public Vector3 outTangent;

    private float gizmoSize = 3;
    private Color waypointColor = Color.blue;
    private Color tangentColor = Color.green;

    public void SetGizmoSize(float size)
    {
        gizmoSize = size;
    }

    public void SetWaypointColor(Color col)
    {
        waypointColor = col;
    }

    public void SetTangentColor(Color col)
    {
        tangentColor = col;
    }

    public Vector3 GetIncTangAbs()
    {
        return (incTangent + transform.position);
    }

    public Vector3 GetOutTangAbs()
    {
        return (outTangent + transform.position);
    }

    void OnDrawGizmos()
    {
        if (!hideGizmo)
        {
            Gizmos.color = waypointColor;
            Gizmos.DrawSphere(transform.position, gizmoSize);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!hideGizmo)
        {
            Gizmos.color = tangentColor;
            Gizmos.DrawSphere(GetIncTangAbs(), gizmoSize);
            Gizmos.DrawSphere(GetOutTangAbs(), gizmoSize);
            Gizmos.DrawLine(transform.position, GetIncTangAbs());
            Gizmos.DrawLine(transform.position, GetOutTangAbs());
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
