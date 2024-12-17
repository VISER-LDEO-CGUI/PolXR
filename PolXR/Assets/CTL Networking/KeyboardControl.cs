using Fusion;
//using GLTFast.Schema;
using TMPro;
using UnityEngine;

public class KeyboardControl : NetworkBehaviour
{
    //[SerializeField] private Ball _prefabBall;

    //[Networked] private TickTimer delay { get; set; }

    //private NetworkCharacterController _cc;
    //private Vector3 _forward;

    //[Networked]
    //public bool spawnedProjectile { get; set; }

    //private ChangeDetector _changeDetector;

    //public override void Spawned()
    //{
    //    _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    //}

    //public UnityEngine.Material _material;


    //private void Awake()
    //{
        //_cc = GetComponent<NetworkCharacterController>();
        //_forward = transform.forward;
        //surfaceDEM = GameObject.Find("MEASURES_NSIDC-0715-002");

        //_material = GetComponentInChildren<MeshRenderer>().material;
    //}

    //public override void Render()
    //{
    //    foreach (var change in _changeDetector.DetectChanges(this))
    //    {
    //        switch (change)
    //        {
    //            case nameof(spawnedProjectile):
    //                _material.color = Color.white;
    //                break;
    //        }
    //    }
    //    _material.color = Color.Lerp(_material.color, Color.blue, Time.deltaTime);
    //}

    public override void FixedUpdateNetwork()
    {
        //if (GetInput(out NetworkInputData data))
        //{
        //    if (Object.HasInputAuthority)
        //    {
        //        if (Input.GetKeyDown(KeyCode.R))
        //        {
        //            GameObject DEM = GameObject.Find("DEMs(Clone)");
        //            NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
        //            DEMController.toggle("MEASURES_NSIDC - 0715 - 002");
        //        } else if (Input.GetKeyDown(KeyCode.T))
        //        {
        //            GameObject DEM = GameObject.Find("DEMs(Clone)");
        //            NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
        //            DEMController.toggle("bottom");
        //        }
        //    }
        //}
    }

    //public override void FixedUpdateNetwork()
    //{
    //    if (GetInput(out NetworkInputData data))
    //    {
    //        // Debug.Log("data came in success");
    //        data.direction.Normalize();
    //        _cc.Move(5 * data.direction * Runner.DeltaTime);

    //        if (data.direction.sqrMagnitude > 0)
    //            _forward = data.direction;

    //        if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
    //        {
    //            if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
    //            {
    //                // Debug.Log("player shoot");
    //                delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
    //                Runner.Spawn(_prefabBall, transform.position + _forward, Quaternion.LookRotation(_forward),
    //                Object.InputAuthority, (runner, o) =>
    //                {
    //                    // Initialize the Ball before synchronizing it
    //                    o.GetComponent<Ball>().Init();
    //                });
    //                spawnedProjectile = !spawnedProjectile;
    //            }
    //        }
    //    }
    //}
    //private TMP_Text _messages;
    //public GameObject surfaceDEM;
    //private void Update()
    //{
    //    if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
    //    {
    //        RPC_SendMessage("Hey Mate!");
    //    }
    //}

    //public void ChangeDEM()
    //{
    //    Debug.Log("Change command sent!");
    //    surfaceDEM.SetActive(!surfaceDEM.activeSelf);
    //}


    //[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    //public void RPC_SendMessage(string message, RpcInfo info = default)
    //{
    //    RPC_RelayMessage(message, info.Source);
    //}
    //[Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    //public void RPC_RelayMessage(string message, PlayerRef messageSource)
    //{
    //    if (_messages == null)
    //        _messages = FindObjectOfType<TMP_Text>();

    //    if (messageSource == Runner.LocalPlayer)
    //    {
    //        message = $"You said: {message}\n";
    //    }
    //    else
    //    {
    //        message = $"Some other player said: {message}\n";
    //    }

    //    _messages.text += message;
    //    surfaceDEM = GameObject.Find("MEASURES_NSIDC-0715-002");
    //    GameObject DEM = GameObject.Find("DEMs(Clone)");
    //    NetworkedDEMController DEMController = DEM.GetComponent<NetworkedDEMController>();
    //    // simply turn off surface
    //    DEMController.toggle("MEASURES_NSIDC-0715-002");

    //    // ChangeDEM();
    //}
}
