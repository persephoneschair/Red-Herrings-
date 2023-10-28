using Control;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerObject
{
    public string playerClientID;
    public Player playerClientRef;
    public Podium podium;
    public string otp;
    public string playerName;

    public GlobalLeaderboardStrap strap;
    public GlobalLeaderboardStrap cloneStrap;

    public string twitchName;
    public Texture profileImage;

    public bool eliminated;

    public int points;
    public bool frozenOut;
    public int streakPointsNextQ = 1;
    public int qsCorrectThisRound = 0;
    public bool perfectRound = false;


    public PlayerObject(Player pl, string name)
    {
        playerClientRef = pl;
        otp = OTPGenerator.GenerateOTP();
        playerName = name;
        points = 0;
        //podium = Podiums.GetPodiums.podia.FirstOrDefault(x => x.containedPlayer == null);
        //podium.containedPlayer = this;
    }

    public void ApplyProfilePicture(string name, Texture tx, bool bypassSwitchAccount = false)
    {
        //Player refreshs and rejoins the same game
        if (PlayerManager.Get.players.Count(x => (!string.IsNullOrEmpty(x.twitchName)) && x.twitchName.ToLowerInvariant() == name.ToLowerInvariant()) > 0 && !bypassSwitchAccount)
        {
            PlayerObject oldPlayer = PlayerManager.Get.players.FirstOrDefault(x => x.twitchName.ToLowerInvariant() == name.ToLowerInvariant());
            if (oldPlayer == null)
                return;

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.SecondInstance, "");

            oldPlayer.playerClientID = playerClientID;
            oldPlayer.playerClientRef = playerClientRef;
            oldPlayer.playerName = playerName;
            oldPlayer.strap.playerNameMesh.text = oldPlayer.playerName;
            oldPlayer.cloneStrap.playerNameMesh.text = oldPlayer.playerName;

            otp = "";
            playerClientRef = null;
            playerName = "";

            if (PlayerManager.Get.pendingPlayers.Contains(this))
                PlayerManager.Get.pendingPlayers.Remove(this);

            HostManager.Get.SendPayloadToClient(oldPlayer, EventLibrary.HostEventType.Validated, $"{oldPlayer.playerName}|{oldPlayer.points.ToString()}|{oldPlayer.twitchName}");
            return;
        }
        otp = "";
        twitchName = name.ToLowerInvariant();
        profileImage = tx;

        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.Validated, $"{playerName}|{points.ToString()}|{twitchName}");
        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
        PlayerManager.Get.players.Add(this);
        PlayerManager.Get.pendingPlayers.Remove(this);
        LeaderboardManager.Get.PlayerHasJoined(this);
        SaveManager.BackUpData();
        //HostManager.GetHost.UpdateLeaderboards();
    }

    public void HandlePlayerScoring(string[] submittedAnswers)
    {
        this.strap.hitAnim.SetTrigger("toggle");
        this.cloneStrap.hitAnim.SetTrigger("toggle");
        switch (GameplayManager.Get.currentRound)
        {
            case GameplayManager.Round.Regular:
                if(QuestionManager.PlayerIsCorrect(submittedAnswers.FirstOrDefault()))
                {
                    qsCorrectThisRound++;
                    AudioManager.Get.Play(AudioManager.OneShotClip.CorrectAnswer);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"CORRECT!|CORRECT");
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    if(GameplayManager.Get.GetRoundBase().bonusQuestion)
                    {
                        points += (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusCorrect;
                        strap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusCorrect, points);
                        cloneStrap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusCorrect, points);
                    }
                    else
                    {
                        points += (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForCorrect;
                        strap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForCorrect, points);
                        cloneStrap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForCorrect, points);
                    }
                    DebugLog.Print($"{playerName} ({qsCorrectThisRound},{points})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
                }
                else
                {
                    AudioManager.Get.Play(AudioManager.OneShotClip.IncorrectAnswer);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"INCORRECT!|INCORRECT");
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    if (GameplayManager.Get.GetRoundBase().bonusQuestion)
                    {
                        points += (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusIncorrect;
                        strap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusIncorrect, points);
                        cloneStrap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForBonusIncorrect, points);
                    }
                    else
                    {
                        points += (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForIncorrect;
                        strap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForIncorrect, points);
                        cloneStrap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as RegularRound).pointsForIncorrect, points);
                    }
                    DebugLog.Print($"{playerName} ({qsCorrectThisRound},{points})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
                }
                break;

            case GameplayManager.Round.Freezeout:
                if (QuestionManager.PlayerIsCorrect(submittedAnswers.FirstOrDefault()))
                {
                    qsCorrectThisRound++;
                    AudioManager.Get.Play(AudioManager.OneShotClip.CorrectAnswer);
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);

                    if (GameplayManager.Get.GetRoundBase().bonusQuestion)
                    {
                        points += (GameplayManager.Get.GetRoundBase() as FreezeoutRound).pointsForBonusCorrect;
                        strap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as FreezeoutRound).pointsForBonusCorrect, points);
                        cloneStrap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as FreezeoutRound).pointsForBonusCorrect, points);
                        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"CORRECT!\n{(GameplayManager.Get.GetRoundBase() as FreezeoutRound).pointsForBonusCorrect} points!|CORRECT");
                    }
                    else
                    {
                        var x = (GameplayManager.Get.GetRoundBase() as FreezeoutRound);
                        int pointsAwarded = x.currentPoints == FreezeoutRound.AvailablePoints.Fast ? x.pointsForFast : x.currentPoints == FreezeoutRound.AvailablePoints.Medium ? x.pointsForMedium : x.pointsForSlow;
                        points += pointsAwarded;
                        strap.PointsTick(points - pointsAwarded, points);
                        cloneStrap.PointsTick(points - pointsAwarded, points);
                        string pointOrPoints = $"{pointsAwarded} {(pointsAwarded == 1 ? "point" : "points")}";
                        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"CORRECT!\n{pointOrPoints} points!|CORRECT");
                    }

                    DebugLog.Print($"{playerName} ({qsCorrectThisRound},{points})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
                }
                else
                {
                    AudioManager.Get.Play(AudioManager.OneShotClip.IncorrectAnswer);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"INCORRECT!\nYou're frozen out of the next question!|INCORRECT");
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    frozenOut = true;

                    DebugLog.Print($"{playerName} ({qsCorrectThisRound},{points})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
                }
                break;

            case GameplayManager.Round.Streak:
                if (QuestionManager.PlayerIsCorrect(submittedAnswers.FirstOrDefault()))
                {
                    qsCorrectThisRound++;
                    string pointOrPoints = $"{streakPointsNextQ} {(streakPointsNextQ == 1 ? "point" : "points")}";
                    AudioManager.Get.Play(AudioManager.OneShotClip.CorrectAnswer);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"CORRECT!\n{pointOrPoints}!|CORRECT");
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);

                    points += streakPointsNextQ;
                    strap.PointsTick(points - streakPointsNextQ, points);
                    cloneStrap.PointsTick(points - streakPointsNextQ, points);
                    streakPointsNextQ += (GameplayManager.Get.GetRoundBase() as StreakRound).pointIncreasePerQ;

                    DebugLog.Print($"{playerName} ({qsCorrectThisRound},{points})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Green);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
                }
                else
                {
                    AudioManager.Get.Play(AudioManager.OneShotClip.IncorrectAnswer);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"INCORRECT!|INCORRECT");
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);

                    streakPointsNextQ = 1;

                    DebugLog.Print($"{playerName} ({qsCorrectThisRound},{points})", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.UpdateScore, $"POINTS: {points}");
                }
                break;

            case GameplayManager.Round.Final:
                //If the round has concluded, no more answers can be processed
                if ((GameplayManager.Get.GetRoundBase() as FinalRound).roundConcluded)
                    return;

                //First, only submissions of exactly the correct answer can be processed
                if (submittedAnswers.Length != GameplayManager.Get.GetRoundBase().currentRound.questions.Count())
                {
                    AudioManager.Get.Play(AudioManager.OneShotClip.IncorrectAnswer);
                    HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"Whoops! You didn't submit exactly {GameplayManager.Get.GetRoundBase().currentRound.questions.Count()} answers!\n Back in {(GameplayManager.Get.GetRoundBase() as FinalRound).defaultFreezeOutTime} seconds...|INCORRECT");
                    strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                    (GameplayManager.Get.GetRoundBase() as FinalRound).UnlockPlayer(this);

                    DebugLog.Print($"{playerName} submission error...", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Orange);
                }
                else
                {
                    List<string> correctAns = GameplayManager.Get.GetRoundBase().currentRound.questions.Select(x => x.correctAnswer).ToList();
                    int matches = submittedAnswers.Count(x => correctAns.Contains(x));
                    if(matches == GameplayManager.Get.GetRoundBase().currentRound.questions.Count())
                    {
                        if ((GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints <= 0)
                            return;

                        DebugLog.Print($"{playerName} has solved the puzzle!", DebugLog.StyleOption.Bold, DebugLog.ColorOption.Green);
                        AudioManager.Get.Play(AudioManager.OneShotClip.CorrectAnswer);
                        strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                        cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Correct);
                        points += (GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints;
                        strap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints, points);
                        cloneStrap.PointsTick(points - (GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints, points);
                        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"CORRECT!\n{(GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints} points!|CORRECT");
                        (GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints -= (GameplayManager.Get.GetRoundBase() as FinalRound).subsequentPointDeduction;

                        if ((GameplayManager.Get.GetRoundBase() as FinalRound).availablePoints <= 0)
                            (GameplayManager.Get.GetRoundBase() as FinalRound).EarlyFinish();
                    }
                    else
                    {
                        DebugLog.Print($"{playerName} got {matches}...", DebugLog.StyleOption.Italic, DebugLog.ColorOption.Red);
                        AudioManager.Get.Play(AudioManager.OneShotClip.IncorrectAnswer);
                        HostManager.Get.SendPayloadToClient(this, EventLibrary.HostEventType.SingleAndMultiResult, $"{matches} CORRECT\n Back in {(GameplayManager.Get.GetRoundBase() as FinalRound).defaultFreezeOutTime} seconds...|INCORRECT");
                        strap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                        cloneStrap.SetStrapColor(GlobalLeaderboardStrap.StrapColor.Incorrect);
                        (GameplayManager.Get.GetRoundBase() as FinalRound).UnlockPlayer(this);
                    }
                }
                break;
        }
    }
}
