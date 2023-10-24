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
    [ShowOnly] public bool roundConcluded;

    public int redHerringRemovalDelay = 10;
    public int redHerringRemovalFrequency = 2;
    private int cardsRemoved = 0;

    public IEnumerator RemoveAnAnswer()
    {
        yield return new WaitForSeconds(redHerringRemovalDelay);
        List<AnswerCard> unrevealedRedHerrings = answerCards.Where(x => currentRound.redHerrings.Contains(x.answerMesh.text)).ToList();

        while (GlobalTimeManager.Get.timeRemaining > 5 && cardsRemoved < currentRound.questions.Count + 2)
        {
            cardsRemoved++;
            AnswerCard a = unrevealedRedHerrings.PickRandom();
            a.Flip();
            foreach (PlayerObject po in PlayerManager.Get.players)
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.KillSingleMultiSelectButton, a.answerMesh.text);
            unrevealedRedHerrings.Remove(a);
            yield return new WaitForSeconds(redHerringRemovalFrequency);
        }
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
        if(redHerringRemovalDelay > 0)
            StartCoroutine(RemoveAnAnswer());
    }

    public void EarlyFinish()
    {
        roundConcluded = true;
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
        LEDManager.Get.LightChase();
        GlobalTimeManager.Get.timeRemaining = 0;
    }

    public override void OnQuestionEnded()
    {
        roundConcluded = true;
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Time up!\n\n<size=50%>The correct answers were:</size>\n{string.Join("\n", currentRound.questions.Select(x => x.correctAnswer))}");
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"POINTS: {po.points}");
        }
        LeaderboardManager.Get.ReorderBoard();
        StartCoroutine(RevealFinalRedHerrings());
    }

    IEnumerator RevealFinalRedHerrings()
    {
        yield return new WaitForSeconds(5f);
        questionMesh.text = "Let's reveal the Red Herrings...";
        foreach (AnswerCard a in answerCards.Where(x => currentRound.redHerrings.Contains(x.answerMesh.text) && !x.anim.GetBool("flip")))
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
        {
            po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
            po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"Thanks for playing Red Herrings!\n\nYou earned {po.points * GameplayPennys.Get.multiplyFactor} Pennys this game!");
        }
            
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

        //Add some catch for if it's timed out
        if (!roundConcluded)
        {
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.MultiSelectQuestion,
                    $"{currentQuestion.questionText}|{GlobalTimeManager.Get.timeRemaining - 1}"); //|{string.Join("|", allShuffledAnswers)} - removed from end for special unlocking mode
            po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
            po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
        }
    }
}
