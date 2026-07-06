using DG.Tweening;
using FishNet.Component.Prediction;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile[] upNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;
    public Tile[] rightNeighbours;

    public float scalingFactor = 1f;

    private void Awake()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one * scalingFactor, 5f).SetEase(Ease.OutElastic);
    }
}