using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fusion.XR.Shared
{
    public abstract class ApplicationLifeCycleManager : MonoBehaviour
    {
        // Called by components planning to disconnect the application
        public virtual void OnApplicationQuitRequest() { }

        // Called by components which want to disallow full screen menu (focus manager, full screen manager, etc.)
        public virtual void ChangeMenuAuthorization(bool authorized) { }
    }
}
