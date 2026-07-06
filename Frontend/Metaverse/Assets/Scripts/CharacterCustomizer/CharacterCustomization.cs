using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using FishNet.Demo.AdditiveScenes;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class CharacterDataManager {
    public static int faceID = 0;
    public static int eyesID = 0;
    public static int skinID = 0;
    public static int hairID = 0;
    public static int clothesID = 0;
}


public class CharacterCustomization : MonoBehaviour {

    public CopyMaterials copyMaterials;
    public ChooseHairSets chooseHairSets;
    private int faceID;
    private int eyesID;
    private int skinID;
    private int hairID;
    private int clothesID;

    [SerializeField] private TextMeshProUGUI faceText;
    [SerializeField] private TextMeshProUGUI eyesText;
    [SerializeField] private TextMeshProUGUI skinText;
    [SerializeField] private TextMeshProUGUI hairText;
    [SerializeField] private TextMeshProUGUI clothesText;

    // Camera Zoom Positions for each part
    private Vector3 facePosition = new Vector3(-0.882f, 1.358f, -8.098f);
    private Vector3 eyesPosition = new Vector3(-0.882f, 1.358f, -8.098f);
    private Vector3 skinPosition = new Vector3(-0.7f, 0.81f, -8.855f);
    private Vector3 hairPosition = new Vector3(-0.882f, 1.358f, -8.098f);
    private Vector3 clothesPosition = new Vector3(-0.7f, 0.81f, -8.855f);

    [SerializeField] private GameObject model;  // Assign the model GameObject in Unity Inspector
    [Header("List of all premade models to copy materials from")]
    public List<GameObject> models;

     private Camera mainCamera;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float originalCameraFOV;
    private void Awake() {
        faceID = 0;
        eyesID = 0;
        skinID = 0;
        hairID = 0;
        clothesID = 0;
    }

    private void Start() {

        mainCamera = Camera.main;
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;
        originalCameraFOV = mainCamera.fieldOfView;

        copyMaterials.sourceModel = models[faceID];
        copyMaterials.CopyFaceMaterial();
        copyMaterials.sourceModel = models[eyesID];
        copyMaterials.CopyEyesMaterial();
        copyMaterials.sourceModel = models[skinID];
        copyMaterials.CopyBodySkinMaterial();
        copyMaterials.sourceModel = models[clothesID];
        copyMaterials.CopyClothesMaterial();
        UpdateText();
    }

    public void UpdateText() {
        faceText.text = faceID.ToString();
        eyesText.text = eyesID.ToString();
        skinText.text = skinID.ToString();
        hairText.text = hairID.ToString();
        clothesText.text = clothesID.ToString();
    }


        // Zoom in to a specific part of the character (e.g., face, eyes, etc.)
    private void ZoomIn(Vector3 targetPosition, float targetFOV = 30f, float duration = 8f)
    {
        // Zoom the camera towards the target position and adjust the FOV
        mainCamera.transform.DOMove(targetPosition, duration).SetEase(Ease.OutQuad);
        mainCamera.DOFieldOfView(targetFOV, duration).SetEase(Ease.OutQuad);

        // Wait for the zoom-in animation to complete before returning to the original position
        DOVirtual.DelayedCall(duration, () =>
        {
            // Return to the original position after a delay
            ReturnToOriginalPosition(duration);
        });
    }

    // Return the camera to the original position
    private void ReturnToOriginalPosition(float duration = 1f)
    {
        // Zoom the camera back to the original position and FOV
        mainCamera.transform.DOMove(originalCameraPosition, duration).SetEase(Ease.InQuad);
        mainCamera.transform.DORotateQuaternion(originalCameraRotation, duration).SetEase(Ease.InQuad);
        mainCamera.DOFieldOfView(originalCameraFOV, duration).SetEase(Ease.InQuad);
    }

    public void SelectFace(bool isForward) {
        if (isForward) {
            if (faceID == models.Count - 1) {
                faceID = 0;
            } else {
                faceID++;
            }
        } else {
            if (faceID == 0) {
                faceID = models.Count - 1;
            } else {
                faceID--;
            }
        }
        copyMaterials.sourceModel = models[faceID];
        copyMaterials.CopyFaceMaterial();

        ZoomIn(facePosition, targetFOV: 40f); 


        UpdateText();
    }

    public void SelectEyes(bool isForward) {
        if (isForward) {
            if (eyesID == models.Count - 1) {
                eyesID = 0;
            } else {
                eyesID++;
            }
        } else {
            if (eyesID == 0) {
                eyesID = models.Count - 1;
            } else {
                eyesID--;
            }
        }
        copyMaterials.sourceModel = models[eyesID];
        copyMaterials.CopyEyesMaterial();

        ZoomIn(eyesPosition, targetFOV: 40f);  // Adjust camera height based on the model

        UpdateText();
    }

    public void SelectSkin(bool isForward) {
        if (isForward) {
            if (skinID == models.Count - 1) {
                skinID = 0;
            } else {
                skinID++;
            }
        } else {
            if (skinID == 0) {
                skinID = models.Count - 1;
            } else {
                skinID--;
            }
        }
        copyMaterials.sourceModel = models[skinID];
        copyMaterials.CopyBodySkinMaterial();

        ZoomIn(skinPosition, targetFOV: 40f);  // Adjust camera height based on the model

        UpdateText();
    }

    public void SelectHair(bool isForward) {
        if (isForward) {
            if (hairID == models.Count - 1) {
                hairID = 0;
            } else {
                hairID++;
            }
        } else {
            if (hairID == 0) {
                hairID = models.Count - 1;
            } else {
                hairID--;
            }
        }
        chooseHairSets.ShowHair(hairID);

        ZoomIn(hairPosition, targetFOV: 40f);  // Adjust camera height based on the model

        UpdateText();
    }

    public void SelectClothes(bool isForward) {
        if (isForward) {
            if (clothesID == models.Count - 1) {
                clothesID = 0;
            } else {
                clothesID++;
            }
        } else {
            if (clothesID == 0) {
                clothesID = models.Count - 1;
            } else {
                clothesID--;
            }
        }
        copyMaterials.sourceModel = models[clothesID];
        copyMaterials.CopyClothesMaterial();

        ZoomIn(clothesPosition, targetFOV: 40f);  // Adjust camera height based on the model

        UpdateText();
    }

    public void RandomPreset() {
        faceID = Random.Range(0, models.Count);
        eyesID = Random.Range(0, models.Count);
        skinID = Random.Range(0, models.Count);
        hairID = Random.Range(0, models.Count);
        clothesID = Random.Range(0, models.Count);
        UpdateText();

        copyMaterials.sourceModel = models[faceID];
        copyMaterials.CopyFaceMaterial();
        copyMaterials.sourceModel = models[eyesID];
        copyMaterials.CopyEyesMaterial();
        copyMaterials.sourceModel = models[skinID];
        copyMaterials.CopyBodySkinMaterial();
        copyMaterials.sourceModel = models[clothesID];
        copyMaterials.CopyClothesMaterial();
        chooseHairSets.ShowHair(hairID);
    }

    public void SubmitPreset () {

        ReturnToOriginalPosition(duration: 1f);

        PlayerPrefs.SetInt("faceID", faceID);
        PlayerPrefs.SetInt("eyesID", eyesID);
        PlayerPrefs.SetInt("skinID", skinID);
        PlayerPrefs.SetInt("hairID", hairID);
        PlayerPrefs.SetInt("clothesID", clothesID);

        CharacterDataManager.faceID = faceID;
        CharacterDataManager.eyesID = eyesID;
        CharacterDataManager.skinID = skinID;
        CharacterDataManager.hairID = hairID;
        CharacterDataManager.clothesID = clothesID;

        SceneManager.LoadScene("Game");
    }
}