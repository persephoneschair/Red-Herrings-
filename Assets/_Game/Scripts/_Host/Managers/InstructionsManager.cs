using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InstructionsManager : SingletonMonoBehaviour<InstructionsManager>
{
    public TextMeshProUGUI instructionsMesh;
    public Animator instructionsAnim;
    private readonly string[] instructions = new string[4]
    {
        "Answer the {0} questions using the options available.\n\n" +
        "" +
        "You win <color=green>{1} point</color> for each correct answer.\n" +
        "You lose <color=#FF8484>{2} point</color> for each incorrect answer.\n\n" +
        "" +
        "Additionally, <color=yellow>BONUS QUESTIONS</color> may appear which are worth <color=green>{3} points</color> if correct but cost <color=#FF8484>{4} points</color> if incorrect.\n" +
        "An extra bonus of <color=green>{5} points</color> is awarded for a perfect round.\n\n" +
        "" +
        "Answers will not be used more than once.\n" +
        "There are <color=yellow>{6} RED HERRINGS</color> hidden on the grid which will NEVER be correct.",


        "Answer the {0} questions using the options available.\n\n" +
        "" +
        "Answer faster to earn more points.\n" +
        "<color=#FF8484>Answering incorrectly</color> freezes you out of the following question.\n\n" +
        "" +
        "Additionally, <color=yellow>BONUS QUESTIONS</color> may appear which are worth a flat rate of <color=green>{1} points</color> if correct.\n" +
        "An extra bonus of <color=green>{2} points</color> is awarded for a perfect round.\n\n" +
        "" +
        "Answers will not be used more than once.\n" +
        "There are <color=yellow>{3} RED HERRINGS</color> hidden on the grid which will NEVER be correct.",


        "Answer the {0} questions using the options available.\n\n" +
        "" +
        "Earn more points for each consecutive correct answer.\n" +
        "<color=#FF8484>Answering incorrectly</color> breaks your streak.\n" +
        "<color=orange>Abstain from answering</color> to preserve your streak.\n\n" +
        "" +
        "No bonus questions this round BUT, a bonus of <color=green>{1} points</color> is awarded for a perfect round.\n\n" +
        "" +
        "Answers will not be used more than once.\n" +
        "There are <color=yellow>{2} RED HERRINGS</color> hidden on the grid which will NEVER be correct.",


        "Find the {0} correct answers that fit the criteria.\n\n" +
        "" +
        "Submit a wrong answer and you'll be told how many you got right and be frozen out for {1} seconds.\n\n" +
        "" +
        "The first to find all {0} wins <color=green>{2} points</color>.\n" +
        "Subsequent correct submissions win <color=orange>{3} points fewer</color> than the previous player.\n\n" +
        "" +
        "Answers will gradually be removed as gameplay progresses.\n\n" +
        "" +
        "Gameplay continues until {5} players have found all {0} answers or <color=yellow>{4} seconds</color> have expired."
    };

    [Button]
    public void OnShowInstructions()
    {
        if (GameplayManager.Get.roundsPlayed == 0)
            GameplayManager.Get.GetRoundBase().questionLozengeAnim.SetTrigger("toggle");

        if(GameplayManager.Get.roundsPlayed != 0)
        {
            AudioManager.Get.StopLoop();
            AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
            LEDManager.Get.LightChase();
        }

        DebugLog.Print($"ROUND {Extensions.ForceFirstCharToUpper(Extensions.NumberToWords(GameplayManager.Get.roundsPlayed + 1))}: {QuestionManager.GetRoundTitle()}", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Default);
        instructionsAnim.SetTrigger("toggle");
        GameplayManager.Get.GetRoundBase().questionMesh.text = QuestionManager.GetRoundTitle();
        switch (GameplayManager.Get.currentRound)
        {
            case GameplayManager.Round.Regular:
                instructionsMesh.text = String.Format(instructions[(int)GameplayManager.Get.currentRound],
                    Extensions.NumberToWords(QuestionManager.GetRoundQCount()),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as RegularRound).pointsForCorrect),
                    Extensions.NumberToWords(Math.Abs((GameplayManager.Get.GetRoundBase() as RegularRound).pointsForIncorrect)),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusCorrect),
                    Extensions.NumberToWords(Mathf.Abs((GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusIncorrect)),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as RegularRound).pointsForPerfect),
                    Extensions.NumberToWords(QuestionManager.GetRedHerringsThisRound()).ToUpperInvariant());
                break;

            case GameplayManager.Round.Freezeout:
                instructionsMesh.text = String.Format(instructions[(int)GameplayManager.Get.currentRound],
                    Extensions.NumberToWords(QuestionManager.GetRoundQCount()),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as FreezeoutRound).pointsForBonusCorrect),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as FreezeoutRound).pointsForPerfect),
                    Extensions.NumberToWords(QuestionManager.GetRedHerringsThisRound()).ToUpperInvariant());
                break;

            case GameplayManager.Round.Streak:
                instructionsMesh.text = String.Format(instructions[(int)GameplayManager.Get.currentRound],
                    Extensions.NumberToWords(QuestionManager.GetRoundQCount()),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as StreakRound).pointsForPerfect),
                    Extensions.NumberToWords(QuestionManager.GetRedHerringsThisRound()).ToUpperInvariant());
                break;

            case GameplayManager.Round.Final:
                instructionsMesh.text = String.Format(instructions[(int)GameplayManager.Get.currentRound],
                    Extensions.NumberToWords(QuestionManager.GetRoundQCount()),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as FinalRound).defaultFreezeOutTime),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as FinalRound).maximumPoints),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as FinalRound).subsequentPointDeduction),
                    Extensions.NumberToWords(GameplayManager.Get.GetRoundBase().defaultQuestionTime),
                    Extensions.NumberToWords((GameplayManager.Get.GetRoundBase() as FinalRound).maximumPoints / (GameplayManager.Get.GetRoundBase() as FinalRound).subsequentPointDeduction));
                break;
        }
    }

    [Button]
    public void OnHideInstructions()
    {
        AudioManager.Get.Play(AudioManager.OneShotClip.Wheee);
        instructionsAnim.SetTrigger("toggle");
    }
}
