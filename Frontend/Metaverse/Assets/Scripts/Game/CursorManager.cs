using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // Ensure the cursor is always visible
        Cursor.visible = true;
        // Unlock the cursor from the center (if it was locked)
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {
        // In case the cursor is locked/unlocked dynamically in your game,
        // you can check and force it to be visible and unlocked every frame.
        Cursor.visible = true;  // Ensures the cursor remains visible
        Cursor.lockState = CursorLockMode.None;  // Ensures the cursor is not locked
    }
}
