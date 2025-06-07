using UnityEngine;

[CreateAssetMenu(fileName = "New Soundtrack", menuName = "SCPCBR/Soundtrack", order = 1)]
public class Soundtrack : ScriptableObject
{
    public string soundtrackName = "New Soundtrack";
    public MusicTrack[] tracks;

    [System.Serializable]
    public class MusicTrack {
        public string trackName = "New Track";
        public AudioClip clip;
    }
}