using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;

public class PhysicCable : MonoBehaviour
{
    [Header("Look")] [SerializeField, Min(1), OnValueChanged("OnNumberOfPointsChanged")]
    private int numberOfPoints = 3;

    [SerializeField, Min(0.01f)] private float size = 0.3f;

    [Header("Bahaviour")] [SerializeField, Min(1f), OnValueChanged("OnSpringForceChanged")]
    private float springForce = 200;
    
    [SerializeField, Min(0), OnValueChanged("OnSpringForceChanged")]
    private float springDamper = 1;

    [Header("Object to set")] [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private CableConnector start;

    [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private CableConnector end;

    [FormerlySerializedAs("cableElement")] [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private GameObject cablePoint;

    [SerializeField, Required, OnValueChanged("UpdatePoints")]
    private GameObject cableConnector;

    private float _distanceBetweenPoints;

    [SerializeField] private List<Transform> points;
    [SerializeField] private List<Transform> connectors;

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
            joint.damper = springDamper;
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
        var endPosition = end.CablePoint.transform.position;
        var startPosition = start.CablePoint.transform.position;
        var distanceStartEnd = (startPosition - endPosition).magnitude;
        _distanceBetweenPoints = distanceStartEnd / (numberOfPoints + 1);
    }

    [Button("Reset points")]
    private void UpdatePoints()
    {
        if (!start || !end || !cablePoint)
        {
            Debug.LogWarning("Can't update because one of objects to set is null!");
            return;
        }
        
        points.Clear();
        points.Add(start.CablePoint.transform);

        connectors.Clear();

        CalculateDistanceBetweenPoints();


        // delete old
        int length = transform.childCount;
        for (int i = length - 1; i >= 0; i--)
            if (transform.GetChild(i).name.StartsWith(CloneText))
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }

        // set new
        Vector3 lastPos = start.CablePoint.transform.position;
        Rigidbody lastBody = start.gameObject.GetOrCreateComponent<Rigidbody>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            // Apply the position to the new point
            GameObject cPoint = CreateNewPoint(i);
            GameObject cConnector = CreateNewCon(i);

            // Get the next position for the based on the last position
            Vector3 newPos = GenerateNextPointPosition(lastPos);
            cPoint.transform.position = newPos;
            cPoint.transform.localScale = Vector3.one * size;
            cPoint.transform.rotation = transform.rotation;

            SetSpring(cPoint.GetOrCreateComponent<SpringJoint>(), lastBody, i == 0 ? start.CablePointOffset : Vector3.zero);

            lastBody = cPoint.GetOrCreateComponent<Rigidbody>();
            
            cConnector.transform.position = GenerateConnectorPosition(lastPos, newPos);
            cConnector.transform.localScale = GenerateConnectorSize(lastPos, newPos);
            cConnector.transform.rotation = GenerateConnectorRotation(lastPos, newPos);
            
            lastPos = newPos;

            points.Add(cPoint.transform);
            connectors.Add(cConnector.transform);
        }

        points.Add(end.CablePoint.transform);

        Vector3 endPos = GenerateNextPointPosition(lastPos);
        end.transform.position = endPos - end.CablePointOffset;
        SetSpring(lastBody.gameObject.AddComponent<SpringJoint>(), end.gameObject.GetOrCreateComponent<Rigidbody>(),  start.CablePointOffset);
        
        GameObject endConnector = CreateNewCon(numberOfPoints);
        endConnector.transform.position = GenerateConnectorPosition(lastPos, endPos);
        endConnector.transform.rotation = GenerateConnectorRotation(lastPos, endPos);
        endConnector.transform.localScale = GenerateConnectorSize(lastPos, endPos);
        connectors.Add(endConnector.transform);
    }

    private Vector3 GenerateNextPointPosition(Vector3 lastPos) =>
        lastPos + start.transform.forward * -1 * _distanceBetweenPoints;

    private Vector3 GenerateConnectorPosition(Vector3 start, Vector3 end) => (start + end) / 2f;

    private Vector3 GenerateConnectorSize(Vector3 start, Vector3 end) => new(size, size, (start - end).magnitude / 2f);

    private Quaternion GenerateConnectorRotation(Vector3 start, Vector3 end) =>
        Quaternion.LookRotation(end - start, Vector3.right);

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
        Transform lastPoint = points[0];
        for (int i = 0; i < connectors.Count; i++)
        {
            Transform nextPoint = points[i + 1];
            Transform connector = connectors[i].transform;
            connector.position = GenerateConnectorPosition(lastPoint.position, nextPoint.position);
            if (lastPoint.position == nextPoint.position || nextPoint.position == connector.position)
            {
                connector.localScale = Vector3.zero;
            }
            else
            {
                connector.rotation = Quaternion.LookRotation(nextPoint.position - connector.position);
                connector.localScale = GenerateConnectorSize(lastPoint.position, nextPoint.position);
            }

            lastPoint = nextPoint;
        }
    }

    // Calculates the half way point between the start and end point
    private Vector3 GetHalfWayLocation(Vector3 start, Vector3 end) => (start + end) / 2f;

    // Calculates the half distance between two points
    private Vector3 GetSizeBetween(Vector3 start, Vector3 end) => new Vector3(size, size, (start - end).magnitude / 2f);
    
    private string PointName(int index) => $"{CloneText}_{index}_Point";
    private string ConnectorName(int index) => $"{CloneText}_{index}_Conn";

    private Transform GetPoint(int index) => transform.Find(PointName(index));


    public void SetSpring(SpringJoint spring, Rigidbody connectedBody, Vector3 connectedAnchor = new())
    {
        spring.connectedBody = connectedBody;
        spring.spring = springForce;
        spring.damper = springDamper;
        spring.autoConfigureConnectedAnchor = false;
        spring.anchor = Vector3.zero;
        spring.connectedAnchor = connectedAnchor;
        spring.minDistance = _distanceBetweenPoints;
        spring.maxDistance = _distanceBetweenPoints;
    }

    private GameObject CreateNewPoint(int index)
    {
        GameObject temp = Instantiate(cablePoint, transform);
        temp.name = PointName(index);
        return temp;
    }

    private GameObject CreateNewCon(int index)
    {
        GameObject temp = Instantiate(cableConnector, transform);
        temp.name = ConnectorName(index);
        return temp;
    }
}