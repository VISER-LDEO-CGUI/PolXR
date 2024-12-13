using System.Collections.Generic;
using System;
using Fusion;
using GLTFast;
using Oculus.Platform.Samples.VrHoops;
using UnityEngine;
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

public class BakingObjectProvider : NetworkObjectProviderDefault
{
    // For this sample, we are using very high flag values to indicate custom.
    // Other values will fall through the default instantiation handling.
    public const int CUSTOM_PREFAB_FLAG = 100000;

    // The NetworkObjectBaker class can be reused and is Runner independent.
    private static NetworkObjectBaker _baker;
    private static NetworkObjectBaker Baker => _baker ??= new NetworkObjectBaker();

    public Transform Container;
    //public GameObject radarMark;
    public GameObject DEM;
    public GameObject gridLine;

    public GameObject MarkObj3D;

    public override NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
    {
        // Detect if this is a custom spawn by its high prefabID value we are passing.
        // The Spawn call will need to pass this value instead of a prefab.
        Debug.LogWarning("Check condition");
        if (context.PrefabId.RawValue == CUSTOM_PREFAB_FLAG)
        {
            Debug.LogWarning("Passed Condition");
            //BetterStreamingAssets.Initialize();
            //GameObject.Find("RadarImageContainer").GetComponent<LoadFlightLines>().LoadFlightLine("20100324_01");
            ////LoadFlightLine("20100324_01"); // TODO: replace with menu option
            //Debug.Log("Loaded flight line");

            // Copy all codes from loadflightline to here


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
            var success = gltf.Load(data);
            Debug.LogWarning("Loaded data");
            UnityEngine.Object[] meshes;


            meshes = ExtractMeshes(gltf);


            NetworkPrefabId parentID = new NetworkPrefabId();
            NetworkPrefabId radargramID = new NetworkPrefabId();

            Debug.LogWarning("mesh length" + meshes.Length);
            Debug.LogWarning("HERERERERERERER" + meshes.Length);

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
                GameObject go = new GameObject(parentName);


                Debug.LogWarning("spawn parent prefab!");
                // var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                var no = go.AddComponent<NetworkObject>();
                go.AddComponent<NetworkTransform>();
                go.name = $"Custom Object";

                // Baking is required for the NetworkObject to be valid for spawning.
                Baker.Bake(go);

                // Move the object to the applicable Runner Scene/PhysicsScene/DontDestroyOnLoad
                // These implementations exist in the INetworkSceneManager assigned to the runner.
                if (context.DontDestroyOnLoad)
                {
                    runner.MakeDontDestroyOnLoad(go);
                }
                else
                {
                    runner.MoveToRunnerScene(go);
                }

                // We are finished. Return the NetworkObject and report success.
                result = no;
                return NetworkObjectAcquireResult.Success;
            }
        }
        result = null;
        return base.AcquirePrefabInstance(runner, context, out result);
    }



        /*
            // CTL Team Change:
            parentID.RawValue = 100000;
                NetworkObject parent = runner.Spawn(parentID, position: new Vector3(0, 0, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: player, (runner, obj) =>
                {
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
                NetworkObject radargram = runner.Spawn(radargramID, position: new Vector3(0, 0, 0), rotation: new Quaternion(0, 0, 0, 0), inputAuthority: player, (runner, obj) =>
                {
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
   


            //    Debug.LogWarning("spawn customized prefab!");
            //    var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //    var no = go.AddComponent<NetworkObject>();
            //    go.AddComponent<NetworkTransform>();
            //    go.name = $"Custom Object";

            //    // Baking is required for the NetworkObject to be valid for spawning.
            //    Baker.Bake(go);

            //    // Move the object to the applicable Runner Scene/PhysicsScene/DontDestroyOnLoad
            //    // These implementations exist in the INetworkSceneManager assigned to the runner.
            //    if (context.DontDestroyOnLoad)
            //    {
            //        runner.MakeDontDestroyOnLoad(go);
            //    }
            //    else
            //    {
            //        runner.MoveToRunnerScene(go);
            //    }

            //    // We are finished. Return the NetworkObject and report success.
            //    result = no;
            //    return NetworkObjectAcquireResult.Success;
            //}

            // For all other spawns, use the default spawning.
            //return base.AcquirePrefabInstance(runner, context, out result);
        }
        */
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

                //// Terry: Adding NetworkObject to all these meshes aka Children of the OBJ
                //// Debug:
                //Debug.Log("Adding NetworkObject to all these meshes.");
                //go.AddComponent<NetworkObject>();
                //go.AddComponent<NetworkKinematicGrabbable>();
                //go.AddComponent<KinematicGrabbable>();
                //go.AddComponent<NetworkTransform>();
                //Debug.Log("Adding component to children success!");

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

}






