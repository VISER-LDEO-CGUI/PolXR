using Fusion.XR.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Fusion.XR.Shared
{
    /*
     * Allow to limit parralel tasks
     * Mainly use to limit Ready Player Me avatar parralel downloads
     */
    public class PerformanceManager : MonoBehaviour
    {
        public List<TaskToken> processingTasks = new List<TaskToken>();
        public List<TaskToken> waitingTasks = new List<TaskToken>();
        public List<TaskToken> cancelledTasks = new List<TaskToken>();

        public float maxWaitTime = 6;
        public float maxProcessingTime = 5;
        public int numberOfParralelTasks = 1;
        public int availabilityCheckDelayMS = 10;
        public enum TaskKind
        {
            NetworkRequest,
            TextureManipulation
        }

        [System.Serializable]
        public struct TaskToken
        {
            public string id;
            public float creationTime;
            public TaskKind kind;

            public TaskToken(TaskKind taskKind = TaskKind.NetworkRequest)
            {
                creationTime = Time.realtimeSinceStartup;
                kind = taskKind;
                id = $"Task-{kind}-{creationTime}";
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }

        public async Task<TaskToken?> RequestToStartTask(TaskKind kind = TaskKind.NetworkRequest)
        {
            if (processingTasks.Count >= numberOfParralelTasks)
            {
                TaskToken waitToken = new TaskToken(kind);
                //Debug.LogError("[Perf] Waiting to process "+token.id+" ...");
                waitingTasks.Add(waitToken);
                while (processingTasks.Count >= numberOfParralelTasks && !cancelledTasks.Contains(waitToken))
                {
                    await AsyncTask.Delay(availabilityCheckDelayMS);
                }
                //Debug.LogError("[Perf] Available for "+ token.id);
                waitingTasks.Remove(waitToken);
                if (cancelledTasks.Contains(waitToken))
                {
                    cancelledTasks.Remove(waitToken);
                    return null;
                }
            }
            else
            {
                //Debug.LogError("[Perf] Immediate processing");
            }
            TaskToken token = new TaskToken(kind);
            processingTasks.Add(token);
            return token;
        }

        public void TaskCompleted(TaskToken token)
        {
            processingTasks.Remove(token);
        }
        public void TaskCompleted(TaskToken? token)
        {
            if (token == null) return;
            TaskCompleted(token.GetValueOrDefault());
        }

        private void Update()
        {
            foreach (var token in waitingTasks)
            {
                if ((Time.time - token.creationTime) > maxWaitTime && !cancelledTasks.Contains(token))
                {
                    cancelledTasks.Add(token);
                }
            }
            foreach (var token in processingTasks)
            {
                if ((Time.time - token.creationTime) > maxProcessingTime)
                {
                    Debug.LogWarning("A blocking task has timed out. Allowing next tasks to process.");
                    processingTasks.Remove(token);
                    break;
                }
            }
        }
    }

}
