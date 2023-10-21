using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerCard : MonoBehaviour
{
    public Animator anim;
    public Image border;
    public Image background;
    public TextMeshProUGUI answerMesh;
    public RawImage herrings;
    public float manyWiggleDuration = 2f;

    public enum CardColorState
    {
        Default,
        Correct,
        Incorrect
    }

    public Color[] borderCols;
    public Color[] backgroundCols;

    public void LoadAnswerCard(string s)
    {
        answerMesh.text = s;
    }

    public void SetColorState(CardColorState state)
    {
        border.color = borderCols[(int)state];
        background.color = backgroundCols[(int)state];
    }

    [Button]
    public void Reveal()
    {
        if (anim.GetBool("flip"))
            anim.SetBool("flip", false);
        anim.SetTrigger("reveal");
    }

    [Button]
    public void Flip()
    {
        AudioManager.Get.PlayUnique(AudioManager.OneShotClip.Whoosh);
        anim.SetBool("flip", !anim.GetBool("flip"));
    }

    public void ActionOnFlip()
    {
        bool reversed = anim.GetBool("flip");
        //answerMesh.enabled = !reversed;
        herrings.enabled = reversed;
        if (reversed)
        {
            ManyWiggle();
            SetColorState(CardColorState.Incorrect);
        }            
    }

    [Button]
    public void ManyWiggle()
    {
        AudioManager.Get.PlayUniqueAgain(AudioManager.OneShotClip.SqueakMultiple);
        DoManyWiggle();
        Invoke("DoManyWiggle", manyWiggleDuration);
    }

    private void DoManyWiggle()
    {
        anim.SetBool("correct", !anim.GetBool("correct"));
    }

    [Button]
    public void SingleWiggle()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.SqueakOnce);
        anim.SetTrigger("wiggle");
    }

    public void RandomWiggles()
    {
        StartCoroutine(Wiggle());
    }

    private IEnumerator Wiggle()
    {
        while(true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 30f));
            if (UnityEngine.Random.Range(0, 2) == 0)
                SingleWiggle();
        }
    }
}
