using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContentItem : MonoBehaviour
{
    [SerializeField] private TMP_Text roomIdText;
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text roomStatusText;
    [SerializeField] private Button joinButton;

    public TMP_Text RoomIdText => roomIdText;
    public TMP_Text RoomNameText => roomNameText;
    public TMP_Text RoomStatusText => roomStatusText;
    public Button JoinButton => joinButton;

    // private void Awake() {
    //     joinButton.onClick.AddListener(() => {
    //         Debug.Log("join button click................................");
    //     });
    // }
}
