public interface ISaveable {
    void CaptureState(SaveData data);
    void LoadState(SaveData data);
}