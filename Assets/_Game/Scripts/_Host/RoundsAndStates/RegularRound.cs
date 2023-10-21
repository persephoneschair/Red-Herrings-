using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularRound : RoundBase
{
    [Header("Round Specific Settings")]
    public int pointsForCorrect = 1;
    public int pointsForIncorrect = -1;
    public int pointsForBonusCorrect = 3;
    public int pointsForBonusIncorrect = -3;
}
