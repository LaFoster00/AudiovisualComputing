using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

namespace AVC
{
    public class ActionToHaptics : MonoBehaviour
    {
        public InputActionReference action;
        public InputActionReference hapticAction;
        public float _amplitude = 1.0f;
        public float _duration = 0.1f;
        public float _frequency = 0.0f;

        private void Start()
        {
            if (action == null || hapticAction == null)
                return;

            action.action.Enable();
            hapticAction.action.Enable();
            action.action.performed += (ctx) =>
            {
                var control = action.action.activeControl;
                if (null == control)
                    return;

                OpenXRInput.SendHapticImpulse(hapticAction.action, _amplitude, _frequency, _duration, control.device);
            };
        }
    }
}