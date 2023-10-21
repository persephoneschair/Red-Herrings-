using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{

    public AudioSource oneShotSource;
    public AudioSource loopingSource;
    public AudioSource xylSource;

    private bool playedUnique;
    private bool playedUniqueAgain;

    public enum OneShotClip
    {
        Splatter,
        EndOfRoundSting,
        ClockTick,
        CorrectAnswer,
        IncorrectAnswer,
        JoinLobby,
        LeaveLobby,
        SqueakOnce,
        SqueakMultiple,
        PointTick,
        Wheee,
        Whoosh,
        Ding
    };

    public enum LoopClip
    {
        Titles,
        LobbyBed,
        RegularQ,
        ExtendedQ,
        FinalQ,
        Credits
    };

    public AudioClip[] stings;
    public AudioClip[] loops;
    public AudioClip[] xylClips;

    #region Public Methods

    public void Play(OneShotClip oneShot, float delay = 0f)
    {
        if (delay != 0f)
            StartCoroutine(Delay(oneShot, delay));
        else
            oneShotSource.PlayOneShot(stings[(int)oneShot]);
    }

    public void PlayUnique(OneShotClip unique)
    {
        if (playedUnique)
            return;
        playedUnique = true;
        Play(unique);
        Invoke("CancelUnique", 5f);
    }

    public void PlayUniqueAgain(OneShotClip unique)
    {
        if (playedUniqueAgain)
            return;
        playedUniqueAgain = true;
        Play(unique);
        Invoke("CancelUniqueAgain", 5f);
    }

    public void StopLoop()
    {
        loopingSource.Stop();
    }

    public void Play(LoopClip loopClip, bool loop = true, float delay = 0f)
    {
        if(delay != 0f)
            StartCoroutine(Delay(loopClip, loop, delay));
        else
        {
            loopingSource.Stop();
            loopingSource.clip = loops[(int)loopClip];
            loopingSource.loop = loop;
            loopingSource.Play();
        }
    }

    public void PlayXyl(int index)
    {
        xylSource.PlayOneShot(xylClips[index]);
    }

    public void Fade()
    {
        StartCoroutine(FadeOutLoop());
    }

    #endregion

    #region Private Methods

    private void CancelUnique()
    {
        playedUnique = false;
    }
    private void CancelUniqueAgain()
    {
        playedUniqueAgain = false;
    }

    private IEnumerator Delay(OneShotClip oneShot, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(oneShot, 0f);
    }

    private IEnumerator Delay(LoopClip loopClip, bool loop, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(loopClip, loop);
    }

    private IEnumerator FadeOutLoop()
    {
        while (loopingSource.volume > 0)
        {
            yield return new WaitForSeconds(0.05f);
            loopingSource.volume -= 0.02f;
        }
        loopingSource.Stop();
        loopingSource.volume = 1;
    }

    #endregion
}
