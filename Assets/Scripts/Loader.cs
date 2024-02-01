using IngameDebugConsole;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class Loader
{
    public enum SceneName
    {
        HomeScene,
        LobbyScene,
        MainScene
    }

    [ConsoleMethod("Load", "Load scene with sceneName")]
    public static void LoadScene(SceneName sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }

    [ConsoleMethod("NetworkLoad", "Network load scene with sceneName mode single")]
    public static void NetworkLoadScene(SceneName sceneName)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName.ToString(), LoadSceneMode.Single);
    }
}