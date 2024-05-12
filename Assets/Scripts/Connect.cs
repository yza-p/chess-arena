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

    public void OnCreateRoom()
    {
        server.Init(8230);
        client.Init("127.0.0.1", 8230);
        loadingOverlay.SetActive(true);
        //Debug.Log("OnCreateRoom");
    }

    public void OnJoinRoom()
    {
        client.Init(addressInput.text, 8230);
        loadingOverlay.SetActive(true);
    }

    public void OnCancelConnect()
    {
        server.Shutdown();
        client.Shutdown();
        loadingOverlay.SetActive(false);
    }

    public void OnBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        SceneManager.LoadScene(0);
    }
}
