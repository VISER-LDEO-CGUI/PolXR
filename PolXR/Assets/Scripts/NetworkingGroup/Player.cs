using Fusion;
using TMPro;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private Ball _prefabBall;

    [Networked] private TickTimer delay { get; set; }

    private NetworkCharacterController _cc;
    private Vector3 _forward;

    private void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _forward = transform.forward;
        surfaceDEM = GameObject.Find("MEASURES_NSIDC-0715-002");
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // Debug.Log("data came in success");
            data.direction.Normalize();
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            if (data.direction.sqrMagnitude > 0)
                _forward = data.direction;

            if (HasStateAuthority && delay.ExpiredOrNotRunning(Runner))
            {
                if (data.buttons.IsSet(NetworkInputData.MOUSEBUTTON0))
                {
                    // Debug.Log("player shoot");
                    delay = TickTimer.CreateFromSeconds(Runner, 0.5f);
                    Runner.Spawn(_prefabBall,
                    transform.position + _forward, Quaternion.LookRotation(_forward),
                    Object.InputAuthority, (runner, o) =>
                    {
                        // Initialize the Ball before synchronizing it
                        o.GetComponent<Ball>().Init();
                    });
                }
            }
        }
    }
    private TMP_Text _messages;
    public GameObject surfaceDEM;
    private void Update()
    {
        if (Object.HasInputAuthority && Input.GetKeyDown(KeyCode.R))
        {
            RPC_SendMessage("Hey Mate!");
        }
    }

    public void ChangeDEM()
    {
        Debug.Log("Change command sent!");
        surfaceDEM.SetActive(!surfaceDEM.activeSelf);
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SendMessage(string message, RpcInfo info = default)
    {
        RPC_RelayMessage(message, info.Source);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RelayMessage(string message, PlayerRef messageSource)
    {
        if (_messages == null)
            _messages = FindObjectOfType<TMP_Text>();

        if (messageSource == Runner.LocalPlayer)
        {
            message = $"You said: {message}\n";
        }
        else
        {
            message = $"Some other player said: {message}\n";
        }

        _messages.text += message;
        ChangeDEM();
    }
}