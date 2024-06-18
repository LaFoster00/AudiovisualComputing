using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SerializedReferenceID))]
public class SerializationReferableMonoBehaviour : MonoBehaviour
{
    public string GOID => GetComponent<SerializedReferenceID>().GOID;
}
