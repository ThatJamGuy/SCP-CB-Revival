using UnityEngine;
using System.Collections;
using FMODUnity;

/// <summary>
/// Script that controls the head object for the Untitled 2004 version of SCP-173
/// </summary>
public class SCP_173_Head : MonoBehaviour {

    [Header("SCP-173 Head Movement Settings")]
    [Space(20)]
    [SerializeField] private int headMovementInterval = 1;
    [SerializeField] private EventReference headMovementSound;

    private Player player;

    private Quaternion previousRotation;
    
    #region Unity Callbacks
    private void Start() {
        // Check if there is a player instance in the scene, and if there isn't we give up now
        if (Player.Instance != null)
            player = Player.Instance;
        else
            return;
        
        // Assuming the checks have passed, start the forever repeating head movement stuff
        StartCoroutine(HeadMovement());
    }
    #endregion

    #region Private Coroutines
    private IEnumerator HeadMovement() {
        // Infinite loop
        while (true) {
            // Set's the previous rotation value to the current rotation of the head
            previousRotation = transform.rotation;
            
            // Wait for as long as I or you told it to
            yield return new WaitForSeconds(headMovementInterval);
            
            // Look at the position of the player and add 0.5 on the y-axis to try and level it out with the camera
            // Then adjust the rotation of the head to look at the player so that he's not mogging D-9341
            transform.LookAt(player.transform.position + new Vector3(0f, 0.5f, 0f));
            transform.rotation *= Quaternion.FromToRotation(Vector3.left, Vector3.forward);
            
            // If the current head rotation doesn't match the last one, then play the head movement sound
            if (transform.rotation != previousRotation)
                AudioManager.PlayOneShot(headMovementSound, transform.position);
        }
    }
    #endregion
}