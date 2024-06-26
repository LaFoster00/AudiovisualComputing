using System;
using System.Collections;
using System.Collections.Generic;
using Cable;
using Content.Serialization;
using UnityEditor;
using UnityEngine;

namespace Interactable.Cable.Audio
{
    public class AudioCableSerializer : MonoBehaviourGuid, IPersistentPrefab
    {
        public string prefabPath;

        [Serializable]
        private struct SaveData
        {
            public int numberOfPoints;
            public float size;
            public float distanceTolerance;
            public float springDamper;

            public int plug1Target;
            public int plug2Target;
        }

        public object Serialize()
        {
            var cable = GetComponent<PhysicCable>();
            var plugs = GetComponentsInChildren<Plug>();

            return new SaveData()
            {
                numberOfPoints = cable.numberOfPoints,
                size = cable.size,
                distanceTolerance = cable.distanceTolerance,
                springDamper = cable.distanceTolerance,
                plug1Target = plugs[0].GetSocketTarget().GetInstanceID(),
                plug2Target = plugs[1].GetSocketTarget().GetInstanceID()
            };
        }

        public void Deserialize(object Data)
        {
            throw new NotImplementedException();
        }

        public string GetPrefab()
        {
            return prefabPath;
        }
    }
}