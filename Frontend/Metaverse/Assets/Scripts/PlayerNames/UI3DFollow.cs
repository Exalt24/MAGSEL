using UnityEngine;

public class UIFollow3DObject : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        // Use Camera.main to get the local player's camera.
        if (Camera.main != null)
        {
            mainCam = Camera.main;
            Debug.Log("Found main camera: " + mainCam.name);
        }
        else
        {
            Debug.LogWarning("Could not find the main camera.");
        }
    }
    
    void Update()
    {
        if (mainCam == null)
            return;

        // Billboard effect: rotate to face the local camera.
        Vector3 direction = transform.position - mainCam.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation * Quaternion.Euler(0, 180f, 0);
    }
}
