using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "SCPCBR/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject itemPrefab;

    public AudioClip pickUpSound;
    public AudioClip useSound;

    public bool isKeycard;
    public int keycardLevel;
}