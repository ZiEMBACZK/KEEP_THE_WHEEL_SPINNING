using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private Transform spawnExitLobbyEffect;
    [SerializeField] private List<GameObject> listOfEffects = new();
    [SerializeField] private SimpleUIManager simpleUIManager;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float apearaAnimationTIme;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.OnPlayerDataNetworkListChanged += OnListChaned;
        GameManager.Instance.OnReadyChanged += OnreadyChanged;
        playerModel.transform.localScale = Vector3.zero;

        transform.DORotate(new Vector3(0, 360, 0), 5f, RotateMode.FastBeyond360)
    .SetEase(Ease.Linear)
    .SetLoops(-1);
    }
    private void OnListChaned(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }
    private void OnreadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }
    public void SpawnDisapearEffectLobby()
    {
        Instantiate(listOfEffects[0], spawnExitLobbyEffect);

    }
    private void UpdatePlayer()
    {
        if (GameManager.Instance.IsPlayerIndexConnected(playerIndex))
        {
            PlayerData playerData = GameManager.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyGameObject.SetActive(GameManager.Instance.IsPlayerReady(playerData.clientId));
            Show();
        }
        else
        {
            readyGameObject.SetActive(false);
            Hide();
        }
    }
    private void Hide()
    {
        playerModel.SetActive(false);
    }
    int effect = 0;
    public void FadeOut(float duration)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = playerModel.GetComponentInChildren<SkinnedMeshRenderer>();
        foreach (Material material in skinnedMeshRenderer.materials)
        {
            if (material.HasProperty("_Color")) // Check if material supports color changes
            {
                material.DOFade(0f, duration).SetEase(Ease.Linear);
            }
        }
    }
    public void FadeIn(float duration)
    {
        SkinnedMeshRenderer skinnedMeshRenderer = playerModel.GetComponentInChildren<SkinnedMeshRenderer>();
        foreach (Material material in skinnedMeshRenderer.materials)
        {
            if (material.HasProperty("_BaseColor")) // Replace "_BaseColor" with your shader's property name for color
            {
                Color color = material.GetColor("_BaseColor");
                color.a = 0f; // Fully transparent
                material.SetColor("_BaseColor", color);

                // Fade to fully visible
                material.DOFade(1f, "_BaseColor", 2f).SetEase(Ease.Linear);
            }
            else
            {
                Debug.LogError("Shader does not support _BaseColor or transparency.");
            }
        }
    }
    private void Show()
    {
        playerModel.SetActive(true);
        playerModel.transform.DOScale(3, apearaAnimationTIme).SetEase(animationCurve);

    }


}
