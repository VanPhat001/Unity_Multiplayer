using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] Button refreshButton;
    [SerializeField] Button createButton;
    [SerializeField] GameObject contentItem;
    [SerializeField] GameObject content;

    static public LobbyUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {   
        refreshButton.onClick.AddListener(async () => {
            List<Lobby> lobbies = await Multiplayer.Instance.RefreshLobbies();
            RenderLobbyRooms(lobbies);
        });

        createButton.onClick.AddListener(async () => {
            await Multiplayer.Instance.CreateLobby();
        });
    }

    public void RemoveAllContentItems()
    {
        foreach (Transform item in content.transform)
        {
            Destroy(item.gameObject);
        }
    }

    // [ConsoleMethod("addContent")]
    public void AddContentItems(Lobby lobby)
    {
        var newContentItem = Instantiate(contentItem);

        var component = contentItem.GetComponent<ContentItem>();
        component.RoomIdText.text = lobby.Id;
        component.RoomNameText.text = lobby.Name;
        component.RoomStatusText.text = lobby.Players.Count + " / " + lobby.MaxPlayers;
        component.JoinButton.onClick.AddListener(async () => {
            Debug.Log("join clicked, lobby id " + lobby.Id);
            await Multiplayer.Instance.JoinLobby(lobby);
        });

        newContentItem.transform.SetParent(content.transform);
    }

    public void RenderLobbyRooms(List<Lobby> lobbies)
    {
        RemoveAllContentItems();

        foreach (Lobby lobby in lobbies)
        {
            AddContentItems(lobby);
        }
    }
}
