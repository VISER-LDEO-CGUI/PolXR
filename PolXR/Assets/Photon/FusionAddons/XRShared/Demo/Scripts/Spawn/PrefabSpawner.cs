using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace Fusion.XRShared.Demo
{
    public class PrefabSpawner : NetworkBehaviour
    {
        public NetworkObject prefab;
        public NetworkObject currentInstance;

        public float liberationDistance = .5f;
        public override void FixedUpdateNetwork()
        {
            base.FixedUpdateNetwork();
            if (Object.HasStateAuthority && (currentInstance == null || Vector3.Distance(transform.position, currentInstance.transform.position) > liberationDistance))
            {
                Spawn();
            }
        }

        void Spawn()
        {
            if (prefab == null) return;
            currentInstance = Runner.Spawn(prefab, transform.position, transform.rotation);
        }
    }

}
