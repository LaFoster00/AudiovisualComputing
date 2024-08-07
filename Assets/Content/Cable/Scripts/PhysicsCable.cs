using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using JointType = UnityEngine.ConfigurableJoint;

namespace Cable
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    public class PhysicCable : MonoBehaviourGuid
    {
        [Header("Look")] [SerializeField, Min(1), OnValueChanged("UpdatePoints")]
        internal int numberOfPoints = 3;

        [SerializeField, Min(0.01f), OnValueChanged("UpdatePoints")]
        internal float size = 0.3f;

        [Header("Bahaviour")] [SerializeField, Min(1f), OnValueChanged("UpdateSpring")]
        internal float distanceTolerance = 200;

        [SerializeField, Range(0, 1), OnValueChanged("UpdateSpring")]
        internal float springDamper = 1;

        [Header("Object to set")] [SerializeField, Required]
        private Transform start;

        [SerializeField, Required] private Transform end;

        [SerializeField, ReadOnly] private List<Transform> elements = new();
        private List<JointType> joints = new();

        [SerializeField, Required] private Material lineMaterial;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private LineRenderer _lineRenderer;
        private bool _fallback;

        private List<Transform> points;
        private byte connections;

        private bool _connected = false;

        private byte Connections
        {
            get => connections;
            set
            {
                if (connections == value)
                    return;
                connections = value;
                if (Connected != (Connections > 0))
                {
                    Connected = Connections > 0;
                }
            }
        }

        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected == value)
                    return;
                UpdateSpring();
                _connected = value;
            }
        }

        public float DistanceBetweenPoints => Vector3.Distance(start.position, end.position) / (TotalElements - 1);

        private int TotalElements => numberOfPoints + 2;

        private void OnEnable()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.enabled = false;

            _mesh = new Mesh();
            _meshFilter.mesh = _mesh;
            if (!_meshRenderer.sharedMaterial.shader.isSupported)
            {
                _fallback = true;
                _meshRenderer.enabled = false;
                _lineRenderer.material = lineMaterial;
                _lineRenderer.enabled = true;
            }
            else
            {
                _fallback = false;
                _meshRenderer.enabled = true;
                _lineRenderer.enabled = false;
            }

            UpdatePoints();
        }

        private void Start()
        {
        }

        private void Update()
        {
            if (!_fallback)
                UpdateMesh();
            else
                UpdateLine();
            UpdateLimit();
        }

        void UpdateMesh()
        {
            // We use  pair of 3 vectors (triangle) to store the previous current and next position in the chain
            Vector3[] vertices = new Vector3[(TotalElements) * 3];

            // Marks the beginning of the cable
            vertices[0] = start.position + start.forward * 0.01f;
            vertices[1] = start.position;
            vertices[2] = elements[0].position;
            for (int element = 0; element < elements.Count; element++)
            {
                // 0: Previous 1: Current 3: Next
                if (element == 0)
                    vertices[3 + element * 3 + 0] = start.position;
                else
                    vertices[3 + element * 3 + 0] = elements[element - 1].position;

                vertices[3 + element * 3 + 1] = elements[element].position;

                if (element == elements.Count - 1)
                    vertices[3 + element * 3 + 2] = end.position;
                else
                    vertices[3 + element * 3 + 2] = elements[element + 1].position;
            }

            vertices[^3] = elements[^1].position;
            vertices[^2] = end.position;
            vertices[^1] = end.position + end.forward * 0.01f;

            // Transform the points into local space so that the frustrum culling works as expected
            // This need to be reversed in the shader by transforming the points back to world space
            transform.InverseTransformPoints(vertices);

            int[] indices = new int[vertices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            var bounds = _mesh.bounds;
            bounds.size += new Vector3(0.1f, 0.1f, 0.1f);
            _mesh.bounds = bounds;

            _lineRenderer.positionCount = 0;
        }

        private void UpdateLine()
        {
            // We use  pair of 3 vectors (triangle) to store the previous current and next position in the chain
            Vector3[] vertices = new Vector3[TotalElements + 2];

            // Marks the beginning of the cable
            vertices[0] = start.position + start.forward * 0.01f;
            vertices[1] = start.position;
            for (int element = 0; element < elements.Count; element++)
            {
                vertices[2 + element] = elements[element].position;
            }

            vertices[^2] = end.position;
            vertices[^1] = end.position + end.forward * 0.01f;

            _lineRenderer.positionCount = vertices.Length;
            _lineRenderer.SetPositions(vertices);

            _mesh.Clear();
            _mesh.vertices = new[] { Vector3.zero };
            _mesh.SetIndices(new[] { 0, 0, 0 }, MeshTopology.Triangles, 0);
        }

        [Button("Update Points")]
        public void UpdatePoints()
        {
            foreach (var e in elements.Where(e => e))
            {
                DestroyImmediate(e.gameObject);
            }

            foreach (var missedElements in GetComponentsInChildren<Transform>()
                         .Where(o => o.name.Contains("Cable_Element_")))
            {
                DestroyImmediate(missedElements.gameObject);
            }

            elements.Clear();
            joints.Clear();

            points = new List<Transform>(TotalElements);
            var direction = Vector3.Normalize(end.position - start.position);
            var distance = DistanceBetweenPoints;

            for (int i = 0; i < numberOfPoints; i++)
            {
                var newElement = new GameObject("Cable_Element_" + i,
                    typeof(Rigidbody),
                    typeof(JointType),
                    typeof(SphereCollider),
                    typeof(Friction));
                newElement.transform.position = start.position + direction * (distance * (i + 1));
                var rigidbody = newElement.GetComponent<Rigidbody>();
                rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
                joints.Add(newElement.GetComponent<JointType>());

                // Add two spring joints to the last element so that it can connect to its previous elements
                // and the end point
                if (i == numberOfPoints - 1)
                {
                    joints.Add(newElement.AddComponent<JointType>());
                }

                elements.Add(newElement.transform);
            }

            UpdateSpring();
        }

        void UpdateSpring()
        {
            var distance = DistanceBetweenPoints;
            for (var index = 0; index < elements.Count; index++)
            {
                var element = elements[index];

                SoftJointLimit limit = new SoftJointLimit
                {
                    limit = distance
                };
                var joints = element.GetComponents<JointType>();
                foreach (var joint in joints)
                {
                    joint.autoConfigureConnectedAnchor = false;
                    joint.linearLimit = limit;
                    joint.xMotion = ConfigurableJointMotion.Limited;
                    joint.yMotion = ConfigurableJointMotion.Limited;
                    joint.zMotion = ConfigurableJointMotion.Limited;
                    /*
                    springJoint.spring = springForce;
                    springJoint.damper = 1;
                    springJoint.determineDistanceOnStart = false;
                    springJoint.distance = distance;
                    springJoint.minDistance = distance / 2;
                    springJoint.maxDistance = distance / 2;
                    springJoint.tolerance = 0;*/
                }

                if (index == 0)
                {
                    joints[0].connectedBody = start.gameObject.GetOrCreateComponent<Rigidbody>();
                }
                else
                {
                    joints[0].connectedBody = elements[index - 1].GetComponent<Rigidbody>();
                }

                if (index == elements.Count - 1)
                {
                    joints[1].connectedBody = end.gameObject.GetOrCreateComponent<Rigidbody>();
                }


                element.SetParent(null);
                element.transform.localScale = new Vector3(size, size, size);
                element.transform.SetParent(transform);

                element.GetComponent<Friction>().friction = springDamper;
            }
        }

        private void UpdateLimit()
        {
            SoftJointLimit limit = new SoftJointLimit
            {
                limit = System.Math.Max(DistanceBetweenPoints, 0.05f),
                bounciness = 0,
                contactDistance = distanceTolerance
            };
            foreach (var joint in joints)
            {
                joint.linearLimit = limit;
            }
        }
    }
}