using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyManager : SingletonMonoBehaviour<LobbyManager>
{
    public bool lateEntry;

    public TextMeshProUGUI welcomeMessageMesh;
    public Animator lobbyCodeAnim;
    private const string welcomeMessage = "Welcome to\n" +
        "<size=300%><color=yellow>[DEAD] Herrings!</color></size>\n\n" +
        "" +
        "Playing on a mobile device? Scan the QR code!\n\n\n\n\n\n\n\n\n" +
        "" +
        "" +
        "" +
        "" +
        "Desktop or laptop? Please visit:\n" +
        "<size=150%><color=yellow>https://persephoneschair.itch.io/gamenight</color></size>\n" +
        "<color=green><size=300%>[ABCD]</size></color>";

    private const string permaMessage = "<color=yellow>!App\n<size=150%><color=green>[ABCD]";

    public Animator permaCodeAnim;
    public TextMeshProUGUI permaCodeMesh;

    [Button]
    public void OnOpenLobby()
    {
        AudioManager.Get.Play(AudioManager.LoopClip.LobbyBed, true);
        lobbyCodeAnim.SetTrigger("toggle");
        welcomeMessageMesh.text = welcomeMessage.Replace("[ABCD]", HostManager.Get.host.RoomCode.ToUpperInvariant()).Replace("[DEAD]", Operator.Get.deadMode ? "Dead" : "Red");
    }

    [Button]
    public void OnLockLobby()
    {
        lateEntry = true;
        lobbyCodeAnim.SetTrigger("toggle");
        permaCodeMesh.text = permaMessage.Replace("[ABCD]", HostManager.Get.host.RoomCode.ToUpperInvariant());
        AudioManager.Get.StopLoop();
        AudioManager.Get.Play(AudioManager.OneShotClip.EndOfRoundSting);
        LEDManager.Get.LightChase();
        Invoke("TogglePermaCode", 1f);
        Invoke("DelayOpeningRound", 1f);
        
    }

    void DelayOpeningRound()
    {
        GameplayManager.Get.currentStage = GameplayManager.GameplayStage.RevealInstructions;
        GameplayManager.Get.ProgressGameplay();
    }

    public void TogglePermaCode()
    {
        permaCodeAnim.SetTrigger("toggle");
    }
}
