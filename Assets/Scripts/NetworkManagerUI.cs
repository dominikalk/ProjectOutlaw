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

    [SerializeField] private GameObject instructionsUI;

    [SerializeField] private GameObject loading;

    private GameManager gameManager;

    private bool connected = false;

    private void Awake()
    {
        hostButton.onClick.AddListener(() =>
        {
            CreateRelay();
            NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
            {
                if (id != 0) return;
                GameObject newPlayer = NetworkManager.LocalClient.PlayerObject.gameObject;
                gameManager.playerObjects.Add(newPlayer);
                playerNoText.text = $"Players In Lobby: {gameManager.playerObjects.Count}/6";
                lobbyContainer.SetActive(true);
                loading.SetActive(false);
            };
        });
        clientButton.onClick.AddListener(() =>
        {
            JoinRelay(gameCodeInput.text);
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                if (IsServer || connected) return;
                connected = true;
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
                lobbyContainer.SetActive(true);
                loading.SetActive(false);
                RectTransform rect = GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
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

    // Adds player gameobject to list in GameManager
    [ServerRpc(RequireOwnership = false)]
    private void AddGameManagerPlayerServerRpc(ulong clientId)
    {
        GameObject newPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        gameManager.playerObjects.Add(newPlayer);
        playerNoText.text = $"Players In Lobby: {gameManager.playerObjects.Count}/6";

        if (gameManager.playerObjects.Count > 1) startGameButton.interactable = true;

        SetNoPlayersClientRpc(gameManager.playerObjects.Count);
    }

    // Set player lobby number in all clients
    [ClientRpc]
    private void SetNoPlayersClientRpc(int noPlayers)
    {
        playerNoText.text = $"Players In Lobby: {noPlayers}/6";
    }

    private async void CreateRelay()
    {
        try
        {
            inputsContainer.SetActive(false);
            loading.SetActive(true);

            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(5);
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

            gameCodeText.text = $"Game Code: {joinCode.ToUpper()}";
            startGameDescText.SetActive(true);
        }
        catch (RelayServiceException e)
        {
            NetworkManager.Singleton.Shutdown();
            inputsContainer.SetActive(true);
            loading.SetActive(false);
            errorText.SetActive(true);
            Debug.Log(e);
        }
    }

    // Enable and disable join button if input has/ doesn't have correct number of chars
    public void OnJoinInputValueChanged()
    {
        if (gameCodeInput.text.Length == 6)
        {
            clientButton.interactable = true;
        }
        else
        {
            clientButton.interactable = false;
        }
    }

    public void ShowInstructionsUI()
    {
        instructionsUI.SetActive(true);
    }

    public void HideInstructionsUI()
    {
        instructionsUI.SetActive(false);
    }
}
