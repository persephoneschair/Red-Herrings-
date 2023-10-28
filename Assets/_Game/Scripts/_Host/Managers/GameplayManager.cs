using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.Linq;

public class GameplayManager : SingletonMonoBehaviour<GameplayManager>
{
    [Header("Rounds")]
    public RoundBase[] rounds;

    [Header("Question Data")]
    public static int nextQuestionIndex = 0;

    public enum GameplayStage
    {
        RunTitles,
        OpenLobby,
        LockLobby,
        RevealInstructions,
        HideInstructions,

        LoadRound,
        RunRound,
        RevealFinalWinner,

        RollCredits,
        DoNothing
    };
    public GameplayStage currentStage = GameplayStage.DoNothing;

    public enum Round
    {
        Regular,
        Freezeout,
        Streak,
        Final,
        None
    };
    public Round currentRound = Round.None;
    public int roundsPlayed = 0;

    [Button]
    public void ProgressGameplay()
    {
        switch (currentStage)
        {
            case GameplayStage.RunTitles:
                currentStage = GameplayStage.DoNothing;
                TitlesManager.Get.RunTitleSequence();
                //If in recovery mode, we need to call Restore Players to restore specific player data (client end should be handled by the reload host call)
                //Also need to call Restore gameplay state to bring us back to where we need to be (skipping titles along the way)
                //Reveal instructions would probably be a sensible place to go to, though check that doesn't iterate any game state data itself
                break;

            case GameplayStage.OpenLobby:
                roundsPlayed = Operator.Get.testMode ? 0 + Operator.Get.debugStartRound : 0;
                LobbyManager.Get.OnOpenLobby();
                currentStage++;
                break;

            case GameplayStage.LockLobby:
                currentStage = GameplayStage.DoNothing;
                LobbyManager.Get.OnLockLobby();
                break;

            case GameplayStage.RevealInstructions:
                currentRound = (Round)roundsPlayed;
                InstructionsManager.Get.OnShowInstructions();
                currentStage++;
                break;

            case GameplayStage.HideInstructions:
                InstructionsManager.Get.OnHideInstructions();
                currentStage++;
                break;

            case GameplayStage.LoadRound:
                rounds[(int)currentRound].LoadRound();
                currentStage++;
                break;

            case GameplayStage.RunRound:
                currentStage = GameplayStage.DoNothing;
                rounds[(int)currentRound].RunRound();
                break;

            case GameplayStage.RevealFinalWinner:
                (rounds[(int)currentRound] as FinalRound).RevealFinalWinner();
                currentStage++;
                break;

            case GameplayStage.RollCredits:
                GameplayPennys.Get.UpdatePennysAndMedals();
                CreditsManager.Get.RollCredits();
                currentStage++;
                break;

            case GameplayStage.DoNothing:
                break;
        }
    }

    public RoundBase GetRoundBase()
    {
        return rounds[(int)currentRound];
    }
}
