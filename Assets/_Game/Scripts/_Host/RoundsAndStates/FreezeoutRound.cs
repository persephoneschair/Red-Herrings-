using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeoutRound : RoundBase
{
    public enum AvailablePoints
    {
        Fast,
        Medium,
        Slow
    }

    [Header("Round Specific Settings")]
    public int pointsForFast = 3;
    public int pointsForMedium = 2;
    public int pointsForSlow = 1;

    public int pointsForBonusCorrect = 5;
    public AvailablePoints currentPoints = AvailablePoints.Fast;
}
