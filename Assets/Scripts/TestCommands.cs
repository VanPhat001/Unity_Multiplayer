using IngameDebugConsole;
using Unity.Netcode;
using UnityEngine;

static public class TestCommands
{
    [ConsoleMethod("cube", "Creates a cube at specified position")]
    public static void CreateCubeAt(Vector3 position)
    {
        GameObject.CreatePrimitive(PrimitiveType.Cube).transform.position = position;
    }

    [ConsoleMethod("removeAllContent", "")]
    public static void RemoveAllContent()
    {
        LobbyUI.Instance.RemoveAllContentItems();
    }

    [ConsoleMethod("createLobby", "")]
    public static async void CreateLobby()
    {
        await Multiplayer.Instance.CreateLobby();
    }

    [ConsoleMethod("quickJoinLobby", "")]
    public static async void QuickJoinLobby()
    {
        await Multiplayer.Instance.QuickJoinLobby();
    }

    [ConsoleMethod("showLobbies", "")]
    public static void ShowLobbies()
    {
        Multiplayer.Instance.ShowLobbies();
    }

    [ConsoleMethod("showInfo", "")]
    public static void ShowInfo()
    {
        try
        {
            Debug.Log($"Server {NetworkManager.Singleton.IsServer}\nHost {NetworkManager.Singleton.IsHost}\nClient {NetworkManager.Singleton.IsClient}");
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }
}
