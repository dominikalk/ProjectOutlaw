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

    private string playerRole;
    private List<string> chatMessages = new List<string>();
    private int maxMessages = 10;

    new public void SendMessage(string role)
    {
        if (string.IsNullOrEmpty(chatInput.text)) return;

        playerRole = role;

        string message = $"{System.DateTime.Now.ToString("hh:mm:ss")} [{role}] {chatInput.text}";

        ChatMessageServerRpc(role, message);

        chatInput.text = string.Empty;
    }

    // Receive chat message from server and update chat window text
    [ClientRpc]
    private void UpdateChatWindowClientRpc(string role, string message)
    {
        chatMessages.Add(message);

        if (chatMessages.Count > maxMessages)
        {
            chatMessages.RemoveAt(0);
        }

        // Filter messages based on player role
        List<string> filteredMessages = chatMessages.Where(m => m.Contains($"[{playerRole}]")).ToList();
        chatText.text = string.Join("\n", filteredMessages);
    }

    // Send chat message to server, which will then broadcast it to all clients
    [ServerRpc(RequireOwnership = false)]
    private void ChatMessageServerRpc(string role, string message)
    {
        UpdateChatWindowClientRpc(role, message);
    }
}
