using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public interface IXRAction
{
    public string Description { get; }
    public string ImageName { get; }
    public string CategoryName { get; }
    public bool IsActionVisible { get; }
    // Lower values appear first
    public int Weight { get; }
    public void Trigger();
}

public interface IXRRootAction : IXRAction
{
    // true if this action can create the root element of its category
    public bool IsInstalled { get; }
}

public interface IXRActionSelecter
{
    public bool TrySelect();
}

[InitializeOnLoad]
public class XRActionsManager
{
    public interface IListener
    {
        void OnActionsChange(IXRAction action);
    }
    public interface ILogListener
    {
        void OnLogsChange();
    }

    public struct XRActionsLogEntry {
        public string text;
        public IXRActionSelecter selecter;
        public UnityEngine.Object associatedObject;
        public string imageName;
        public bool forceExitPrefabMode;
        public float additionTime;
        public bool isLatestAddition;
    }


    public const string RUNNER_CATEGORY = "Runner";
    public const string HARDWARERIG_CATEGORY = "HardwareRig";
    public const string NETWORKRIG_CATEGORY = "NetworkRig";
    public const string SCENE_OBJECT_CATEGORY = "SceneObjects";

    static Dictionary<string, List<IXRAction>> ActionByCategory = new Dictionary<string, List<IXRAction>>();

    public static List<XRActionsLogEntry> Logs = new List<XRActionsLogEntry>();

    public static List<IListener> Listeners = new List<IListener>();
    public static List<ILogListener> LogListeners = new List<ILogListener>();
    static XRActionsManager()
    {
    }

    public static void ClearLogs()
    {
        Logs.Clear();
        foreach (var listener in LogListeners)
        {
            listener.OnLogsChange();
        }
    }

    public static void AddLog(string text, IXRActionSelecter selecter = null, UnityEngine.Object associatedObject = null, string imageName = null, bool forceExitPrefabMode = false, float additiontime = -1)
    {
        if(additiontime == -1)
        {
            additiontime = Time.time;
        }
        for(int i = 0; i < Logs.Count;i++)
        {
            if(Logs[i].additionTime != additiontime)
            {
                var log = Logs[i];
                log.isLatestAddition = false;
                Logs[i] = log;
            }
        }
        Logs.Add(new XRActionsManager.XRActionsLogEntry { text = text, selecter = selecter, associatedObject = associatedObject, imageName = imageName, forceExitPrefabMode = forceExitPrefabMode, additionTime = additiontime, isLatestAddition = true }); ;
        foreach (var listener in LogListeners)
        {
            listener.OnLogsChange();
        }
    }

    public static void RegisterAction(IXRAction action)
    {
        if(ActionByCategory.ContainsKey(action.CategoryName) == false)
        {
            ActionByCategory[action.CategoryName] = new List<IXRAction>();
        }
        if (ActionByCategory[action.CategoryName].Contains(action)) return;
        ActionByCategory[action.CategoryName].Add(action);
        ActionByCategory[action.CategoryName].Sort(delegate (IXRAction a1, IXRAction a2)
        {
            if (a1 is IXRRootAction && a2 is not IXRRootAction) return -1;
            if (a2 is IXRRootAction && a1 is not IXRRootAction) return 1;
            return a1.Weight.CompareTo(a2.Weight);
        });
        foreach(var listener in Listeners)
        {
            listener.OnActionsChange(action);
        }
    }

    public static void RegisterListener(IListener listener)
    {
        if (Listeners.Contains(listener)) return;
        Listeners.Add(listener);
    }

    public static void UnregisterListener(IListener listener)
    {
        if (Listeners.Contains(listener) == false) return;
        Listeners.Remove(listener);
    }

    public static void RegisterLogListener(ILogListener listener)
    {
        if (LogListeners.Contains(listener)) return;
        LogListeners.Add(listener);
    }

    public static void UnregisterLogListener(ILogListener listener)
    {
        if (LogListeners.Contains(listener) == false) return;
        LogListeners.Remove(listener);
    }

    public static IXRRootAction RootActionForCategory(string category)
    {
        foreach(var action in ActionsInCategory(category))
        {
            if (action is IXRRootAction rootAction) return rootAction;
        }
        return null;
    }

    public static List<IXRAction> ActionsInCategory(string category)
    {
        if (ActionByCategory.ContainsKey(category) == false)
        {
            return new List<IXRAction>();
        }
        return ActionByCategory[category];
    }
}

