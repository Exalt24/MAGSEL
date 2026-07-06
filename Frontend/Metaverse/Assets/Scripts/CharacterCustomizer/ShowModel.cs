using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowModel : MonoBehaviour
{
    public CopyMaterials copyMaterials;

    [Header("List of Models")]
    public GameObject model;

    public void ShowModelDress()
    {
        model.SetActive(true);
        copyMaterials.targetModel = model;
    }
}