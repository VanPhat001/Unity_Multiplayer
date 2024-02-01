using System.Collections;
using System.Collections.Generic;
using IngameDebugConsole;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
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
        refreshButton.onClick.AddListener(async () =>
        {
            List<Lobby> lobbies = await Multiplayer.Instance.RefreshLobbies();
            RenderLobbyRooms(lobbies);
        });

        createButton.onClick.AddListener(async () =>
        {
            await Multiplayer.Instance.CreateLobby();
        });
    }

    public void RemoveAllContentItems()
    {
        try
        {
            for (int i = content.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(content.transform.GetChild(i).gameObject);
            }
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    public void AddContentItems(Lobby lobby)
    {
        var newContentItem = Instantiate(contentItem);

        var component = newContentItem.GetComponent<ContentItem>();
        component.RoomIdText.text = lobby.Id;
        component.RoomNameText.text = lobby.Name;
        component.RoomStatusText.text = lobby.Players.Count + " / " + lobby.MaxPlayers;

        component.JoinButton.onClick.AddListener(async () =>
        {
            Debug.Log("join clicked, lobby id " + lobby.Id);
            // await Multiplayer.Instance.JoinLobby(lobby);
            await Multiplayer.Instance.QuickJoinLobby();
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
