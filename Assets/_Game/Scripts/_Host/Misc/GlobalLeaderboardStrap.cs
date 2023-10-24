using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalLeaderboardStrap : MonoBehaviour
{
    public Vector3 startPos;
    public PlayerObject containedPlayer;

    public TextMeshProUGUI playerNameMesh;
    public TextMeshProUGUI totalCorrectMesh;
    public RawImage avatarRend;

    public Image borderRend;
    public Image backgroundRend;

    public Color[] borderCols;
    public Color[] backgroundCols;

    private Vector3 targetPosition;
    private float elapsedTime = 0;

    public Animator hitAnim;

    public enum StrapColor
    {
        Default,
        LockedIn,
        FrozenOut,
        Correct,
        Incorrect,
        Streak
    }


    public void SetUpStrap()
    {
        startPos = GetComponent<RectTransform>().localPosition;
        targetPosition = startPos;
        playerNameMesh.text = "";
        totalCorrectMesh.text = "";
        gameObject.SetActive(false);
    }

    public void PopulateStrap(PlayerObject pl, bool isClone)
    {
        //Flicker to life?
        gameObject.SetActive(true);
        containedPlayer = pl;
        playerNameMesh.text = pl.playerName;
        avatarRend.texture = pl.profileImage;
        totalCorrectMesh.text = pl.points.ToString();
        if (!isClone)
            pl.strap = this;
        else
            pl.cloneStrap = this;
    }

    public void SetStrapColor(StrapColor col)
    {
        backgroundRend.color = backgroundCols[(int)col];
        borderRend.color = borderCols[(int)col];
    }

    public void PointsTick(int current, int target)
    {
        hitAnim.SetTrigger("toggle");
        //LeaderboardManager.Get.ReorderBoard();
        StartCoroutine(TickRoutine(current, target));
    }

    IEnumerator TickRoutine(int current, int target)
    {
        if(current < target)
        {
            for (int i = current + 1; i <= target; i++)
            {
                totalCorrectMesh.text = i.ToString();
                AudioManager.Get.Play(AudioManager.OneShotClip.PointTick);
                yield return new WaitForSeconds(0.1f);
            }                
        }
        else
        {
            for (int i = current - 1; i >= target; i--)
            {
                totalCorrectMesh.text = i.ToString();
                AudioManager.Get.Play(AudioManager.OneShotClip.PointTick);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void MoveStrap(Vector3 targetPos, int i)
    {
        //playerNameMesh.text = (i + 1).ToString();
        targetPosition = targetPos;
        elapsedTime = 0;
    }

    public void Update()
    {
        LerpStraps();
    }

    private void LerpStraps()
    {
        elapsedTime += Time.deltaTime * 1f;
        float percentageComplete = elapsedTime / LeaderboardManager.Get.reorderDuration;
        this.gameObject.transform.localPosition = Vector3.Lerp(this.gameObject.transform.localPosition, targetPosition, Mathf.SmoothStep(0, 1, percentageComplete));
    }
}
