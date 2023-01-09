using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CW.Common;

namespace Lean.Touch
{
    public class LeanTranslatePlane : LeanDragTranslate
    {
        [Tooltip("How smoothly this object moves to its target position")]
        public float Dampening = 10.0f;
        [HideInInspector]
        public Vector3 RemainingDelta;

        protected virtual void LateUpdate()
        {
            // Get t value
            var factor = CwHelper.DampenFactor(Dampening, Time.deltaTime);
            // Dampen remainingDelta
            var newDelta = Vector3.Lerp(RemainingDelta, Vector3.zero, factor);
            // Shift this transform by the change in delta
            Vector3 moveMe = RemainingDelta - newDelta;
            moveMe.z = moveMe.y;
            moveMe.y = 0f;
            transform.position += moveMe;
            // Update remainingDelta with the dampened value
            RemainingDelta = newDelta;
        }

        private void Translate(Vector2 screenDelta)
        {
            // Make sure the camera exists
            var camera = CwHelper.GetCamera(Camera, gameObject);
            if (camera == null)
            {
                return;
            }
            var oldPosition = transform.position;
            var screenPosition = camera.WorldToScreenPoint(oldPosition);
            // Add the deltaPosition
            screenPosition += (Vector3)screenDelta;
            // Convert back to world space
            var newPosition = camera.ScreenToWorldPoint(screenPosition);
            // Add to delta
            RemainingDelta += newPosition - oldPosition;
        }
    }
}
