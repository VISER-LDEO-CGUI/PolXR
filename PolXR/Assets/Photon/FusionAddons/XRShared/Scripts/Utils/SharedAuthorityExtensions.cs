using Fusion;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.XR.Shared
{
    public static class SharedAuthorityExtensions
    {
        /**
         * Request state authority and wait for it to be received
         * Relevant in shared topology only
         */
        public static async Task<bool> WaitForStateAuthority(this NetworkObject o, float maxWaitTime = 8)
        {
            if (o == null)
            {
                Debug.LogError("Null network object");
                return false;
            }
            float waitStartTime = Time.time;
            o.RequestStateAuthority();
            while (!o.HasStateAuthority && (Time.time - waitStartTime) < maxWaitTime)
            {
                await AsyncTask.Delay(1);
            }
            return o.HasStateAuthority;
        }

        public async static Task EnsureHasStateAuthority(this NetworkObject o)
        {
            await o.EnsureHasStateAuthority(PlayerRef.None);
        }

        public async static Task EnsureHasStateAuthority(this NetworkObject o, PlayerRef originalOwner)
        {
            int i = 0;
            const int maxAttempts = 1_000;
            while (i < maxAttempts)
            {
                if (o == null || o.Runner == null)
                {
                    // Object destroyed
                    return;
                }

                if (o != null && o.StateAuthority != null && originalOwner != null && o.StateAuthority != originalOwner)
                {
                    // A new owner has been found
                    break;
                }
                int playerCount = 0;
                PlayerRef lowestPlayerRef = PlayerRef.None;
                foreach (var activePlayer in o.Runner.ActivePlayers)
                {
                    playerCount++;
                    if (lowestPlayerRef == PlayerRef.None || lowestPlayerRef.PlayerId > activePlayer.PlayerId)
                    {
                        lowestPlayerRef = activePlayer;
                    }
                }
                if (lowestPlayerRef == o.Runner.LocalPlayer)
                {
                    o.RequestStateAuthority();
                }
                await AsyncTask.Delay(100);
                i++;
            }
        }
    }
}

