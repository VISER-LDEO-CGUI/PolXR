using Fusion;
using UnityEngine;

public class BakingObjectProvider : NetworkObjectProviderDefault
{
    // For this sample, we are using very high flag values to indicate custom.
    // Other values will fall through the default instantiation handling.
    public const int CUSTOM_PREFAB_FLAG = 100000;

    // The NetworkObjectBaker class can be reused and is Runner independent.
    private static NetworkObjectBaker _baker;
    private static NetworkObjectBaker Baker => _baker ??= new NetworkObjectBaker();

    public override NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
    {
        // Detect if this is a custom spawn by its high prefabID value we are passing.
        // The Spawn call will need to pass this value instead of a prefab.
        Debug.LogWarning("Check condition");
        if (context.PrefabId.RawValue == CUSTOM_PREFAB_FLAG)
        {
            //BetterStreamingAssets.Initialize();
            //GameObject.Find("RadarImageContainer").GetComponent<LoadFlightLines>().LoadFlightLine("20100324_01");
            ////LoadFlightLine("20100324_01"); // TODO: replace with menu option
            //Debug.Log("Loaded flight line");

            // Copy all codes from loadflightline to here


            Debug.LogWarning("spawn customized prefab!");
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
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

        // For all other spawns, use the default spawning.
        return base.AcquirePrefabInstance(runner, context, out result);
    }
}