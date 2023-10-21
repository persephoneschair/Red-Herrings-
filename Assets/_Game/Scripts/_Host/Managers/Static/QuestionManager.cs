using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

public static class QuestionManager
{
    public static Pack currentPack = null;

    public static void DecompilePack(TextAsset tx)
    {
        currentPack = JsonConvert.DeserializeObject<Pack>(tx.text);
    }

    public static Round GetCurrentRound()
    {
        return currentPack.rounds[GameplayManager.Get.roundsPlayed];
    }

    public static string GetRoundTitle()
    {
        return GetCurrentRound().roundTitle;
    }

    public static int GetRedHerringsThisRound()
    {
        return GetCurrentRound().redHerrings.Count;
    }

    public static int GetRoundQCount()
    {
        return GetCurrentRound().questions.Count;
    }

    public static Question GetQuestion(int qNum)
    {
        switch (GameplayManager.Get.currentRound)
        {
            default:
                return null;
        }
    }

    public static bool PlayerIsCorrect(string answer)
    {
        return answer == GameplayManager.Get.GetRoundBase().currentQuestion.correctAnswer;
    }
}
