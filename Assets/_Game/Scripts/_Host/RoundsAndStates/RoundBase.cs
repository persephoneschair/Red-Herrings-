using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoundBase : MonoBehaviour
{
    public Round currentRound = null;
    public Question currentQuestion = null;
    private int currentQuestionIndex;

    [Header("Base Properties")]
    public Animator questionLozengeAnim;
    public TextMeshProUGUI questionMesh;

    public List<string> allShuffledAnswers = new List<string>();
    public List<AnswerCard> answerCards = new List<AnswerCard>();
    public GameObject answerCardToInstance;
    public Transform answerCardInstanceTarget;

    public Animator bonusAnim;

    [Header("Base Settings")]
    public int defaultQuestionTime = 15;
    public float delayBetweenQuestions = 5f;
    public float endOfRoundDelay = 10f;
    public bool allowBonuses = false;
    [ShowOnly] public bool bonusQuestion = false;
    public int pointsForPerfect = 2;

    public virtual void LoadRound()
    {
        DestroyRoundObjectsAndReset();

        currentRound = QuestionManager.GetCurrentRound();
        currentRound.questions.Shuffle();
        foreach (Question q in currentRound.questions)
            allShuffledAnswers.Add(q.correctAnswer);
        foreach (string s in currentRound.redHerrings)
            allShuffledAnswers.Add(s);
        allShuffledAnswers.Shuffle();

        foreach (string s in allShuffledAnswers)
        {
            answerCards.Add(Instantiate(answerCardToInstance, answerCardInstanceTarget).GetComponent<AnswerCard>());
            answerCards.LastOrDefault().LoadAnswerCard(s);
        }
        StartCoroutine(RevealAnswerCards(false));

        GlobalTimeManager.Get.ToggleClock();
    }

    public IEnumerator RevealAnswerCards(bool endOfRound)
    {
        if (endOfRound)
            yield return new WaitForSeconds(0.5f);

        int xylInd = 0;
        foreach (AnswerCard card in answerCards)
        {
            card.Reveal();
            if(!endOfRound)
            {
                AudioManager.Get.PlayXyl(xylInd);
                xylInd++;
                card.RandomWiggles();
            }                
            yield return new WaitForSeconds(0.15f);
        }

        if (endOfRound)
        {
            yield return new WaitForSeconds(1f);
            DestroyRoundObjectsAndReset();
        }            
    }

    public virtual void RunRound()
    {
        LoadQuestion();
    }

    public virtual void LoadQuestion()
    {
        foreach (AnswerCard a in answerCards)
            a.SetColorState(AnswerCard.CardColorState.Default);

        bonusQuestion = (currentQuestionIndex == 0 || currentQuestionIndex > currentRound.questions.Count - 2 || bonusQuestion || !allowBonuses) ? false : ThisQuestionIsBonus();

        if (bonusQuestion)
            StartCoroutine(AnnounceBonus());
        else
            PreRunQuestion();
    }

    private void PreRunQuestion()
    {
        foreach(PlayerObject po in PlayerManager.Get.players)
        {
            if (po.frozenOut)
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "You are frozen out of this question");
            else
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "Get ready for the question...");
        }
        questionMesh.text = "Get ready...";
        GlobalTimeManager.Get.StartClock(defaultQuestionTime, true);
        currentQuestion = currentRound.questions[currentQuestionIndex];
        Invoke("QuestionRunning", (float)GlobalTimeManager.Get.defaultCountdownTimer - 1);
    }

    private IEnumerator AnnounceBonus()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, "BONUS QUESTION!");

        AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
        bonusAnim.SetTrigger("toggle");
        LEDManager.Get.LightChase();
        yield return new WaitForSeconds(5f);
        PreRunQuestion();
        yield break;
    }

    public virtual void QuestionRunning()
    {
        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => !x.frozenOut))
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.MultipleChoiceQuestion,
                    $"<size=50%>{(currentQuestionIndex + 1).ToString()}/{currentRound.questions.Count}</size>\n{currentQuestion.questionText}|{GameplayManager.Get.GetRoundBase().defaultQuestionTime - 1}|{string.Join("|", allShuffledAnswers)}");

        foreach (PlayerObject po in PlayerManager.Get.players.Where(x => x.frozenOut))
            po.frozenOut = false;

        AudioManager.Get.Play(AudioManager.LoopClip.RegularQ, false, 1f);
        AudioManager.Get.Play(AudioManager.OneShotClip.Ding);
        questionMesh.text = currentQuestion.questionText;
    }

    public virtual void OnQuestionEnded()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"The correct answer was {currentQuestion.correctAnswer}");
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"POINTS: {po.points}");
        }

        LeaderboardManager.Get.ReorderBoard();
        AnswerCard ac = answerCards.FirstOrDefault(x => x.answerMesh.text == currentQuestion.correctAnswer);
        ac.SetColorState(AnswerCard.CardColorState.Correct);
        ac.ManyWiggle();
        Invoke("ResetForNewQuestion", delayBetweenQuestions);
    }

    public virtual void ResetForNewQuestion()
    {
        currentQuestionIndex++;
        ResetPlayerVariables();
        if(currentQuestionIndex == currentRound.questions.Count)
        {
            foreach (PlayerObject po in PlayerManager.Get.players)
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"<size=50%>The Red Herrings were:</size>\n{string.Join("\n", currentRound.redHerrings)}");
            questionMesh.text = "Let's reveal the Red Herrings...";
            foreach (AnswerCard a in answerCards)
                a.SetColorState(AnswerCard.CardColorState.Default);
            foreach (AnswerCard a in answerCards.Where(x => currentRound.redHerrings.Contains(x.answerMesh.text)))
                a.Flip();
            Invoke("EndOfRound", endOfRoundDelay);
        }
        else
            LoadQuestion();
    }

    public virtual void EndOfRound()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
        GlobalTimeManager.Get.ToggleClock();
        LEDManager.Get.LightChase();
        questionMesh.text = $"End of Round {Extensions.ForceFirstCharToUpper(Extensions.NumberToWords(GameplayManager.Get.roundsPlayed + 1))}!";
        foreach (PlayerObject po in PlayerManager.Get.players)
            HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"End of Round {Extensions.ForceFirstCharToUpper(Extensions.NumberToWords(GameplayManager.Get.roundsPlayed + 1))}");
        foreach (AnswerCard a in answerCards.Where(x => currentRound.redHerrings.Contains(x.answerMesh.text)))
        {
            a.Flip();
            a.SetColorState(AnswerCard.CardColorState.Default);
        }
        StartCoroutine(RevealAnswerCards(true));
        ResetEndOfRoundPlayerVariables();
        GameplayManager.Get.currentRound++;
        GameplayManager.Get.roundsPlayed++;
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RevealInstructions;
    }

    public bool ThisQuestionIsBonus()
    {
        return UnityEngine.Random.Range(0, 3) == 0 ? true : false;
    }

    public void DestroyRoundObjectsAndReset()
    {
        foreach (AnswerCard card in answerCards)
            Destroy(card.gameObject);
        answerCards.Clear();
        allShuffledAnswers.Clear();
        currentRound = null;
        currentQuestion = null;
        currentQuestionIndex = 0;
    }

    public virtual void ResetPlayerVariables()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            if (po.frozenOut)
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.FrozenOut);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.FrozenOut);
            }
            else if(po.streakPointsNextQ > 1)
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Streak);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Streak);
            }
            else
            {
                po.strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
                po.cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Default);
            }
        }
    }

    public void ResetEndOfRoundPlayerVariables()
    {
        foreach (PlayerObject po in PlayerManager.Get.players)
        {
            if(po.qsCorrectThisRound == currentRound.questions.Count)
            {
                po.points += GameplayManager.Get.GetRoundBase().pointsForPerfect;
                po.strap.PointsTick(po.points - GameplayManager.Get.GetRoundBase().pointsForPerfect, po.points);
                po.cloneStrap.PointsTick(po.points - GameplayManager.Get.GetRoundBase().pointsForPerfect, po.points);
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.Information, $"End of Round {Extensions.ForceFirstCharToUpper(Extensions.NumberToWords(GameplayManager.Get.roundsPlayed + 1))}\n(You earned a perfect round bonus of {GameplayManager.Get.GetRoundBase().pointsForPerfect} points!");
                HostManager.Get.SendPayloadToClient(po, EventLibrary.HostEventType.UpdateScore, $"POINTS: {po.points}");
            }
            po.frozenOut = false;
            po.streakPointsNextQ = 1;
            po.qsCorrectThisRound = 0;
        }
        LeaderboardManager.Get.ReorderBoard();
        ResetPlayerVariables();
    }
}
