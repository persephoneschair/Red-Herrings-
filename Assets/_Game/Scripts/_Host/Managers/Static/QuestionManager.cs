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

    public static void ConvertFromLegacyTemplate(TextAsset tx)
    {
        LegacyPack lp = JsonConvert.DeserializeObject<LegacyPack>(tx.text);

        Pack p = new Pack()
        {
            author = Operator.Get.legacyPackAuthor
        };

        LegacyStandardRound[] lRs = new LegacyStandardRound[3] { lp.round1, lp.round2, lp.round3 };

        for(int i = 0; i < 3; i++)
        {
            Round r = new Round()
            {
                roundTitle = lRs[i].title,
                redHerrings = lRs[i].rogueAnswers,
                questions = new List<Question>()
            };
            for(int j = 0; j < lRs[i].questions.Count; j++)
            {
                Question q = new Question()
                {
                    questionText = lRs[i].questions[j],
                    correctAnswer = lRs[i].answers[j]
                };
                r.questions.Add(q);
            }
            p.rounds.Add(r);
        }

        Round fR = new Round()
        {
            roundTitle = lp.finalRound.roundTitle,
            redHerrings = lp.finalRound.rogueAnswers,
            questions = new List<Question>()
        };
        for(int i = 0; i < lp.finalRound.correctAnswers.Count; i++)
        {
            Question q = new Question()
            {
                questionText = lp.finalRound.roundDescription,
                correctAnswer = lp.finalRound.correctAnswers[i]
            };
            fR.questions.Add(q);
        }
        p.rounds.Add(fR);

        Operator.Get.legacyPackOutput = JsonConvert.SerializeObject(p, Formatting.Indented);
        currentPack = p;
    }
}
