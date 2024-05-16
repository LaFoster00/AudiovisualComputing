using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CableConnector : MonoBehaviour
{
    public GameObject CablePoint;
    //public GameObject cableAnchor;

    public Vector3 CablePointOffset => CablePoint.transform.position - transform.position;
}
