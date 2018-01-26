using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointSystem : MonoBehaviour {

    public int smoothness = 10;
    public bool circle = false;
    public bool hideInSceneView = false;
    public Color trackColor = Color.yellow;
    public Color waypointColor = Color.red;
    public Color tangentColor = Color.blue;
    public float gizmoSize = 4;
    public float speed;
    public int turnOffset;

    [HideInInspector]
    public bool initiated = false;

    private Vector3 location;
    private Waypoint[] wayPoints;
    private Vector3[] wayPointsPos;
    private Vector3[] p1;
    private Vector3[] p2;
    private Vector3[] lineSegments = new Vector3[0];
    private int locationPointer;

    public void Initiate()
    {
        ComputeInitialPath();
        initiated = true;
    }

    public Vector3[] GetP1()
    {
        return p1;
    }

    public Vector3[] GetP2()
    {
        return p2;
    }

    void OnDrawGizmos()
    {
        if (!hideInSceneView && initiated)
        {
            FindChildren();
            CalculateLineSegments();
            Gizmos.color = trackColor;
            for (int i = 0; i < lineSegments.Length - 1; i++)
            {
                Gizmos.DrawLine(lineSegments[i], lineSegments[i + 1]);
            }
            foreach (Waypoint waypoint in wayPoints)
            {
                waypoint.hideGizmo = false;
            }
        }
        else if (hideInSceneView && initiated)
        {
            foreach (Waypoint waypoint in wayPoints)
            {
                waypoint.hideGizmo = true;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update () {
	}

    public void Initialise()
    {
        locationPointer = 1;
        FindChildren();
        CalculateLineSegments();
        location = wayPointsPos[0];
    }

    private void CalculatePath()
    {
        float[] x = new float[wayPoints.Length];
        float[] y = new float[wayPoints.Length];
        float[] z = new float[wayPoints.Length];

        for (int i = 0; i < wayPoints.Length; i++)
        {
            x[i] = wayPointsPos[i][0];
            y[i] = wayPointsPos[i][1];
            z[i] = wayPointsPos[i][2];
        }

        p1 = new Vector3[wayPoints.Length - 1];
        p2 = new Vector3[wayPoints.Length - 1];

        computeControlPoints(x, 0);
        computeControlPoints(y, 1);
        computeControlPoints(z, 2);
    }

    private void CalculateLineSegments()
    {
        UpdateWaypoints();
        if (circle)
        {
            lineSegments = new Vector3[smoothness * (wayPoints.Length) + 1];
        }
        else
        {
            lineSegments = new Vector3[smoothness * (wayPoints.Length - 1) + 1];
        }
        
        for (int i = 0; i < wayPoints.Length - 1; i++)
        {
            Vector3 cp0 = wayPointsPos[i];
            Vector3 cp1 = wayPoints[i].GetOutTangAbs();
            Vector3 cp2 = wayPoints[i + 1].GetIncTangAbs();
            Vector3 cp3 = wayPointsPos[i + 1];
            for (int t = 0; t < smoothness; t++)
            {
                Vector3 nextPoint = BezierCurvePoint(t / (float)smoothness, cp0, cp1, cp2, cp3);
                lineSegments[i * smoothness + t] = nextPoint;
            }
        }

        if (circle)
        {
            int n = wayPointsPos.Length - 1;
            Vector3 cp0 = wayPointsPos[n];
            Vector3 cp1 = wayPoints[n].GetOutTangAbs();
            Vector3 cp2 = wayPoints[0].GetIncTangAbs();
            Vector3 cp3 = wayPointsPos[0];
            for (int t = 0; t <= smoothness; t++)
            {
                Vector3 nextPoint = BezierCurvePoint(t / (float)smoothness, cp0, cp1, cp2, cp3);
                lineSegments[n * smoothness + t] = nextPoint;
            }
        }
        else
        {
            lineSegments[lineSegments.Length - 1] = wayPointsPos[wayPointsPos.Length - 1];
        }
    }

    private void FindChildren()
    {
        int k = 0;
        wayPoints = new Waypoint[transform.childCount];
        wayPointsPos = new Vector3[transform.childCount];
        foreach (Transform child in transform)
        {
            Waypoint waypoint = child.GetComponent<Waypoint>();
            wayPoints[k] = waypoint;
            if (!waypoint)
            {
                wayPoints[k] = child.gameObject.AddComponent<Waypoint>();
            }
            wayPointsPos[k] = child.position;
            k++;
        }
    }

    private void UpdateWaypoints()
    {
        for (int i = 0; i < wayPoints.Length; i++)
        {
            wayPointsPos[i] = wayPoints[i].transform.position;
            wayPoints[i].SetGizmoSize(gizmoSize);
            wayPoints[i].SetWaypointColor(waypointColor);
            wayPoints[i].SetTangentColor(tangentColor);
            wayPoints[i].transform.LookAt(wayPoints[i].outTangent + wayPointsPos[i]);
        }
    }

    private void computeControlPoints(float[] K, int dim)
    {
        int n = K.Length - 1;
        float[] cp1 = new float[n];
        float[] cp2 = new float[n];

        /*rhs vector*/
        float[] a = new float[n];
        float[] b = new float[n];
        float[] c = new float[n];
        float[] r = new float[n];

        /*left most segment*/
        a[0] = 0;
        b[0] = 2;
        c[0] = 1;
        r[0] = K[0] + 2 * K[1];

        /*internal segments*/
        for (int i = 1; i < n - 1; i++)
        {
            a[i] = 1;
            b[i] = 4;
            c[i] = 1;
            r[i] = 4 * K[i] + 2 * K[i + 1];
        }

        /*right segment*/
        a[n - 1] = 2;
        b[n - 1] = 7;
        c[n - 1] = 0;
        r[n - 1] = 8 * K[n - 1] + K[n];

        /*solves Ax=b with the Thomas algorithm (from Wikipedia)*/
        for (int i = 1; i < n; i++)
        {
            float m = a[i] / b[i - 1];
            b[i] = b[i] - m * c[i - 1];
            r[i] = r[i] - m * r[i - 1];
        }

        cp1[n - 1] = r[n - 1] / b[n - 1];
        for (int i = n - 2; i >= 0; --i)
        {
            cp1[i] = (r[i] - c[i] * cp1[i + 1]) / b[i];
        }

        /*we have p1, now compute p2*/
        for (int i = 0; i < n - 1; i++)
        {
            cp2[i] = 2 * K[i + 1] - cp1[i + 1];
        }
        cp2[n - 1] = 0.5f * (K[n] + cp1[n - 1]);

        for (int i = 0; i < n; i++)
        {
            p1[i][dim] = cp1[i];
            p2[i][dim] = cp2[i];
        }
    }

    private Vector3 BezierCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = (1.0f - t);
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;
        Vector3 vector = (uuu * p0);
        vector += (3 * uu * t * p1);
        vector += (3 * u * tt * p2);
        vector += (ttt * p3);
        return vector;
    }

    public Vector3 UpdatePositionBaseOnSpeed(float speed)
    {
        float distToTravel = speed * Time.deltaTime;
        float distToNextPoint = (lineSegments[locationPointer] - location).magnitude;
        while (distToTravel >= distToNextPoint)
        {
            distToTravel -= distToNextPoint;
            location = lineSegments[locationPointer];
            locationPointer++;
            if (locationPointer >= lineSegments.Length)
            {
                if (circle)
                {
                    locationPointer = 1;
                    distToNextPoint = (lineSegments[locationPointer] - location).magnitude;
                }
                else
                {
                    distToTravel = 0;
                }
            }
            else
            {
                distToNextPoint = (lineSegments[locationPointer] - location).magnitude;
            }

        }
        if (distToTravel > 0)
        {
            location += distToTravel * (lineSegments[locationPointer] - location).normalized;
        }
        return location;
    }

    public void UpdateRotation(Transform trans)
    {
        int lookPointer = locationPointer + turnOffset;
        if (lookPointer < lineSegments.Length)
        {
            trans.LookAt(lineSegments[lookPointer]);
        }
        else if (circle)
        {
            lookPointer -= lineSegments.Length - 1;
            trans.LookAt(lineSegments[lookPointer]);
        }
    }

    public void RenameChildren()
    {
        int k = 0;
        foreach (Transform child in transform)
        {
            child.gameObject.name = "Waypoint " + k;
            k++;
        }
    }

    public void ComputeInitialPath()
    {
        FindChildren();
        int n = wayPoints.Length - 1;
        CalculatePath();

        Waypoint w = wayPoints[0];
        w.outTangent = p1[0] - wayPointsPos[0];
        w.incTangent = wayPointsPos[0] + (wayPointsPos[0] - p1[0]) - wayPointsPos[0];

        for (int i = 1; i < n; i++)
        {
            Waypoint wp = wayPoints[i];
            wp.incTangent = p2[i - 1] - wayPointsPos[i];
            wp.outTangent = p1[i] - wayPointsPos[i];
        }

        w = wayPoints[n];
        w.incTangent = p2[n - 1] - wayPointsPos[n];
        w.outTangent = wayPointsPos[n] + (wayPointsPos[n] - p2[n - 1]) - wayPointsPos[n];
    }

}

