using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button hostSheriffButton;
    [SerializeField] private Button hostOutlawButton;
    [SerializeField] private Button clientSheriffButton;
    [SerializeField] private Button clientOutlawButton;

    private GameManager gameManager;

    private void Awake()
    {
        hostSheriffButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, true);
            hideButtons();
        });
        hostOutlawButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, false);
            hideButtons();
        });
        clientSheriffButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            StartCoroutine(WaitForNetworkConnection(true));
        });
        clientOutlawButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            StartCoroutine(WaitForNetworkConnection(false));
        });
    }

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Make sure connection is valid before checking NetworkManager
    private IEnumerator WaitForNetworkConnection(bool isSheriff)
    {
        // TODO: add timeout so no infinite loop
        while (!NetworkManager.Singleton.IsConnectedClient)
        {
            yield return new WaitForEndOfFrame();
        }

        AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, isSheriff);
        hideButtons();
    }

    // Hides all button UI
    private void hideButtons()
    {
        hostSheriffButton.gameObject.SetActive(false);
        hostOutlawButton.gameObject.SetActive(false);
        clientSheriffButton.gameObject.SetActive(false);
        clientOutlawButton.gameObject.SetActive(false);
    }

    // Adds player gameobject to list in GameManager
    [ServerRpc(RequireOwnership = false)]
    private void AddGameManagerPlayerServerRpc(ulong clientId, bool isSheriff)
    {
        GameObject newPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        if (isSheriff) gameManager.sheriffs.Add(newPlayer.GetComponent<Sheriff>());
        else gameManager.outlaws.Add(newPlayer.GetComponent<Outlaw>());
    }
}
