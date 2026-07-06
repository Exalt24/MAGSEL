using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseHairSets : MonoBehaviour
{
    // Reference to the target model (the model where hair should be activated)
    public GameObject targetModel;

    // List to store the hair models
    private GameObject[] customHairs;

    void Start()
    {
        // Make sure targetModel is assigned
        if (targetModel != null)
        {
            // Find and store all the custom hairs when the script starts
            FindCustomHairs();
        }
        else
        {
            Debug.LogError("Target model is not assigned!");
        }
    }

    // This method will find the "Custom Hairs" under the targetModel
    private void FindCustomHairs()
    {
        // Ensure that targetModel is assigned
        if (targetModel == null)
        {
            Debug.LogError("Target model is not assigned!");
            return;
        }

        // Find the "Custom Hairs" GameObject under the targetModel
        Transform customHairsParent = FindChildByName(targetModel.transform, "Custom Hairs");

        if (customHairsParent == null)
        {
            Debug.LogError("Custom Hairs not found in the target model hierarchy!");
            return;
        }

        // Get all children of the "Custom Hairs" GameObject
        customHairs = new GameObject[customHairsParent.childCount];

        //Disable all the custom hairs except the first one
        for (int i = 0; i < customHairsParent.childCount; i++)
        {
            customHairs[i] = customHairsParent.GetChild(i).gameObject;
            customHairs[i].SetActive(false);
        }

        // Activate the first hair
        customHairs[0].SetActive(true);
    }

    public void ShowHair(int index)
    {
        // Ensure that the index is valid
        if (index < 0 || index >= customHairs.Length)
        {
            Debug.LogError("Index out of range: " + index);
            return;
        }

        // Deactivate all hairs
        for (int i = 0; i < customHairs.Length; i++)
        {
            customHairs[i].SetActive(false);
        }

        // Activate the hair at the specified index
        customHairs[index].SetActive(true);
    }

    // Recursive function to find a child by name in the hierarchy
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
