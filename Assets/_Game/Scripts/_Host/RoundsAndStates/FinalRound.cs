 using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalRound : RoundBase
{
    [Header("Round Specific Settings")]
    public int maximumPoints = 50;
    public int subsequentPointDeduction = 5;
    public int defaultFreezeOutTime = 5;
    [ShowOnly] public int availablePoints;

    public void RemoveAnAnswer()
    {
        //Implement this
    }

    public override void QuestionRunning()
    {
        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => !x.frozenOut))
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.MultiSelectQuestion,
                $"{currentQuestion.questionText}|{GameplayManager.Get.GetRoundBase().defaultQuestionTime - 1}|{string.Join("|", allShuffledAnswers)}");

        AudioManager.Get.Play(AudioManager.OneShotClip.Ding);
        AudioManager.Get.Play(AudioManager.LoopClip.FinalQ, false, 1f);
        questionMesh.text = currentQuestion.questionText;
        availablePoints = maximumPoints;
    }

    public void EarlyFinish()
    {
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
        LEDManager.Get.LightChase();
        GlobalTimeManager.Get.timeRemaining = 0;
    }

    public override void OnQuestionEnded()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Time up!\n\nThe correct answers were:\n{string.Join("\n", currentRound.questions.Select(x => x.correctAnswer))}");
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"POINTS: {po.points}");
        }
        StartCoroutine(RevealFinalRedHerrings());
    }

    IEnumerator RevealFinalRedHerrings()
    {
        yield return new WaitForSeconds(5f);
        questionMesh.text = "Let's reveal the Red Herrings...";
        foreach (AnswerCard a in answerCards)
            a.SetColorState(AnswerCard.CardColorState.Default);
        foreach (AnswerCard a in answerCards.Where(x => currentRound.redHerrings.Contains(x.answerMesh.text)))
            a.Flip();

        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RevealFinalWinner;
    }

    public void RevealFinalWinner()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
        GlobalTimeManager.Get.ToggleClock();
        LEDManager.Get.LightChase();
        LobbyManager.Get.TogglePermaCode();
        questionMesh.text = $"GAME OVER!";
        foreach (AnswerCard a in answerCards.Where(x => currentRound.redHerrings.Contains(x.answerMesh.text)))
        {
            a.Flip();
            a.SetColorState(AnswerCard.CardColorState.Default);
        }
        StartCoroutine(RevealAnswerCards(true));
        GameplayManager.Get.roundsPlayed++;
        GameplayManager.Get.currentRound = GameplayManager.Round.None;
        Invoke("KillQStrap", 3.5f);
        foreach (PlayerObject po in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Thanks for playing Red Herrings!\n\nYou earned {po.points * GameplayPennys.Get.multiplyFactor} Pennys this game!");
    }

    private void KillQStrap()
    {
        questionLozengeAnim.SetTrigger("toggle");
        //Maybe display the winner?
    }

    public void UnlockPlayer(PlayerObject po)
    {
        StartCoroutine(UnlockRoutine(po));
    }

    IEnumerator UnlockRoutine(PlayerObject po)
    {
        yield return new WaitForSeconds((GameplayManager.Get.GetRoundBase() as FinalRound).defaultFreezeOutTime);

        HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.MultiSelectQuestion,
                $"{currentQuestion.questionText}|{GlobalTimeManager.Get.timeRemaining - 1}|{string.Join("|", allShuffledAnswers)}");
        po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
        po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
    }
}
