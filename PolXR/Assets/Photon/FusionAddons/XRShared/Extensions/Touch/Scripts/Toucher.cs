using Fusion.XR.Shared.Rig;
using UnityEngine;

namespace Fusion.XR.Shared.Touch
{
    public interface ITouchable
    {
        public void OnToucherContactStart(Toucher toucher);
        public void OnToucherStay(Toucher toucher);
        public void OnToucherContactEnd(Toucher toucher);
    }

    /**
     * Allow to detect ITouchable components in contact
     * 
     * Must be store under an HardwareHand to allow ITouchables to potentially manage haptic feedback properly
     */
    public class Toucher : MonoBehaviour
    {
        public HardwareHand hardwareHand;
        [SerializeField]
        protected bool lookForTouchableInColliderParent = true;

        protected virtual void Awake()
        {
            hardwareHand = GetComponentInParent<HardwareHand>();
        }

        protected Collider lastCheckCollider = null;
        ITouchable lastCheckedTouchable = null;
        ITouchable LookForTouchable(Collider other)
        {
            if (other != lastCheckCollider)
            {
                CheckCollider(other);
            }
            return lastCheckedTouchable;
        }

        protected virtual void CheckCollider(Collider other)
        {
            lastCheckCollider = other;
            if (lookForTouchableInColliderParent)
            {
                lastCheckedTouchable = other.GetComponentInParent<ITouchable>();
            }
            else
            {
                lastCheckedTouchable = other.GetComponent<ITouchable>();
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            ITouchable otherGameObjectTouchable = LookForTouchable(other);
            if (otherGameObjectTouchable != null)
            {
                otherGameObjectTouchable.OnToucherContactStart(this);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            ITouchable otherGameObjectTouchable = LookForTouchable(other);
            if (otherGameObjectTouchable != null)
            {
                otherGameObjectTouchable.OnToucherStay(this);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            ITouchable otherGameObjectTouchable = LookForTouchable(other);
            if (otherGameObjectTouchable != null)
            {
                otherGameObjectTouchable.OnToucherContactEnd(this);
            }
        }
    }

}

