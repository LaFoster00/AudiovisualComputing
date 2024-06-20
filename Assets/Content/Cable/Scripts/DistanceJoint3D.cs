using UnityEngine;
using UnityEngine.Serialization;

public class DistanceJoint3D : MonoBehaviour {

    public Rigidbody connectedBody;
    public bool determineDistanceOnStart = true;
    public float distance;
    public float spring = 0.1f;
    public float damper = 5f;

    protected Rigidbody Rigidbody;

    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (determineDistanceOnStart && connectedBody != null)
            distance = Vector3.Distance(Rigidbody.position, connectedBody.position);
    }

    void FixedUpdate()
    {
        var connection = Rigidbody.position - connectedBody.position;
        var distanceDiscrepancy = distance - connection.magnitude;

        Rigidbody.position += distanceDiscrepancy * connection.normalized;

        var velocityTarget = connection + (Rigidbody.velocity + Physics.gravity * spring);
        var projectOnConnection = Vector3.Project(velocityTarget, connection);
        Rigidbody.velocity += (velocityTarget - projectOnConnection) / (1 + damper * Time.fixedDeltaTime);
    }
}