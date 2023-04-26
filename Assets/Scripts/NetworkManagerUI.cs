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
    [SerializeField] private Button hostSheriffButton;
    [SerializeField] private Button hostOutlawButton;
    [SerializeField] private Button clientSheriffButton;
    [SerializeField] private Button clientOutlawButton;
    [SerializeField] private TMP_InputField gameCodeInput;
    [SerializeField] private GameObject gameCodeContainer;
    [SerializeField] private TextMeshProUGUI gameCodeText;

    private GameManager gameManager;

    private void Awake()
    {
        hostSheriffButton.onClick.AddListener(() =>
        {
            CreateRelay();
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, true);
                hideButtons();
            };
        });
        hostOutlawButton.onClick.AddListener(() =>
        {
            CreateRelay();
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, false);
                hideButtons();
            };
        });
        clientSheriffButton.onClick.AddListener(() =>
        {
            JoinRelay(gameCodeInput.text);
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, true);
                hideButtons();
            };
        });
        clientOutlawButton.onClick.AddListener(() =>
        {
            JoinRelay(gameCodeInput.text);
            NetworkManager.Singleton.OnClientConnectedCallback += (_) =>
            {
                AddGameManagerPlayerServerRpc(NetworkManager.Singleton.LocalClientId, false);
                hideButtons();
            };
        });
    }

    private async void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }


    // Hides all button UI
    private void hideButtons()
    {
        hostSheriffButton.gameObject.SetActive(false);
        hostOutlawButton.gameObject.SetActive(false);
        clientSheriffButton.gameObject.SetActive(false);
        clientOutlawButton.gameObject.SetActive(false);
        gameCodeInput.gameObject.SetActive(false);
    }

    // Adds player gameobject to list in GameManager
    [ServerRpc(RequireOwnership = false)]
    private void AddGameManagerPlayerServerRpc(ulong clientId, bool isSheriff)
    {
        GameObject newPlayer = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        if (isSheriff) gameManager.sheriffs.Add(newPlayer.GetComponent<Sheriff>());
        else gameManager.outlaws.Add(newPlayer.GetComponent<Outlaw>());
    }

    private async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(9);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log(joinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            gameCodeContainer.SetActive(true);
            gameCodeText.text = $"Game Code: {joinCode}";
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Joining Relay with " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
