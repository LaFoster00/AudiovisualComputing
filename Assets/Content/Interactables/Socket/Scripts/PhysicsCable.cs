using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

[RequireComponent(typeof(LineRenderer))]
public class PhysicCable : MonoBehaviour
{
    [Header("Look")] [SerializeField, Min(1), OnValueChanged("OnNumberOfPointsChanged")]
    private int numberOfPoints = 3;

    [SerializeField, Min(0.01f)] private float size = 0.3f;

    [Header("Bahaviour")] [SerializeField, Min(1f), OnValueChanged("OnSpringForceChanged")]
    private float springForce = 200;

    [Header("Object to set")] [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private CableConnector start;

    [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private CableConnector end;

    [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private GameObject cableElement;

    private float _distanceBetweenPoints;

    [SerializeField] private List<Transform> points;

    [SerializeField, Required] private LineRenderer lineRenderer;

    [SerializeField, OnValueChanged("ConnectionCountChanged")]
    private byte connections;

    private byte Connections
    {
        get => connections;
        set
        {
            if (connections == value)
                return;
            connections = value;
            OnConnectionsChanged();
            if (Connected != (Connections > 0))
            {
                Connected = Connections > 0;
            }
        }
    }

    private bool _connected = false;

    public bool Connected
    {
        get => _connected;
        set
        {
            if (_connected == value)
                return;
            _connected = value;
            OnConnectedChanged();
        }
    }

    private const string CloneText = "ChainPart";

    public IReadOnlyList<Transform> Points => points;

    public void AddConnection(int count)
    {
        Connections += (byte)count;
    }

    private void OnNumberOfPointsChanged()
    {
        CalculateDistanceBetweenPoints();
        UpdatePoints();
    }

    private void OnSpringForceChanged()
    {
        foreach (var joint in GetComponentsInChildren<SpringJoint>())
        {
            joint.spring = springForce;
        }
    }

    private void OnConnectedChanged()
    {
    }

    private void OnConnectionsChanged()
    {
        if (connections != 2) return;

        CalculateDistanceBetweenPoints();
        foreach (var springJoint in GetComponentsInChildren<SpringJoint>())
        {
            SetSpring(springJoint, springJoint.connectedBody);
        }
    }

    private void CalculateDistanceBetweenPoints()
    {
        var endPosition = end.cableAnchor.transform.position;
        var startPosition = start.cableAnchor.transform.position;
        var distanceStartEnd = (startPosition - endPosition).magnitude;
        _distanceBetweenPoints = distanceStartEnd / (numberOfPoints + 1);
    }

    private void UpdateLineRenderer()
    {
        Vector3[] positions = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            positions[i] = points[i].position;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    [Button("Reset points")]
    private void UpdatePoints()
    {
        if (!start || !end || !cableElement)
        {
            Debug.LogWarning("Can't update because one of objects to set is null!");
            return;
        }

        points.Clear();
        points.Add(start.cableAnchor.transform);


        CalculateDistanceBetweenPoints();


        // delete old
        int length = transform.childCount;
        for (int i = length - 1; i >= 0; i--)
            if (transform.GetChild(i).name.StartsWith(CloneText))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

        // set new
        Vector3 lastPos = start.cableAnchor.transform.position;
        Rigidbody lastBody = start.gameObject.GetOrCreateComponent<Rigidbody>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            // Get the next position for the based on the last position
            Vector3 newPos = GenerateNextPointPosition(lastPos, i == 0 ? 0.5f : 1.0f);

            // Apply the position to the new point
            GameObject cPoint = CreateNewPoint(i);
            cPoint.transform.position = newPos;
            cPoint.transform.localScale = new Vector3(size, size, _distanceBetweenPoints);
            cPoint.transform.rotation = transform.rotation;

            SetSpring(cPoint.GetOrCreateComponent<SpringJoint>(), lastBody, i == 0 ? start.cableAnchor.transform.position : new Vector3());

            lastBody = cPoint.GetOrCreateComponent<Rigidbody>();
            lastPos = newPos;

            points.Add(cPoint.transform);
        }

        points.Add(end.cableAnchor.transform);

        end.transform.position = GenerateNextPointPosition(lastPos, 0.5f) + end.transform.position -
                                 end.cableAnchor.transform.position;
        end.transform.LookAt(end.transform.position + end.transform.position - start.cableAnchor.transform.position,
            Vector3.up);
        SetSpring(lastBody.gameObject.AddComponent<SpringJoint>(), end.gameObject.GetOrCreateComponent<Rigidbody>(),
            end.cableAnchor.transform.localPosition);

        Vector3 GenerateNextPointPosition(Vector3 lastPos, float scalar = 1.0f) =>
            lastPos + start.cableAnchor.transform.forward * -1 * scalar * _distanceBetweenPoints;

        UpdateLineRenderer();
    }

    [Button("Add point")]
    private void AddPoint()
    {
        Transform lastPrevPoint = GetPoint(numberOfPoints - 1);
        if (lastPrevPoint == null)
        {
            Debug.LogWarning("Cant find point number " + (numberOfPoints - 1));
            return;
        }

        Rigidbody endRB = end.GetComponent<Rigidbody>();
        // Remove the link to the end handle of the cable
        foreach (var spring in lastPrevPoint.GetComponents<SpringJoint>())
            if (spring.connectedBody == endRB)
                DestroyImmediate(spring);

        // Create the new element in the chain
        GameObject cPoint = CreateNewPoint(numberOfPoints);

        cPoint.transform.position = end.transform.position;
        cPoint.transform.rotation = end.transform.rotation;
        cPoint.transform.localScale = Vector3.one * size;

        SetSpring(cPoint.GetComponent<SpringJoint>(), lastPrevPoint.GetComponent<Rigidbody>());
        SetSpring(cPoint.AddComponent<SpringJoint>(), endRB);

        // fix end
        end.transform.position += end.transform.forward * _distanceBetweenPoints;

        numberOfPoints++;
    }

    [Button("Remove point")]
    private void RemovePoint()
    {
        if (numberOfPoints < 2)
        {
            Debug.LogWarning("Cable can't have less then 1 element");
            return;
        }

        Transform lastPrevPoint = GetPoint(numberOfPoints - 1);
        if (lastPrevPoint == null)
        {
            Debug.LogWarning("Cant find point number " + (numberOfPoints - 1));
            return;
        }

        Transform lastLastPrevPoint = GetPoint(numberOfPoints - 2);
        if (lastLastPrevPoint == null)
        {
            Debug.LogWarning("Cant find point number " + (numberOfPoints - 2));
            return;
        }


        Rigidbody endRB = end.GetComponent<Rigidbody>();
        SetSpring(lastLastPrevPoint.gameObject.AddComponent<SpringJoint>(), endRB);

        end.transform.position = lastPrevPoint.position;
        end.transform.rotation = lastPrevPoint.rotation;

        DestroyImmediate(lastPrevPoint.gameObject);

        numberOfPoints--;
    }

    private void Update()
    {
        UpdateLineRenderer();
    }

    // Calculates the half way point between the start and end point
    private Vector3 GetHalfWayLocation(Vector3 start, Vector3 end) => (start + end) / 2f;

    // Calculates the half distance between two points
    private Vector3 GetSizeBetween(Vector3 start, Vector3 end) => new Vector3(size, size, (start - end).magnitude / 2f);

    private Quaternion CountRoationOfCon(Vector3 start, Vector3 end) =>
        Quaternion.LookRotation(end - start, Vector3.right);

    private string PointName(int index) => $"{CloneText}_{index}_Point";

    private Transform GetPoint(int index) => transform.Find(PointName(index));


    public void SetSpring(SpringJoint spring, Rigidbody connectedBody, Vector3 connectedAnchor = new())
    {
        spring.connectedBody = connectedBody;
        spring.spring = springForce;
        spring.damper = 0.2f;
        spring.autoConfigureConnectedAnchor = false;
        spring.anchor = Vector3.zero;
        spring.connectedAnchor = connectedAnchor;
        spring.minDistance = _distanceBetweenPoints;
        spring.maxDistance = _distanceBetweenPoints;
    }

    private GameObject CreateNewPoint(int index)
    {
        GameObject temp = Instantiate(cableElement, transform);
        temp.name = PointName(index);
        return temp;
    }
}