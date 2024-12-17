#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;

namespace Fusion.XRShared.Tools
{
    public class PackagePresenceCheck
    {
        string[] packageNames = null;
        UnityEditor.PackageManager.Requests.ListRequest request;
        public delegate void ResultDelegate(Dictionary<string, UnityEditor.PackageManager.PackageInfo> packageInfoByPackageName);
        ResultDelegate resultCallback;
        public delegate void SingleResultDelegate(UnityEditor.PackageManager.PackageInfo packageInfo);
        SingleResultDelegate singleResultCallback;

        Dictionary<string, UnityEditor.PackageManager.PackageInfo> results = new Dictionary<string, UnityEditor.PackageManager.PackageInfo>();
        public PackagePresenceCheck(string[] packageNames, ResultDelegate resultCallback, bool useOfflineMode = true)
        {
            this.packageNames = packageNames;
            this.resultCallback = resultCallback;
            request = Client.List(offlineMode: useOfflineMode, includeIndirectDependencies: true);
            EditorApplication.update += Progress;
        }

        public PackagePresenceCheck(string packageName, SingleResultDelegate resultCallback, bool useOfflineMode = true)
        {
            this.packageNames = new string[] { packageName };
            this.singleResultCallback = resultCallback;
            request = Client.List(offlineMode: useOfflineMode, includeIndirectDependencies: true);
            EditorApplication.update += Progress;
        }

        void Progress()
        {
            if (request.IsCompleted)
            {
                results = new Dictionary<string, UnityEditor.PackageManager.PackageInfo>();
                if (request.Status == StatusCode.Success)
                {
                    foreach (var info in request.Result)
                    {
                        foreach(var checkedPackageName in packageNames)
                        {
                            if (info.name == checkedPackageName)
                            {
                                results[checkedPackageName] = info;
                                if (singleResultCallback != null)
                                    singleResultCallback(info);
                                break;
                            }
                        }
                    }
                }
                if(resultCallback != null)
                    resultCallback(results);
                EditorApplication.update -= Progress;
            }
        }
    }
}
#endif