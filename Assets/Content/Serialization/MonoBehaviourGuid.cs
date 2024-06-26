using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SerializedReferenceID))]
public class MonoBehaviourGuid : MonoBehaviour
{
    public string guid => GetComponent<SerializedReferenceID>().GUID;
}
