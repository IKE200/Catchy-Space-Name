using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Waypoint))]
public class WaypointEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        Waypoint waypoint = (Waypoint)target;
        Vector3 waypointPos = waypoint.transform.position;
        Quaternion rot;

        if (Tools.pivotRotation == PivotRotation.Global)
        {
            rot = Quaternion.identity;
        }
        else
        {
            rot = waypoint.transform.rotation;
        }

        if (Tools.current == Tool.Move)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newIncTangentAbs = Handles.PositionHandle(waypoint.GetIncTangAbs(), rot);
            Vector3 newOutTangentAbs = waypointPos + (waypointPos - newIncTangentAbs).normalized * waypoint.outTangent.magnitude;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypoint, "Change Inc Tangent Pos.");
                waypoint.incTangent = newIncTangentAbs - waypointPos;
                waypoint.outTangent = newOutTangentAbs - waypointPos;
            }

            EditorGUI.BeginChangeCheck();
            newOutTangentAbs = Handles.PositionHandle(waypoint.GetOutTangAbs(), rot);
            newIncTangentAbs = waypointPos + (waypointPos - newOutTangentAbs).normalized * waypoint.incTangent.magnitude;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypoint, "Change Out Tangent Pos.");
                waypoint.incTangent = newIncTangentAbs - waypointPos;
                waypoint.outTangent = newOutTangentAbs - waypointPos;
            }
        }
        else if (Tools.current == Tool.Scale)
        {
            EditorGUI.BeginChangeCheck();
            float newScaleInc = Handles.ScaleSlider(waypoint.incTangent.magnitude, waypoint.GetIncTangAbs(), waypoint.incTangent.normalized, waypoint.transform.rotation, HandleUtility.GetHandleSize(waypoint.GetIncTangAbs()), waypoint.incTangent.magnitude / 10);
            newScaleInc = (newScaleInc - waypoint.incTangent.magnitude) / 10 + waypoint.incTangent.magnitude;
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypoint, "Change Scale of incoming controlpoint.");
                waypoint.incTangent = waypoint.incTangent.normalized * newScaleInc;
            }

            EditorGUI.BeginChangeCheck();
            float newScaleOut = Handles.ScaleSlider(waypoint.outTangent.magnitude, waypoint.GetOutTangAbs(), waypoint.outTangent.normalized, waypoint.transform.rotation, HandleUtility.GetHandleSize(waypoint.GetOutTangAbs()), waypoint.outTangent.magnitude / 10);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(waypoint, "Change Scale of incoming controlpoint.");
                waypoint.outTangent = waypoint.outTangent.normalized * newScaleOut;
            }
        }

    }
}
