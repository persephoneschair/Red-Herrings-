using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TitlesManager : SingletonMonoBehaviour<TitlesManager>
{
    public TextMeshPro titlesMesh;
    public Animator titlesAnim;

    public GameObject[] liveFish;
    public GameObject[] deadFish;

    [TextArea (3,4)] public string[] titleOptions;
    

    [Button]
    public void RunTitleSequence()
    {
        if (Operator.Get.skipOpeningTitles)
        {
            titlesMesh.text = Operator.Get.deadMode ? "Dead\nHerrings!" : "Red\nHerrings!";
            titlesAnim.SetTrigger("end");
            EndOfTitleSequence();
            Invoke("SkipDelay", 3f);
        }
            
        else
        {
            GameplayManager.Get.currentStage = GameplayManager.GameplayStage.DoNothing;
            StartCoroutine(TitleSequence());
        }
    }

    void SkipDelay()
    {
        foreach (GameObject go in liveFish)
            go.SetActive(!Operator.Get.deadMode);

        foreach (GameObject go in deadFish)
            go.SetActive(Operator.Get.deadMode);
    }

    IEnumerator TitleSequence()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.Titles, false);
        yield return new WaitForSeconds(1f);
        for(int i = 0; i < titleOptions.Length - 1; i++)
        {
            titlesMesh.text = titleOptions[i];
            titlesAnim.SetTrigger("toggle");
            yield return new WaitForSeconds(6.5f);
        }
        titlesMesh.text = titleOptions.LastOrDefault();
        titlesAnim.SetTrigger("end");
        LEDManager.Get.LightChase();
        yield return new WaitForSeconds(4.5f);
        if (Operator.Get.deadMode)
        {
            foreach (GameObject go in liveFish)
                go.SetActive(false);

            foreach(GameObject go in deadFish)
                go.SetActive(true);
            
            titlesMesh.text = "Dead\nHerrings!";
            AudioManager.Get.Play(AudioManager.OneShotClip.Splatter);
        }            
        yield return new WaitForSeconds(5f);
        EndOfTitleSequence();
    }

    void EndOfTitleSequence()
    {
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.OpenLobby;
        GameplayManager.Get.ProgressGameplay();
        //this.gameObject.SetActive(false);
    }
}
