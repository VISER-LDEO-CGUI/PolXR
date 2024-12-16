using Fusion.Sockets;
using Fusion.XR.Host.Grabbing;
using GLTFast;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Microsoft.MixedReality.Toolkit.UI.BoundsControlTypes;
using System.IO;


namespace Fusion.Addons.ConnectionManagerAddon
{
    /**
     * 
     * Handles:
     * - connection launch (either with room name or matchmaking session properties)
     * - user representation spawn on connection
     **/
    public class ConnectionManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [System.Flags]
        public enum ConnectionCriterias
        {
            RoomName = 1,
            SessionProperties = 2
        }

        [System.Serializable]
        public struct StringSessionProperty
        {
            public string propertyName;
            public string value;
        }

        [Header("Room configuration")]
        public GameMode gameMode = GameMode.Shared;
        public string roomName = "SampleFusion";
        public bool connectOnStart = true;
        [Tooltip("Set it to 0 to use the DefaultPlayers value, from the Global NetworkProjectConfig (simulation section)")]
        public int playerCount = 0;

        [Header("Room selection criteria")]
        public ConnectionCriterias connectionCriterias = ConnectionCriterias.RoomName;
        [Tooltip("If connectionCriterias include SessionProperties, additionalSessionProperties (editable in the inspector) will be added to sessionProperties")]
        public List<StringSessionProperty> additionalSessionProperties = new List<StringSessionProperty>();
        public Dictionary<string, SessionProperty> sessionProperties;

        [Header("Fusion settings")]
        [Tooltip("Fusion runner. Automatically created if not set")]
        public NetworkRunner runner;
        public INetworkSceneManager sceneManager;

        [Header("Local user spawner")]
        public NetworkObject userPrefab;

        [Header("Event")]
        public UnityEvent onWillConnect = new UnityEvent();

        [Header("Info")]
        public List<StringSessionProperty> actualSessionProperties = new List<StringSessionProperty>();

        [Header("Prefab to generate")]
        [SerializeField] NetworkObject DEMsPrefab;
        [SerializeField] NetworkObject RadarImageContainer;

        // Dictionary of spawned user prefabs, to store them on the server for host topology, and destroy them on disconnection (for shared topology, use Network Objects's "Destroy When State Authority Leaves" option)
        private Dictionary<PlayerRef, NetworkObject> _spawnedUsers = new Dictionary<PlayerRef, NetworkObject>();

        bool ShouldConnectWithRoomName => (connectionCriterias & ConnectionManager.ConnectionCriterias.RoomName) != 0;
        bool ShouldConnectWithSessionProperties => (connectionCriterias & ConnectionManager.ConnectionCriterias.SessionProperties) != 0;

        private void Awake()
        {
            // Check if a runner exist on the same game object
            if (runner == null) runner = GetComponent<NetworkRunner>();

            // Create the Fusion runner and let it know that we will be providing user input
            if (runner == null) runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;
        }

        private async void Start()
        {
            // Launch the connection at start
            if (connectOnStart) await Connect();
        }

        Dictionary<string, SessionProperty> AllConnectionSessionProperties
        {
            get
            {
                var propDict = new Dictionary<string, SessionProperty>();
                actualSessionProperties = new List<StringSessionProperty>();
                if (sessionProperties != null)
                {
                    foreach (var prop in sessionProperties)
                    {
                        propDict.Add(prop.Key, prop.Value);
                        actualSessionProperties.Add(new StringSessionProperty { propertyName = prop.Key, value = prop.Value });
                    }
                }
                if (additionalSessionProperties != null)
                {
                    foreach (var additionalProperty in additionalSessionProperties)
                    {
                        propDict[additionalProperty.propertyName] = additionalProperty.value;
                        actualSessionProperties.Add(additionalProperty);
                    }

                }
                return propDict;
            }
        }

        public virtual NetworkSceneInfo CurrentSceneInfo()
        {
            var activeScene = SceneManager.GetActiveScene();
            SceneRef sceneRef = default;

            if (activeScene.buildIndex < 0 || activeScene.buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                Debug.LogError("Current scene is not part of the build settings");
            }
            else
            {
                sceneRef = SceneRef.FromIndex(activeScene.buildIndex);
            }

            var sceneInfo = new NetworkSceneInfo();
            if (sceneRef.IsValid)
            {
                sceneInfo.AddSceneRef(sceneRef, LoadSceneMode.Single);
            }
            return sceneInfo;
        }

        public async Task Connect()
        {
            // Create the scene manager if it does not exist
            if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
            if (onWillConnect != null) onWillConnect.Invoke();

            // Start or join (depends on gamemode) a session with a specific name
            // Original code
            //var args = new StartGameArgs()

            //{
            //    GameMode = gameMode,
            //    Scene = CurrentSceneInfo(),
            //    SceneManager = sceneManager
            //};

            // CTL Team Change: Use customized Object Provider instead of fallback to default one
            var args = new StartGameArgs()
            {
                ObjectProvider = new BakingObjectProvider(),
                GameMode = gameMode,
                Scene = CurrentSceneInfo(),
                SceneManager = sceneManager
            };
            // Connection criteria
            if (ShouldConnectWithRoomName)
            {
                args.SessionName = roomName;
            }
            if (ShouldConnectWithSessionProperties)
            {
                args.SessionProperties = AllConnectionSessionProperties;
            }
            // Room details
            if (playerCount > 0)
            {
                args.PlayerCount = playerCount;
            }

            await runner.StartGame(args);

            string prop = "";
            if (runner.SessionInfo.Properties != null && runner.SessionInfo.Properties.Count > 0)
            {
                prop = "SessionProperties: ";
                foreach (var p in runner.SessionInfo.Properties) prop += $" ({p.Key}={p.Value.PropertyValue}) ";
            }
            Debug.Log($"Session info: Room name {runner.SessionInfo.Name}. Region: {runner.SessionInfo.Region}. {prop}");
            if ((connectionCriterias & ConnectionManager.ConnectionCriterias.RoomName) == 0)
            {
                roomName = runner.SessionInfo.Name;
            }
        }

        #region Player spawn
        public void OnPlayerJoinedSharedMode(NetworkRunner runner, PlayerRef player)
        {
            if (player == runner.LocalPlayer && userPrefab != null)
            {
                // Spawn the user prefab for the local user
                NetworkObject networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, player, (runner, obj) => {
                });
            }
        }
        public Transform Container;
        //public GameObject radarMark;
        public GameObject DEM;
        public GameObject gridLine;

        public GameObject MarkObj3D;

        public async void OnPlayerJoinedHostMode(NetworkRunner runner, PlayerRef player)
        {
            // The user's prefab has to be spawned by the host
            if (runner.IsServer && userPrefab != null)
            {
                Debug.Log($"OnPlayerJoined. PlayerId: {player.PlayerId}");
                // We make sure to give the input authority to the connecting player for their user's object
                NetworkObject networkPlayerObject = runner.Spawn(userPrefab, position: transform.position, rotation: transform.rotation, inputAuthority: player, (runner, obj) => {
                });

                // Keep track of the player avatars so we can remove it when they disconnect
                _spawnedUsers.Add(player, networkPlayerObject);

                // CTL Network Team change:
                if (player.PlayerId == 1) {
                    NetworkObject DEMsNetwork = runner.Spawn(DEMsPrefab, position: new Vector3(10, -1, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: player, (runner, obj) => {
                    });
                    Debug.LogWarning("Spawned DEMs with Player 1");

                    //NetworkPrefabId demID = new NetworkPrefabId();
                    //demID.RawValue = 100000;
                    //for (int i = 0; i < 10; i++) {
                    //    NetworkObject DEMsID = runner.Spawn(demID, position: new Vector3(10, -1, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: player, (runner, obj) => {
                    //    });
                    //}
                    NetworkPrefabId parentID = new NetworkPrefabId();
                    parentID.RawValue = 100000;
                    NetworkObject parent = runner.Spawn(parentID, position: new Vector3(0, 0, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: null);
                    // RadarImageContainer.GetComponent<LoadFlightLines>().callStarts();



                    /*


                    gameObject.AddComponent<LoadFlightLines>();
                    LoadFlightLines flightLinesLoader = gameObject.GetComponent<LoadFlightLines>();

                    BetterStreamingAssets.Initialize();
                    // LoadFlightLine("20100324_01"); // TODO: replace with menu option
                    Debug.LogWarning("Loaded flight line in Advanced Spawning");


                    // // Load the data // old workflow loading from resources
                    //UnityEngine.Object[] meshes = Resources.LoadAll(Path.Combine("Radar3D", "Radar", line_id));
                    Dictionary<string, GameObject> polylines = flightLinesLoader.createPolylineObjects("20100324_01");
                    Debug.LogWarning("Passed createPolylinObjects");
                    //GameObject prefab = Instantiate(Resources.Load(Path.Combine("Radar3D", "Radar", "RadarContainer")) as GameObject);


                    // Load the glTF asset from streaming assets folder
                    byte[] data = BetterStreamingAssets.ReadAllBytes("radar.glb");

                    var gltf = new GltfImport();
                    Debug.LogWarning("Try to load data");
                    var success = await gltf.Load(data);
                    Debug.LogWarning("Loaded data");
                    UnityEngine.Object[] meshes;

                    if (success)
                    {
                        // Extract meshes and textures
                        meshes = flightLinesLoader.ExtractMeshes(gltf);
                    }
                    else
                    {
                        Debug.LogError("Failed to load glTF asset");
                        return;
                    }

                    NetworkPrefabId parentID = new NetworkPrefabId();
                    NetworkPrefabId radargramID = new NetworkPrefabId();

                    Debug.LogWarning("mesh length" + meshes.Length);

                    for (int i = 0; i < meshes.Length; i++)
                    {
                        // Create radargram objects
                        GameObject[] meshBoth = flightLinesLoader.createRadargramObjects(meshes[i]);
                        GameObject meshForward = meshBoth[0];
                        GameObject meshBackward = meshBoth[1];
                        Bounds meshBounds = meshForward.GetComponent<Renderer>().bounds; // cuz we need bounds in world coords

                        // Select and name line
                        string key = meshForward.name.Substring(meshForward.name.IndexOf('_', meshForward.name.Length - 5));
                        GameObject line;
                        try
                        {
                            line = polylines[key];
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Polyline not found: '" + key + "'");
                            Destroy(meshForward);
                            Destroy(meshBackward);
                            continue;
                        }
                        line.name = $"FL_{meshForward.name.Trim().Substring(5)}";

                        // Create a parent for all the new objects to associate with RadarEvents3D
                        string parentName = "#" + key; //"GRP_" + meshForward.name;
                        // change the parent to parentLocal to distinguish
                        GameObject parentLocal = new GameObject(parentName);

                        // CTL Team Change:
                        parentID.RawValue = 100000;
                        NetworkObject parent = runner.Spawn(parentID, position: new Vector3(0, 0, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: player, (runner, obj) => {
                        });
                        parentID.RawValue += 1;



                        parent.transform.SetParent(Container);
                        // Single player version
                        // RadarEvents3D script = parent.AddComponent<RadarEvents3D>();
                        // Photon uses AddBehavior
                        RadarEvents3D script = parent.AddBehaviour<RadarEvents3D>();
                        parent.transform.localScale = new Vector3(1, 1, 1);
                        parent.transform.localPosition = new Vector3(0, 0, 0);
                        parent.transform.rotation = Quaternion.identity;
                        // Commented out Reason: Photon can't use this since BoundsControl is MonoBehavior only
                        // BoundsControl parentBoundsControl = parent.AddBehaviour<BoundsControl>();

                        // Commented out Reason: Photon can't use this since BoundsControl is MonoBehavior only
                        //turns off gizmos and bounding boxes
                        //parentBoundsControl.LinksConfig.ShowWireFrame = false;
                        //parentBoundsControl.RotationHandlesConfig.ShowHandleForX = false;
                        //parentBoundsControl.RotationHandlesConfig.ShowHandleForY = false;
                        //parentBoundsControl.RotationHandlesConfig.ShowHandleForZ = false;
                        //parentBoundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

                        // Create a parent to group both radargram objects
                        GameObject radargramLocal = new GameObject("OBJ_" + meshForward.name);

                        // CTL Team Change:
                        radargramID.RawValue = 200000;
                        NetworkObject radargram = runner.Spawn(radargramID, position: new Vector3(0, 0, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: player, (runner, obj) => {
                        });
                        radargramID.RawValue += 1;


                        radargram.transform.localPosition = meshBounds.center;
                        // MeshCollider radarCollider = radargram.AddComponent<MeshCollider>();
                        // radarCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;

                        // add mesh colliders to each of the mesh forward and backward
                        MeshCollider meshForwardCollider = meshForward.AddComponent<MeshCollider>();
                        meshForwardCollider.sharedMesh = meshForward.GetComponent<MeshFilter>().mesh;

                        MeshCollider meshBackwardCollider = meshBackward.AddComponent<MeshCollider>();
                        meshBackwardCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;


                        // Organize the children
                        line.transform.parent = parent.transform;
                        radargram.transform.parent = parent.transform;
                        meshForward.transform.parent = radargram.transform;
                        meshBackward.transform.parent = radargram.transform;

                        // Place polyline properly in relation to the DEM
                        line.transform.rotation = Quaternion.Euler(-90f, 0f, 180f);

                        // Add the correct Bounds Control so that MRTK knows where the objects are
                        // Commented out Reason: Photon can't use this since BoundsControl is MonoBehavior only
                        //BoundsControl boundsControl = radargram.AddComponent<BoundsControl>();
                        //boundsControl.CalculationMethod = BoundsCalculationMethod.ColliderOverRenderer;

                        //turns off gizmos and bounding boxes
                        // Commented out Reason: Photon can't use this since BoundsControl is MonoBehavior only
                        //boundsControl.LinksConfig.ShowWireFrame = false;
                        //boundsControl.RotationHandlesConfig.ShowHandleForX = false;
                        //boundsControl.RotationHandlesConfig.ShowHandleForY = false;
                        //boundsControl.RotationHandlesConfig.ShowHandleForZ = false;
                        //boundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

                        BoxCollider boxCollider = radargram.GetComponent<BoxCollider>();
                        boxCollider.center = new Vector3(0, 0, 0);//meshBounds.center;
                        boxCollider.size = meshBounds.size;
                        // Commented out Reason: Photon can't use this since BoundsControl is MonoBehavior only
                        // boundsControl.BoundsOverride = boxCollider;

                        // Commented out Reason: Photon can't use this since RotationAxisConstraint is MonoBehavior only
                        // Constrain the rotation axes
                        // RotationAxisConstraint rotationConstraint = radargram.AddComponent<RotationAxisConstraint>();
                        // rotationConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;

                        // Set the parent's BoxCollider to have the same bounds
                        BoxCollider parentCollider = parent.GetComponent<BoxCollider>();

                        // Add the correct Object Manipulator so users can grab the radargrams
                        // Commented out Reason: Photon can't use this since MRTK uses MonoBehavior
                        // radargram.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
                        // radargram.AddComponent<NearInteractionGrabbable>();
                        // Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator = radargram.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();

                        // objectManipulator.enabled = true;

                        // Link the parent to the menu
                        script.Menu = GameObject.Find("Menu");

                        // Create and place the radar mark for the minimap
                        Vector3 position = meshForward.transform.position + meshForward.transform.localPosition; // TODO: this
                                                                                                                 //GameObject mark = Instantiate(radarMark, position, Quaternion.identity, parent.transform);

                        //GameObject markObj3D = Instantiate(MarkObj3D, position, Quaternion.identity, radargram.transform);
                        GameObject markObj3D = Instantiate(MarkObj3D, radargram.transform);
                        markObj3D.transform.localPosition = Vector3.zero;
                    }

                    //Drop everything onto the DEM -- this should correlate with the DEM position
                    Container.transform.localPosition = new Vector3(-10f, 0f, 10f);
                    foreach (var obj in meshes)
                    {
                        Destroy(obj);
                    }

                    */

                }
            }

        }

        // Despawn the user object upon disconnection
        public void OnPlayerLeftHostMode(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar (only the host would have stored the spawned game object)
            if (_spawnedUsers.TryGetValue(player, out NetworkObject networkObject))
            {
                runner.Despawn(networkObject);
                _spawnedUsers.Remove(player);
            }
        }

        #endregion

        #region INetworkRunnerCallbacks
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if(runner.Topology == Topologies.ClientServer)
            {
                OnPlayerJoinedHostMode(runner, player);
            }
            else
            {
                OnPlayerJoinedSharedMode(runner, player);
            }
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
            if (runner.Topology == Topologies.ClientServer)
            {
                OnPlayerLeftHostMode(runner, player);
            }
        }

        private bool _mouseButton0;
        private void Update()
        {
            _mouseButton0 = _mouseButton0 | Input.GetKey(KeyCode.X);
            if (Input.GetKey(KeyCode.X))
            {
                // Debug.Log("shoot");
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();

            if (Input.GetKey(KeyCode.W)) {
                data.direction += Vector3.forward;
                Debug.Log("going forward");
            }
            if (Input.GetKey(KeyCode.K)) {
                data.direction += Vector3.back;
                Debug.Log("going backward");
            }

            if (Input.GetKey(KeyCode.J)) {
                data.direction += Vector3.left;
                Debug.Log("going left");
            }

            if (Input.GetKey(KeyCode.L)) { 
                data.direction += Vector3.right;
                Debug.Log("going right");
            }
            data.buttons.Set(NetworkInputData.MOUSEBUTTON0, _mouseButton0);
            _mouseButton0 = false;

            input.Set(data);
        }
        #endregion

        #region INetworkRunnerCallbacks (debug log only)
        public void OnConnectedToServer(NetworkRunner runner) {
            Debug.Log("OnConnectedToServer");

        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("Shutdown: " + shutdownReason);
        }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {
            Debug.Log("OnDisconnectedFromServer: "+ reason);
        }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
            Debug.Log("OnConnectFailed: " + reason);
        }
        #endregion

        #region LoadFlightLines

        /*

    public class LoadFlightLines : MonoBehaviour
        {
            public Transform Container;
            //public GameObject radarMark;
            public GameObject DEM;
            public GameObject gridLine;

            public GameObject MarkObj3D;
            //public void Start()
            //{
            //    BetterStreamingAssets.Initialize();
            //    LoadFlightLine("20100324_01"); // TODO: replace with menu option
            //    Debug.Log("Loaded flight line");
            //}
            // public class YourCustomInstantiator : GLTFast.IInstantiator {
            // // Your code here
            // }
            public async void LoadFlightLine(string line_id)
            {
                // // Load the data // old workflow loading from resources
                //UnityEngine.Object[] meshes = Resources.LoadAll(Path.Combine("Radar3D", "Radar", line_id));
                Dictionary<string, GameObject> polylines = createPolylineObjects(line_id);
                //GameObject prefab = Instantiate(Resources.Load(Path.Combine("Radar3D", "Radar", "RadarContainer")) as GameObject);

                // Load the glTF asset from streaming assets folder
                byte[] data = BetterStreamingAssets.ReadAllBytes("radar.glb");

                var gltf = new GltfImport();
                var success = await gltf.Load(data);
                UnityEngine.Object[] meshes;

                if (success)
                {
                    // Extract meshes and textures
                    meshes = ExtractMeshes(gltf);
                }
                else
                {
                    Debug.LogError("Failed to load glTF asset");
                    return;
                }

                for (int i = 0; i < meshes.Length; i++)
                {
                    // Create radargram objects
                    GameObject[] meshBoth = createRadargramObjects(meshes[i]);
                    GameObject meshForward = meshBoth[0];
                    GameObject meshBackward = meshBoth[1];
                    Bounds meshBounds = meshForward.GetComponent<Renderer>().bounds; // cuz we need bounds in world coords

                    // Select and name line
                    string key = meshForward.name.Substring(meshForward.name.IndexOf('_', meshForward.name.Length - 5));
                    GameObject line;
                    try
                    {
                        line = polylines[key];
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Polyline not found: '" + key + "'");
                        Destroy(meshForward);
                        Destroy(meshBackward);
                        continue;
                    }
                    line.name = $"FL_{meshForward.name.Trim().Substring(5)}";

                    // Create a parent for all the new objects to associate with RadarEvents3D
                    string parentName = "#" + key; //"GRP_" + meshForward.name;
                    GameObject parent = new GameObject(parentName);
                    parent.transform.SetParent(Container);
                    RadarEvents3D script = parent.AddComponent<RadarEvents3D>();
                    parent.transform.localScale = new Vector3(1, 1, 1);
                    parent.transform.localPosition = new Vector3(0, 0, 0);
                    parent.transform.rotation = Quaternion.identity;
                    BoundsControl parentBoundsControl = parent.AddComponent<BoundsControl>();

                    //turns off gizmos and bounding boxes
                    parentBoundsControl.LinksConfig.ShowWireFrame = false;
                    parentBoundsControl.RotationHandlesConfig.ShowHandleForX = false;
                    parentBoundsControl.RotationHandlesConfig.ShowHandleForY = false;
                    parentBoundsControl.RotationHandlesConfig.ShowHandleForZ = false;
                    parentBoundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

                    // Create a parent to group both radargram objects
                    GameObject radargram = new GameObject("OBJ_" + meshForward.name);
                    radargram.transform.localPosition = meshBounds.center;
                    // MeshCollider radarCollider = radargram.AddComponent<MeshCollider>();
                    // radarCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;

                    // add mesh colliders to each of the mesh forward and backward
                    MeshCollider meshForwardCollider = meshForward.AddComponent<MeshCollider>();
                    meshForwardCollider.sharedMesh = meshForward.GetComponent<MeshFilter>().mesh;

                    MeshCollider meshBackwardCollider = meshBackward.AddComponent<MeshCollider>();
                    meshBackwardCollider.sharedMesh = meshBackward.GetComponent<MeshFilter>().mesh;


                    // Organize the children
                    line.transform.parent = parent.transform;
                    radargram.transform.parent = parent.transform;
                    meshForward.transform.parent = radargram.transform;
                    meshBackward.transform.parent = radargram.transform;

                    // Place polyline properly in relation to the DEM
                    line.transform.rotation = Quaternion.Euler(-90f, 0f, 180f);

                    // Add the correct Bounds Control so that MRTK knows where the objects are
                    BoundsControl boundsControl = radargram.AddComponent<BoundsControl>();
                    boundsControl.CalculationMethod = BoundsCalculationMethod.ColliderOverRenderer;

                    //turns off gizmos and bounding boxes
                    boundsControl.LinksConfig.ShowWireFrame = false;
                    boundsControl.RotationHandlesConfig.ShowHandleForX = false;
                    boundsControl.RotationHandlesConfig.ShowHandleForY = false;
                    boundsControl.RotationHandlesConfig.ShowHandleForZ = false;
                    boundsControl.ScaleHandlesConfig.ShowScaleHandles = false;

                    BoxCollider boxCollider = radargram.GetComponent<BoxCollider>();
                    boxCollider.center = new Vector3(0, 0, 0);//meshBounds.center;
                    boxCollider.size = meshBounds.size;
                    boundsControl.BoundsOverride = boxCollider;

                    // Constrain the rotation axes
                    RotationAxisConstraint rotationConstraint = radargram.AddComponent<RotationAxisConstraint>();
                    rotationConstraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;

                    // Set the parent's BoxCollider to have the same bounds
                    BoxCollider parentCollider = parent.GetComponent<BoxCollider>();

                    // Add the correct Object Manipulator so users can grab the radargrams
                    radargram.AddComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();
                    radargram.AddComponent<NearInteractionGrabbable>();
                    Microsoft.MixedReality.Toolkit.UI.ObjectManipulator objectManipulator = radargram.GetComponent<Microsoft.MixedReality.Toolkit.UI.ObjectManipulator>();

                    //// Test if this is the code for generating radargram
                    //// Debug:
                    //Debug.Log("Radargram Test Add Component Network!!");
                    //radargram.AddComponent<NetworkKinematicGrabbable>();
                    //radargram.AddComponent<NetworkObject>();
                    //radargram.AddComponent<KinematicGrabbable>();
                    //radargram.AddComponent<NetworkTransform>();
                    //Debug.Log("Add Component Success!");


                    objectManipulator.enabled = true;

                    // Link the parent to the menu
                    script.Menu = GameObject.Find("Menu");

                    // Create and place the radar mark for the minimap
                    Vector3 position = meshForward.transform.position + meshForward.transform.localPosition; // TODO: this
                                                                                                             //GameObject mark = Instantiate(radarMark, position, Quaternion.identity, parent.transform);

                    //GameObject markObj3D = Instantiate(MarkObj3D, position, Quaternion.identity, radargram.transform);
                    GameObject markObj3D = Instantiate(MarkObj3D, radargram.transform);
                    markObj3D.transform.localPosition = Vector3.zero;
                }

                //Drop everything onto the DEM -- this should correlate with the DEM position
                Container.transform.localPosition = new Vector3(-10f, 0f, 10f);
                foreach (var obj in meshes)
                {
                    Destroy(obj);
                }

            }

            internal UnityEngine.Object[] ExtractMeshes(GltfImport gltfImport)
            {
                // Implement the IInstantiator interface methods to extract meshes
                // You can use gltfImport.Meshes and gltfImport.MeshResults to access mesh data

                // Loop through mesh results
                Mesh[] myMeshes = gltfImport.GetMeshes();
                List<UnityEngine.Object> meshes = new List<UnityEngine.Object>();

                for (int i = 0; i < myMeshes.Length; i++)
                {
                    // Example: Instantiate a Unity GameObject for each mesh
                    Mesh mesh = myMeshes[i];
                    int dotIndex = mesh.name.IndexOf('.');
                    if (dotIndex != -1)
                    {
                        mesh.name = mesh.name.Substring(0, dotIndex);
                    }
                    if (mesh.name.StartsWith("Data_20100324"))
                    {
                        GameObject go = new GameObject(mesh.name);
                        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
                        meshFilter.sharedMesh = mesh;
                        MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
                        meshRenderer.material = gltfImport.GetMaterial(i);

                        // Terry: Adding NetworkObject to all these meshes aka Children of the OBJ
                        // Debug:
                        Debug.Log("Adding NetworkObject to all these meshes.");
                        go.AddComponent<NetworkObject>();
                        go.AddComponent<NetworkKinematicGrabbable>();
                        go.AddComponent<KinematicGrabbable>();
                        go.AddComponent<NetworkTransform>();
                        Debug.Log("Adding component to children success!");

                        // Rotate the texture 90 degrees to the left
                        // this is basically swapping out the original .glb texture and using new png images instead
                        string imgName = mesh.name + ".png";
                        string path = Path.Combine("HorizontalRadar", imgName);
                        byte[] fileData = BetterStreamingAssets.ReadAllBytes(path);
                        Texture2D radarimg = new Texture2D(meshRenderer.material.mainTexture.width, meshRenderer.material.mainTexture.height, TextureFormat.RGBA32, 1, false);
                        radarimg.LoadImage(fileData);
                        meshRenderer.material.mainTexture = rotateTexture(radarimg, false);
                        radarimg.Apply();

                        meshRenderer.material.mainTexture.filterMode = FilterMode.Bilinear;

                        // Rotate texture 90 degrees to the right by adjusting UV coordinates
                        // this is easy and quick but BAD because then line picking doesn't work since the uvs are wrong
                        // Vector2[] uvs = mesh.uv;
                        // for (int j = 0; j < uvs.Length; j++)
                        // {
                        //     uvs[j] = new Vector2(uvs[j].y, 1 - uvs[j].x);
                        // }
                        // mesh.uv = uvs;

                        // Set the rendering mode to "Opaque" if the material doesn't contain transparency
                        // This is necessary for the radar to render correctly, otherwise materials in the back
                        // will appear in the front probably becuase of some transparency problem
                        if (!meshRenderer.material.shader.name.Contains("Transparent"))
                        {
                            meshRenderer.material.SetFloat("_Mode", 0);
                            meshRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                            meshRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                            meshRenderer.material.SetInt("_ZWrite", 1);
                            meshRenderer.material.DisableKeyword("_ALPHATEST_ON");
                            meshRenderer.material.DisableKeyword("_ALPHABLEND_ON");
                            meshRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                            meshRenderer.material.renderQueue = -1;
                        }

                        meshRenderer.receiveShadows = true;
                        meshFilter.sharedMesh.RecalculateNormals();
                        // Add more customization as needed
                        meshes.Add(go);
                    }
                }

                return meshes.ToArray();
            }
            Texture2D rotateTexture(Texture2D originalTexture, bool clockwise)
            {
                Color32[] original = originalTexture.GetPixels32();
                Color32[] rotated = new Color32[original.Length];
                int w = originalTexture.width;
                int h = originalTexture.height;

                int iRotated, iOriginal;

                for (int j = 0; j < h; ++j)
                {
                    for (int i = 0; i < w; ++i)
                    {
                        iRotated = (i + 1) * h - j - 1;
                        iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                        rotated[iRotated] = original[iOriginal];
                    }
                }

                Texture2D rotatedTexture = new Texture2D(h, w);
                rotatedTexture.SetPixels32(rotated);
                rotatedTexture.Apply();
                return rotatedTexture;
            }


            public GameObject[] createRadargramObjects(UnityEngine.Object obj)
            {
                // Select mesh
                GameObject meshForward = Instantiate(obj as GameObject);
                GameObject meshBackward = Instantiate(obj as GameObject);

                // Texture the other side of the mesh
                MeshFilter meshFilter = meshBackward.GetComponent<MeshFilter>();
                Mesh meshUnderlying = meshFilter.mesh;
                int[] triangles = meshUnderlying.triangles;
                for (int j = 0; j < triangles.Length; j += 3)
                {
                    int temp = triangles[j];
                    triangles[j] = triangles[j + 1];
                    triangles[j + 1] = temp;
                }
                meshUnderlying.triangles = triangles;

                // Name meshes
                meshForward.name = meshForward.name.Replace("(Clone)", "").Trim();
                meshBackward.name = "_" + meshForward.name;

                // Return meshes
                GameObject[] meshes = new GameObject[2];
                meshes[0] = meshForward;
                meshes[1] = meshBackward;
                return meshes;
            }

            public Dictionary<string, GameObject> createPolylineObjects(string line_id)
            {
                Debug.LogWarning("get into create polyline");
                // Load the polyline file
                // this works for the streaming assets folder!!
                string filename = "FlightLine_" + line_id + ".obj";
                byte[] data = BetterStreamingAssets.ReadAllBytes(filename);
                string allText = System.Text.Encoding.Default.GetString(data);

                //string allText = File.ReadAllText(path);

                // Split up the text by object definition
                Dictionary<string, GameObject> polylines = new Dictionary<string, GameObject>();
                string[] objects = allText.Split("\no ");
                string key = null;
                Debug.LogWarning("before foreach create polyline");
                foreach (string objectText in objects)
                {

                    // Ensure we're looking at an object definition
                    if (Regex.Matches(objectText, "\nv ").Count == 0) continue;

                    // Instantiate the Game Object
                    GameObject line = Instantiate(gridLine);
                    Debug.LogWarning("finished  create polyline reading line");

                    // Create an array to store the vertices as per the .obj specification
                    Vector3[] vertices = new Vector3[Regex.Matches(objectText, "v ").Count];
                    int v = 0;

                    // Read the file one line at a time
                    string[] text = objectText.Split('\n');
                    for (int i = 0; i < text.Length; i++)
                    {
                        string textline = text[i];

                        // Set the key
                        if (key is null)
                        {
                            key = textline.Trim().Substring(textline.IndexOf('_', textline.Length - 5));
                            continue;
                        }

                        // Vertex
                        if (textline.StartsWith("v "))
                        {
                            // Extract coordinates as floats
                            string[] vertexComponents = textline.Split(' ');
                            float x = float.Parse(vertexComponents[1]);
                            float y = float.Parse(vertexComponents[3]); // because unity and blender have different y/z
                            float z = float.Parse(vertexComponents[2]);

                            // Store the coordinates
                            Vector3 vertex = new Vector3(x, y, z);
                            vertices[v++] = vertex;
                        }
                    }

                    // Apply rendering
                    LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
                    lineRenderer.positionCount = vertices.Length;
                    lineRenderer.SetPositions(vertices);

                    // Add material
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                    Debug.LogWarning("finished  create polyline material");
                    // Create collider
                    for (int i = 1; i < vertices.Length; i++)
                    {
                        // Calculate the line segment
                        Vector3 a = vertices[i - 1];
                        Vector3 b = vertices[i];

                        // Add the collider
                        BoxCollider collider = line.AddComponent<BoxCollider>();
                        collider.isTrigger = true;

                        // Set the collider bounds
                        collider.center = (a + b) / 2f;
                        collider.size = new Vector3(
                            Math.Max(Mathf.Abs(a.x - b.x), 0.2f),
                            Math.Max(Mathf.Abs(a.y - b.y), 0.2f),
                            Math.Max(Mathf.Abs(a.z - b.z), 0.2f)
                        );
                    }

                    // Store the polyline
                    polylines.Add(key, line);

                    // Reset the key
                    key = null;

                }

                // Finish up
                Destroy(gridLine);
                return polylines;
            }

    }

                */
        #endregion LoadFlightLines


        #region Unused INetworkRunnerCallbacks 

        // public void OnInput(NetworkRunner runner, NetworkInput input) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        #endregion
    }

}
