using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Unity.VisualScripting;

public class Controller : NetworkBehaviour
{
    public Camera mainCamera;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            GameObject cameraObject = GameObject.Find("MainCamera");
            if (cameraObject != null)
            {
                mainCamera = cameraObject.GetComponent<Camera>();
            }
            else
            {
                Debug.LogError("Main Camera not found in the scene.");
            }
            Debug.Log("Client started and is owner of the object.");
            mainCamera.enabled = false;
        }
        else
        {
            Debug.Log("Client started but not owner of the object.");
            Transform cameraTransform = transform.Find("Camera");
            Transform playerFollowCamera = transform.Find("PlayerFollowCamera");
            if (cameraTransform != null && playerFollowCamera != null)
            {
                cameraTransform.gameObject.SetActive(false);
                playerFollowCamera.gameObject.SetActive(false);
            }
            else 
            {
                Debug.LogWarning("PlayerFollowCamera GameObject not found in the hierarchy.");
            }
        }
    }

    private Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
}
