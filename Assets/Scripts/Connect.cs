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
        Debug.Log("Connect screen script is ok!");
    }

    public void OnCreateRoom()
    {
        server.Init(6039);
        client.Init("127.0.0.1", 6039);
        loadingOverlay.SetActive(true);
        //Debug.Log("OnCreateRoom");
    }

    public void OnJoinRoom()
    {
        client.Init(addressInput.text, 6039);
        loadingOverlay.SetActive(true);
    }

    public void OnCancelConnect()
    {
        if (server.isActiveAndEnabled)
            server.Shutdown();
        if (client.isActiveAndEnabled)
            client.Shutdown();
        loadingOverlay.SetActive(false);
    }

    public void OnBackButton()
    {
        if (server.isActiveAndEnabled)
            server.Shutdown();
        if (client.isActiveAndEnabled)
            client.Shutdown();
        SceneManager.LoadScene(0);
    }
}
