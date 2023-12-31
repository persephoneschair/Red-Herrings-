using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
{
    public List<PlayerObject> pendingPlayers = new List<PlayerObject>();
    public List<PlayerObject> players = new List<PlayerObject>();

    [Header("Controls")]
    public bool pullingData = true;
    [Range(0,39)] public int playerIndex;


    private PlayerObject _focusPlayer;
    public PlayerObject FocusPlayer
    {
        get { return _focusPlayer; }
        set
        {
            if(value != null)
            {
                _focusPlayer = value;
                playerName = value.playerName;
                twitchName = value.twitchName;
                profileImage = value.profileImage;

                points = value.points;
            }
            else
            {
                playerName = "OUT OF RANGE";
                twitchName = "OUT OF RANGE";
                profileImage = null;

                points = 0;
            }                
        }
    }

    [Header("Fixed Fields")]
    [ShowOnly] public string playerName;
    [ShowOnly] public string twitchName;
    public Texture profileImage;

    [Header("Variable Fields")]
    public int points;

    void UpdateDetails()
    {
        if (playerIndex >= players.Count)
            FocusPlayer = null;
        else
            FocusPlayer = players.OrderBy(x => x.playerName).ToList()[playerIndex];
    }

    private void Update()
    {
        if (pullingData)
            UpdateDetails();
    }

    [Button]
    public void SetPlayerDetails()
    {
        if (pullingData)
            return;
        SetDataBack();
    }

    [Button]
    public void RestoreOrEliminatePlayer()
    {
        if (pullingData)
            return;
        pullingData = true;

    }

    void SetDataBack()
    {
        FocusPlayer.points = points;
        pullingData = true;
    }
}
