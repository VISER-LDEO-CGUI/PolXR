using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared
{
    /***
     * 
     * VelocityExtension provides the AngularVelocityChange helper method : it returns an angular velocity based on the elapsed time between the previous and the new rotation
     * 
     ***/
    public static class VelocityExtension
    {
        public static Vector3 AngularVelocityChange(this Quaternion previousRotation, Quaternion newRotation, float elapsedTime)
        {
            Quaternion rotationStep = newRotation * Quaternion.Inverse(previousRotation);
            rotationStep.ToAngleAxis(out float angle, out Vector3 axis);
            // Angular velocity uses eurler notation, bound to -180° / +180°
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (Mathf.Abs(angle) > Mathf.Epsilon)
            {
                float radAngle = angle * Mathf.Deg2Rad;
                Vector3 angularStep = axis * radAngle;
                Vector3 angularVelocity = angularStep / elapsedTime;
                if (!float.IsNaN(angularVelocity.x))
                    return angularVelocity;
            }
            return Vector3.zero;
        }
    }
}
