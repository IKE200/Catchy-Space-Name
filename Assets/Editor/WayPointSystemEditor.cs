using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WayPointSystem))]
public class WayPointSystemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WayPointSystem wayPointSystem = (WayPointSystem)target;
        if (GUILayout.Button("Assign numbered names"))
        {
            wayPointSystem.RenameChildren();
        }
        if (GUILayout.Button("Compute initial Path using all child objects"))
        {
            if (wayPointSystem.initiated && EditorUtility.DisplayDialog("Warning", "You already calculated the initial path, recalculating it will undo all custom tweaks you made. Do you want to continue?", "yes", "no"))
            {
                wayPointSystem.Initiate();
            }
            else if (!wayPointSystem.initiated)
            {
                wayPointSystem.Initiate();
            }
        }
    }
}