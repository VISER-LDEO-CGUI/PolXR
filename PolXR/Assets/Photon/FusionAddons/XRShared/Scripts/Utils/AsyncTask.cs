using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


namespace Fusion.XR.Shared
{
    /***
     * 
     * AsyncTask can be used to replaces the System.Threading.Tasks.Task.Delay() for WebGL builds.
     * It relies on Task.Yield which is compatible for WebGL builds.
     * 
     **/
    public static class AsyncTask
    {
        public static async Task Delay(int milliseconds)
        {
#if !UNITY_WEBGL
            await Task.Delay(milliseconds);
#else       
            // Unity 2021 do NOT support Task.Delay() in WebGL
            float startTime = Time.realtimeSinceStartup;
            float delay = (float)milliseconds / 1000f;

            while (Time.realtimeSinceStartup - startTime < delay)
            {
                // Wait for the delay time to pass
                await Task.Yield();
            }
#endif
        }
    }
}
