using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreditsManager : SingletonMonoBehaviour<CreditsManager>
{

    public GameObject endCard;
    [TextArea(10, 20)] public string additionalCredits;
    public TextMeshPro creditsMesh;
    public Animator creditsAnim;

    private void Start()
    {
        this.gameObject.SetActive(false);
    }

    [Button]
    public void RollCredits()
    {
        LEDManager.Get.DimLights();
        creditsMesh.alignment = TextAlignmentOptions.Top;
        creditsMesh.enableAutoSizing = false;
        AudioManager.Get.Play(AudioManager.LoopClip.Credits, false);
        creditsMesh.text = additionalCredits.Replace("[RED]", Operator.Get.deadMode ? "Dead" : "Red");
        this.gameObject.SetActive(true);
        creditsAnim.SetTrigger("toggle");
        StartCoroutine(EndCard());
    }

    IEnumerator EndCard()
    {
        yield return new WaitForSeconds(58.5f);
        endCard.SetActive(true);
        LeaderboardManager.Get.HideAllStraps();
    }
}
