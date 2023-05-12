using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

public class NetworkManagerUI : NetworkBehaviour
{
    //[SerializeField] private Button hostSheriffButton;
    //[SerializeField] private Button hostOutlawButton;
    //[SerializeField] private Button clientSheriffButton;
    //[SerializeField] private Button clientOutlawButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    [SerializeField] private TMP_InputField gameCodeInput;

    [SerializeField] private GameObject inputsContainer;
    [SerializeField] private GameObject lobbyContainer;

    [SerializeField] private TextMeshProUGUI gameCodeText;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject startGameDescText;
    [SerializeField] private TextMeshProUGUI playerNoText;
    [SerializeField] private GameObject errorText;

    [SerializeField] private GameObject loading;

    private GameManager gameManager;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            CreateRelay();
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
                lobbyContainer.SetActive(true);
                loading.SetActive(false);
            };
        });
        clientButton.onClick.AddListener(() =>
        {
            JoinRelay(gameCodeInput.text);
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
                lobbyContainer.SetActive(true);
                loading.SetActive(false);
            };
        });
    }

    private async void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            loading.SetActive(false);
            inputsContainer.SetActive(true);
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    // Adds player gameobject to list in GameManager Dev Mode
    //[ServerRpc(RequireOwnership = false)]
    //private void AddGameManagerPlayerDevServerRpc(ulong clientId, bool isSheriff)
    //{
    //    GameObject newPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
    //    if (isSheriff) gameManager.sheriffs.Add(newPlayer.GetComponent<Sheriff>());
    //    else gameManager.outlaws.Add(newPlayer.GetComponent<Outlaw>());
    //}

    // Adds player gameobject to list in GameManager
    [ServerRpc(RequireOwnership = false)]
    private void AddGameManagerPlayerServerRpc(ulong clientId)
    {
        GameObject newPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        gameManager.playerObjects.Add(newPlayer);
        playerNoText.text = $"Players In Lobby: {gameManager.playerObjects.Count}/6";
    }

    private async void CreateRelay()
    {
        try
        {
            inputsContainer.SetActive(false);
            loading.SetActive(true);

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(9);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            gameCodeText.text = $"Game Code: {joinCode}";
            startGameButton.gameObject.SetActive(true);
        }
        catch (RelayServiceException e)
        {
            inputsContainer.SetActive(true);
            loading.SetActive(false);
            errorText.SetActive(true);
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            inputsContainer.SetActive(false);
            loading.SetActive(true);

            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            gameCodeText.text = $"Game Code: {joinCode}";
            startGameDescText.SetActive(true);
        }
        catch (RelayServiceException e)
        {
            inputsContainer.SetActive(true);
            loading.SetActive(false);
            errorText.SetActive(true);
            Debug.Log(e);
        }
    }
}
