using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GlobalTimeManager : SingletonMonoBehaviour<GlobalTimeManager>
{
    public int defaultCountdownTimer = 4;
    private bool isCountdown;
    private int storedStartTime;

    public Animator clockAnim;
    public TextMeshProUGUI clockMesh;

    public int timeRemaining = 0;

    public void StartClock(int startTime, bool countdown = false)
    {
        isCountdown = countdown;
        storedStartTime = startTime;
        if (isCountdown)
            timeRemaining = defaultCountdownTimer;
        else
            timeRemaining = storedStartTime;

        StartCoroutine(RunClock());
    }

    private IEnumerator RunClock()
    {
        while((timeRemaining > 1 && isCountdown) || (timeRemaining > 0 && !isCountdown))
        {
            if(timeRemaining == 1 && !isCountdown)
                AudioManager.Get.Play(AudioManager.OneShotClip.LeaveLobby);
            else
                AudioManager.Get.Play(AudioManager.OneShotClip.ClockTick);

            clockAnim.SetTrigger("tick");
            yield return new WaitForSeconds(1f);

            //Change enum for R2
            if(GameplayManager.Get.currentRound == GameplayManager.Round.Freezeout)
            {
                var rnd = GameplayManager.Get.GetRoundBase() as FreezeoutRound;
                //Reset to fast on countdown
                if (isCountdown)
                    rnd.currentPoints = FreezeoutRound.AvailablePoints.Fast;
                else
                {
                    if (timeRemaining <= rnd.defaultQuestionTime / 3)
                        rnd.currentPoints = FreezeoutRound.AvailablePoints.Slow;
                    else if(timeRemaining <= (rnd.defaultQuestionTime / 3) * 2)
                        rnd.currentPoints = FreezeoutRound.AvailablePoints.Medium;
                    else
                        rnd.currentPoints = FreezeoutRound.AvailablePoints.Fast;
                }
            }
        }

        if(isCountdown)
        {
            isCountdown = false;
            StartClock(storedStartTime + 1);
        }
        else
            OnTimerEnded();
    }

    public void OnTimerEnded()
    {
        GameplayManager.Get.GetRoundBase().OnQuestionEnded();
    }

    public void ToggleClock()
    {
        clockAnim.SetTrigger("toggle");
    }

    public void OnSpinAnimation()
    {
        timeRemaining--;
        clockMesh.text = timeRemaining.ToString();
    }
}
