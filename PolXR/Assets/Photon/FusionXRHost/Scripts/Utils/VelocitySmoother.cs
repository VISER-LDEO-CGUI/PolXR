using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Host.Utils
{
    /**
     * Compute an average velocity. 
     * Useful when no rigidbody is available, or for very high tick rate (~ 120), as rigidbody regular velocity computed with very short deltaTime values tend to not be relevant anymore for release velocity
     */
    public class VelocitySmoother : MonoBehaviour
    {
        [Tooltip("If false, the velocity won't be computed anymore - to decrease CPU usage")]
        public bool istrackingVelocity = true;

        [Tooltip("If true (and a rigidbody is present), the returned average velocity will preserve the direction of the rigidbody velocity")]
        public bool useRigidbodyVelocityDirection = true;

        Rigidbody rb;
        NetworkObject no;

        #region Velocity estimation
        // Velocity computation
        const int velocityBufferSize = 5;
        Vector3 lastPosition;
        Quaternion previousRotation;
        Vector3[] lastMoves = new Vector3[velocityBufferSize];
        Vector3[] lastAngularVelocities = new Vector3[velocityBufferSize];
        float[] lastDeltaTime = new float[velocityBufferSize];
        int lastMoveIndex = 0;

        public Vector3 AverageVelocity
        {
            get
            {
                Vector3 move = Vector3.zero;
                float time = 0;
                for (int i = 0; i < velocityBufferSize; i++)
                {
                    if (lastDeltaTime[i] != 0)
                    {
                        move += lastMoves[i];
                        time += lastDeltaTime[i];
                    }
                }
                if (time == 0) return Vector3.zero;
                var averageVelocity = (move / time);
                if (rb && useRigidbodyVelocityDirection)
                {
                    averageVelocity = averageVelocity.magnitude * rb.velocity.normalized;
                }
                return averageVelocity;
            }
        }

        public Vector3 AverageAngularVelocity
        {
            get
            {
                Vector3 culmulatedAngularVelocity = Vector3.zero;
                int step = 0;
                for (int i = 0; i < velocityBufferSize; i++)
                {
                    if (lastDeltaTime[i] != 0)
                    {
                        culmulatedAngularVelocity += lastAngularVelocities[i];
                        step++;
                    }
                }
                if (step == 0) return Vector3.zero;
                return culmulatedAngularVelocity / step;
            }
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            no = GetComponentInParent<NetworkObject>();
        }

        void TrackVelocity()
        {
            lastMoves[lastMoveIndex] = transform.position - lastPosition;
            lastAngularVelocities[lastMoveIndex] = previousRotation.AngularVelocityChange(transform.rotation, Time.deltaTime);
            lastDeltaTime[lastMoveIndex] = Time.deltaTime;
            lastMoveIndex = (lastMoveIndex + 1) % 5;
            lastPosition = transform.position;
            previousRotation = transform.rotation;
        }

        public void ResetVelocityTracking()
        {
            for (int i = 0; i < velocityBufferSize; i++) lastDeltaTime[i] = 0;
            lastMoveIndex = 0;
        }
        #endregion

        private void Update()
        {
            if(no && no.Runner && no.Runner.IsResimulation)
            {
                // We only read velocity during forward ticks, to avoid detecting past positions of the object
                return;
            }
            if(istrackingVelocity) TrackVelocity();
        }
    }
}
