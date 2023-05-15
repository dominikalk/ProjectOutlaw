using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Linq;

public class ChatSystem : NetworkBehaviour
{
    [SerializeField] private TMP_Text chatText;
    [SerializeField] public TMP_InputField chatInput;

    private Dictionary<string, List<string>> chatMessagesByRole = new Dictionary<string, List<string>>();
    private int maxMessagesPerRole = 7;

    new public void SendMessage(string role)
    {
        if (string.IsNullOrEmpty(chatInput.text)) return;

        string message = $"[{System.DateTime.Now.ToString("hh:mm:ss")}] {chatInput.text}";

        ChatMessageServerRpc(role, message);

        chatInput.text = string.Empty;
    }

    // Receive chat message from server and update chat window text
    [ClientRpc]
    private void UpdateChatWindowClientRpc(string role, string message)
    {
        if (!chatMessagesByRole.ContainsKey(role))
        {
            chatMessagesByRole[role] = new List<string>();
        }

        chatMessagesByRole[role].Add(message);

        if (chatMessagesByRole[role].Count > maxMessagesPerRole)
        {
            chatMessagesByRole[role].RemoveAt(0);
        }

        bool isSheriff = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Sheriff>().enabled;
        string playerRole = isSheriff ? "Sheriff" : "Outlaw";

        // Filter messages based on player role
        List<string> filteredMessages = chatMessagesByRole.ContainsKey(playerRole) ? chatMessagesByRole[playerRole] : new List<string>();
        chatText.text = string.Join("\n", filteredMessages);
    }

    // Send chat message to server, which will then broadcast it to all clients
    [ServerRpc(RequireOwnership = false)]
    private void ChatMessageServerRpc(string role, string message)
    {
        UpdateChatWindowClientRpc(role, message);
    }
}
