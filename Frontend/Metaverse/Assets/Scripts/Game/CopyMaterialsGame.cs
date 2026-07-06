using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CopyMaterialsGame : MonoBehaviour
{
    [HideInInspector] public GameObject sourceModel;
    public GameObject targetModel;

    Transform FindChildByName(Transform parent, string name)
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
    public void CopyFaceMaterial()
    {
        //Get the materials from the character's Face
        Transform sourceFace = FindChildByName(sourceModel.transform, "Face");
        Transform targetFace = FindChildByName(targetModel.transform, "Face");
        SkinnedMeshRenderer sourceMesh = sourceFace.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer targetMesh = targetFace.GetComponent<SkinnedMeshRenderer>();
        Material[] sourceMaterials = sourceMesh.sharedMaterials;
        Material[] targetMaterials = targetMesh.sharedMaterials;

        for (int i = 0; i < targetMaterials.Length; i++)
        {
            //We don't want any eye materials. Just everything else except the eyes
            if (targetMaterials[i].name.Contains("EyeWhite") ||
                targetMaterials[i].name.Contains("EyeIris") ||
                targetMaterials[i].name.Contains("EyeHighlight") ||
                targetMaterials[i].name.Contains("EyeExtra") ||
                targetMaterials[i].name.Contains("FaceEyeline") ||
                targetMaterials[i].name.Contains("FaceEyelash"))
            {
                continue;
            }
            else
            {
                for (int j = 0; j < sourceMaterials.Length; j++)
                {
                    //It's important to skip the first F/M letter in the material name
                    if (targetMaterials[i].name.Substring(11) ==
                        sourceMaterials[j].name.Substring(11))
                    {
                        targetMaterials[i] = sourceMaterials[j];
                        break;
                    }
                    else if (j == sourceMaterials.Length - 1)
                    {
                        Debug.Log("Name match is not found!");
                    }
                }
            }
        }

        //Set the new List as materials
        targetMesh.materials = targetMaterials;
    }

    public void CopyEyesMaterial()
    {
        //Get the materials
        Transform sourceFace = FindChildByName(sourceModel.transform, "Face");
        Transform targetFace = FindChildByName(targetModel.transform, "Face");
        SkinnedMeshRenderer sourceMesh = sourceFace.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer targetMesh = targetFace.GetComponent<SkinnedMeshRenderer>();
        Material[] sourceMaterials = sourceMesh.sharedMaterials;
        Material[] targetMaterials = targetMesh.sharedMaterials;

        //Set the Target materials as the Source material
        //This keeps the order of the material in the list the same, very important
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            //We only want all eyes materials to be swapped
            if (!targetMaterials[i].name.Contains("EyeWhite") &&
                !targetMaterials[i].name.Contains("EyeIris") &&
                !targetMaterials[i].name.Contains("EyeHighlight") &&
                !targetMaterials[i].name.Contains("EyeExtra") &&
                !targetMaterials[i].name.Contains("FaceEyeline") &&
                !targetMaterials[i].name.Contains("FaceEyelash"))
            {
                continue;
            }
            else
            {
                for (int j = 0; j < sourceMaterials.Length; j++)
                {
                    //It's important to skip the first F/M letter in the material name
                    if (targetMaterials[i].name.Substring(11) ==
                        sourceMaterials[j].name.Substring(11))
                    {
                        targetMaterials[i] = sourceMaterials[j];
                        break;
                    }
                    else if (j == sourceMaterials.Length - 1)
                    {
                        Debug.Log("Name match is not found!");
                    }
                }
            }
        }

        //Set the new List as materials
        targetMesh.materials = targetMaterials;
    }

    public void CopyBodySkinMaterial()
    {
        //Get the materials
        Transform sourceFace = FindChildByName(sourceModel.transform, "Body");
        Transform targetFace = FindChildByName(targetModel.transform, "Body");
        SkinnedMeshRenderer sourceMesh = sourceFace.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer targetMesh = targetFace.GetComponent<SkinnedMeshRenderer>();
        Material[] sourceMaterials = sourceMesh.sharedMaterials;
        Material[] targetMaterials = targetMesh.sharedMaterials;

        //Set the Target materials as the Source material
        //This keeps the order of the material in the list, very important
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            //We don't want any eye materials. Just everything else except the eyes
            if (!targetMaterials[i].name.Contains("Body"))
            {
                continue;
            }
            else
            {
                for (int j = 0; j < sourceMaterials.Length; j++)
                {
                    //It's important to skip the first F/M letter in the material name
                    if (targetMaterials[i].name.Substring(11) ==
                        sourceMaterials[j].name.Substring(11))
                    {
                        targetMaterials[i] = sourceMaterials[j];
                        break;
                    }
                    else if (j == sourceMaterials.Length - 1)
                    {
                        Debug.Log("Name match is not found!");
                    }
                }
            }
        }

        //Set the new List as materials
        targetMesh.materials = targetMaterials;
    }

    public void CopyClothesMaterial()
    {
        //Get the materials
        Transform sourceFace = FindChildByName(sourceModel.transform, "Body");
        Transform targetFace = FindChildByName(targetModel.transform, "Body");
        SkinnedMeshRenderer sourceMesh = sourceFace.GetComponent<SkinnedMeshRenderer>();
        SkinnedMeshRenderer targetMesh = targetFace.GetComponent<SkinnedMeshRenderer>();
        Material[] sourceMaterials = sourceMesh.sharedMaterials;
        Material[] targetMaterials = targetMesh.sharedMaterials;

        //Set the Target materials as the Source material
        //This keeps the order of the material in the list, very important
        for (int i = 0; i < targetMaterials.Length; i++)
        {
            //We don't want any eye materials. Just everything else except the eyes
            if (!targetMaterials[i].name.Contains("Onepiece") &&
                !targetMaterials[i].name.Contains("Tops") &&
                !targetMaterials[i].name.Contains("Bottoms") &&
                !targetMaterials[i].name.Contains("Accessory") &&
                !targetMaterials[i].name.Contains("Shoes"))
            {
                continue;
            }
            else
            {
                for (int j = 0; j < sourceMaterials.Length; j++)
                {
                    //It's important to skip the first F/M letter in the material name
                    //There are just way too many variations of tops and one-piece
                    if (targetMaterials[i].name.Substring(11) == sourceMaterials[j].name.Substring(11) ||
                        (targetMaterials[i].name.Contains("Onepiece") && sourceMaterials[j].name.Contains("Tops")) ||
                        (targetMaterials[i].name.Contains("Tops") && sourceMaterials[j].name.Contains("Onepiece")))
                    {
                        targetMaterials[i] = sourceMaterials[j];
                        break;
                    }
                    else if (j == sourceMaterials.Length - 1)
                    {
                        Debug.Log("Name match is not found!");
                    }
                }
            }
        }

        //Set the new List as materials
        targetMesh.materials = targetMaterials;
    }
}

