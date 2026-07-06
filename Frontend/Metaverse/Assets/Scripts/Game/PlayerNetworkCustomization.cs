using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerCustomization : NetworkBehaviour
{
    // Use the working style for SyncVars.
    private readonly SyncVar<int> _faceID = new SyncVar<int>(0);
    private readonly SyncVar<int> _eyesID = new SyncVar<int>(0);
    private readonly SyncVar<int> _skinID = new SyncVar<int>(0);
    private readonly SyncVar<int> _hairID = new SyncVar<int>(0);
    private readonly SyncVar<int> _clothesID = new SyncVar<int>(0);

    [Header("Customization Tools")]
    public CopyMaterialsGame copyMaterials;   // Script to copy materials.
    public ChooseHairSetsGame chooseHairSets;     // Script to handle hair.

    [Header("Models")]
    public List<GameObject> models; // Source models for materials/hair.

    [Header("Avatar Reference")]
    public GameObject finalModel;   // The child ("FinalModel") to be updated.

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameObject chooseHairSetsObject = GameObject.Find("ShowHairSets");
        if (chooseHairSetsObject != null)
        {
            chooseHairSets = chooseHairSetsObject.GetComponent<ChooseHairSetsGame>();
        }
        else
        {
            Debug.LogError("ChooseHairSets object not found in the scene.");
        }

        GameObject copyMaterialsObject = GameObject.Find("Copy Materials");
        if (copyMaterialsObject != null)
        {
            copyMaterials = copyMaterialsObject.GetComponent<CopyMaterialsGame>();
        }
        else
        {
            Debug.LogError("CopyMaterials object not found in the scene.");
        }

        // Subscribe to SyncVar change events.
        _faceID.OnChange += FaceIDChanged;
        _eyesID.OnChange += EyesIDChanged;
        _skinID.OnChange += SkinIDChanged;
        _hairID.OnChange += HairIDChanged;
        _clothesID.OnChange += ClothesIDChanged;

        // If this object is owned by the local player, disable the main camera and send saved customization.
        if (IsOwner)
        {
            CmdUpdateCustomization(
                CharacterDataManager.faceID,
                CharacterDataManager.eyesID,
                CharacterDataManager.skinID,
                CharacterDataManager.hairID,
                CharacterDataManager.clothesID
            );
        }

        // Apply the current customization on all clients.
        ApplyCustomization();
    }

    public override void OnStopClient()
    {
        // Unsubscribe from change events.
        _faceID.OnChange -= FaceIDChanged;
        _eyesID.OnChange -= EyesIDChanged;
        _skinID.OnChange -= SkinIDChanged;
        _hairID.OnChange -= HairIDChanged;
        _clothesID.OnChange -= ClothesIDChanged;
        base.OnStopClient();
    }

    // ServerRPC: Called by the local owner to update customization data on the server.
    [ServerRpc(RequireOwnership = false)]
    public void CmdUpdateCustomization(int newFace, int newEyes, int newSkin, int newHair, int newClothes)
    {
        _faceID.Value = newFace;
        _eyesID.Value = newEyes;
        _skinID.Value = newSkin;
        _hairID.Value = newHair;
        _clothesID.Value = newClothes;
    }

    // Delegate methods for SyncVar changes.
    private void FaceIDChanged(int oldVal, int newVal, bool asServer)
    {
        ApplyCustomization();
    }

    private void EyesIDChanged(int oldVal, int newVal, bool asServer)
    {
        ApplyCustomization();
    }

    private void SkinIDChanged(int oldVal, int newVal, bool asServer)
    {
        ApplyCustomization();
    }

    private void HairIDChanged(int oldVal, int newVal, bool asServer)
    {
        ApplyCustomization();
    }

    private void ClothesIDChanged(int oldVal, int newVal, bool asServer)
    {
        ApplyCustomization();
    }

    // Applies the customization by updating materials and hair on the finalModel.
    void ApplyCustomization()
    {
        if (finalModel == null || models == null || models.Count == 0)
        {
            Debug.LogWarning("Final model or models not assigned.");
            return;
        }

        if (copyMaterials != null)
        {
            copyMaterials.targetModel = finalModel;
            // Update face material.
            copyMaterials.sourceModel = models[_faceID.Value % models.Count];
            copyMaterials.CopyFaceMaterial();

            // Update eyes material.
            copyMaterials.sourceModel = models[_eyesID.Value % models.Count];
            copyMaterials.CopyEyesMaterial();

            // Update body/skin material.
            copyMaterials.sourceModel = models[_skinID.Value % models.Count];
            copyMaterials.CopyBodySkinMaterial();

            // Update clothes material.
            copyMaterials.sourceModel = models[_clothesID.Value % models.Count];
            copyMaterials.CopyClothesMaterial();
        }

        if (chooseHairSets != null)
        {
            chooseHairSets.targetModel = finalModel;
            chooseHairSets.FindCustomHairs();
            chooseHairSets.ShowHair(_hairID.Value % models.Count);
        }
    }
}
