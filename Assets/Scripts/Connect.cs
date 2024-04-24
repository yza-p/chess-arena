using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Connect : MonoBehaviour
{
    public static Connect Instance { get; set; }

    public Server server;
    public Client client;

    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject loadingOverlay;

    private void Awake()
    {
        Instance = this;
    }

    public void OnButtonPlay(int sceneID)
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        SceneManager.LoadScene(sceneID);
    }

    public void OnCreateRoom()
    {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        loadingOverlay.SetActive(true);
        //Debug.Log("OnCreateRoom");
    }

    public void OnJoinRoom()
    {
        client.Init(addressInput.text, 8007);
    }

    public void OnCancelConnect()
    {
        loadingOverlay.SetActive(false);
    }

    public void OnBackButton()
    {
        server.Shutdown();
        client.Shutdown();
    }
}
