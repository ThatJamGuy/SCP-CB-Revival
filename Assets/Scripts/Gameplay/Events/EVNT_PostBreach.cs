using AOT;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class EVNT_PostBreach : MonoBehaviour {
    [SerializeField] private bool devMode;
    [SerializeField] private EventReference alarm2;

    private readonly System.Collections.Generic.Queue<string> markerQueue = new System.Collections.Generic.Queue<string>();

    private EventInstance eventInstance;
    private GCHandle callbackHandle;

    private void OnDisable() {
        if (eventInstance.isValid()) {
            eventInstance.setUserData(IntPtr.Zero);
            eventInstance.setCallback(null);

            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
            eventInstance.clearHandle();
        }

        if (callbackHandle.IsAllocated)
            callbackHandle.Free();
    }

    private void Start() {
        if (devMode) TriggerPostBreachEvent();
    }

    private void Update() {
        lock (markerQueue) {
            while (markerQueue.Count > 0)
                HandleMarker(markerQueue.Dequeue());
        }
    }

    private void EnqueueMarker(string marker) {
        lock (markerQueue)
            markerQueue.Enqueue(marker);
    }

    public void TriggerPostBreachEvent() {
        MusicManager.Instance.SetTrack(MusicManager.MusicTrack.GeneralHorror03);

        eventInstance = FMODUnity.RuntimeManager.CreateInstance(alarm2);

        callbackHandle = GCHandle.Alloc(this);
        eventInstance.setUserData(GCHandle.ToIntPtr(callbackHandle));
        eventInstance.setCallback(EventCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        eventInstance.start();
    }

    public void ShakeCameraLarge() {
        RevivalRuntimeEngine.Instance.ShakeCamera(0.2f, 0, 5);
    }

    public void ShakeCameraSmall() {
        RevivalRuntimeEngine.Instance.ShakeCamera(0.03f, 0, 2);
    }

    public void ChangeMusicToLCZ() {
        //MusicManager.instance.SetMusicState(MusicState.LCZ);
    }

    private void HandleMarker(string marker) {
        switch (marker) {
            case "Shake_Large":
                ShakeCameraLarge();
                break;
            case "Shake_Small":
                ShakeCameraSmall();
                break;
            case "Music_LCZ":
                ChangeMusicToLCZ();
                break;
        }
    }

    [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT EventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr) {
        if (type != EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            return FMOD.RESULT.OK;

        var marker = (TIMELINE_MARKER_PROPERTIES)
            Marshal.PtrToStructure(parameterPtr,
            typeof(TIMELINE_MARKER_PROPERTIES));

        string markerName = marker.name;

        EventInstance instance = new EventInstance(instancePtr);

        instance.getUserData(out IntPtr userData);

        if (userData != IntPtr.Zero) {
            var handle = GCHandle.FromIntPtr(userData);
            var evt = handle.Target as EVNT_PostBreach;
            evt.EnqueueMarker(markerName);
        }

        return FMOD.RESULT.OK;
    }
}