using Fusion.XR.Host.Rig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.XR.Host.Grabbing
{
    [DefaultExecutionOrder(NetworkGrabbable.EXECUTION_ORDER)]
    public abstract class NetworkGrabbable : NetworkBehaviour
    {
        public const int EXECUTION_ORDER = NetworkGrabber.EXECUTION_ORDER + 10;
        public virtual NetworkGrabber CurrentGrabber { get; set; }
        public bool IsGrabbed => CurrentGrabber != null;

        [Header("Events")]
        public UnityEvent onDidUngrab = new UnityEvent();
        public UnityEvent<NetworkGrabber> onDidGrab = new UnityEvent<NetworkGrabber>();
        
        public abstract void Grab(NetworkGrabber newGrabber, GrabInfo newGrabInfo);
        public abstract void Ungrab(NetworkGrabber grabber, GrabInfo newGrabInfo);

        public void DidGrab()
        {
            if (onDidGrab != null) onDidGrab.Invoke(CurrentGrabber);
        }

        public void DidUngrab(NetworkGrabber lastGrabber)
        {
            if (onDidGrab != null) onDidUngrab.Invoke();
        }
    }
}


