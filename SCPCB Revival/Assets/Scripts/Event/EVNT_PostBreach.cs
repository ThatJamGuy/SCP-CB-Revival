using AOT;
using FMOD.Studio;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;

public class EVNT_PostBreach : MonoBehaviour {
    EventInstance eventInstance;
    GCHandle callbackHandle;

    readonly System.Collections.Generic.Queue<string> markerQueue = new System.Collections.Generic.Queue<string>();

    private void OnEnable() {
        DevConsole.Instance.Add("startevent_postbreach", () => TriggerPostBreachEvent());
    }

    private void OnDisable() {
        if (eventInstance.isValid()) {
            eventInstance.stop(STOP_MODE.ALLOWFADEOUT);
            eventInstance.release();
        }

        if (callbackHandle.IsAllocated)
            callbackHandle.Free();
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
        Debug.Log("Post-breach event triggered.");

        PlayerAccessor.instance.allowInput = false;
        StartCoroutine(PlayerInputRoutine());

        CanvasInstance.instance.introScreenStuff.SetActive(true);

        MusicManager.instance.SetMusicState(MusicState.CreepyMusic03);

        eventInstance = FMODUnity.RuntimeManager.CreateInstance(FMODEvents.instance.alarm2);

        callbackHandle = GCHandle.Alloc(this);
        eventInstance.setUserData(GCHandle.ToIntPtr(callbackHandle));
        eventInstance.setCallback(EventCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);

        eventInstance.start();

        AmbienceController.Instance.PlayCommotionEvent();
    }

    public void ShakeCameraLarge() {
        GlobalCameraShake.instance.ShakeCamera(0.2f, 0f, 5f);
    }

    public void ShakeCameraSmall() {
        GlobalCameraShake.instance.ShakeCamera(0.03f, 0f, 2f);
    }

    public void ChangeMusicToLCZ() {
        MusicManager.instance.SetMusicState(MusicState.LCZ);
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
    static FMOD.RESULT EventCallback(
    EVENT_CALLBACK_TYPE type,
    IntPtr instancePtr,
    IntPtr parameterPtr) {
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

    private IEnumerator PlayerInputRoutine() {
        yield return new WaitForSeconds(5f);
        PlayerAccessor.instance.allowInput = true;
    }
}