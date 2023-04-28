using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChatManagerController : NetworkBehaviour
{
    [Header("Connection Variables")]
    public string username;

    [Header("Connection Panel Variables")]
    public GameObject connectionPanel;
    public TMP_InputField usernameInput;
    public GameObject iconConnection;
    public TextMeshProUGUI usernameDisplay;

    [Header("Chat Screen Variables")]
    public TMP_InputField chatInput;
    public GameObject chatPanel;
    public TextMeshProUGUI chatText;
    public Scrollbar scrollbar;

    private void Awake()
    {
        Screen.SetResolution(820, 480, false);
        iconConnection.SetActive(false);
        connectionPanel.SetActive(true);
        chatPanel.SetActive(false);
    }

    //Connections
    public void ServerConnection()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            connectionPanel.SetActive(false);
            iconConnection.SetActive(true);
            usernameDisplay.text = "Server";
            chatText.text += "Server Started\n";
        }
    }

    public void ClientConnection()
    {
        if (usernameInput.text != "")
        {
            chatPanel.SetActive(true);
            username = usernameInput.text;
            
            NetworkManager.Singleton.StartClient();
            connectionPanel.SetActive(false);
            iconConnection.SetActive(true);
            usernameDisplay.text = username;
            
            StartCoroutine(ClientConnected());

        }
    }

    //Chat Functions
    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ulong clientID)
    {
        Debug.Log("ServerRpc");
        ReceiveChatMessageClientRpc(message, clientID);

        chatText.text += "<color=black>" + message + "</color>\n";
        StartCoroutine(ScrollDown());
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong clientID)
    {
        Debug.Log("Message Received: " + clientID);
        // Detect if the user is the one who sent the message. The message has the username before ":". Red for the owner, black for everyone else.
        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            chatText.text += "<color=red>" + message + "</color>\n";
        }
        else if(clientID == 0)
        {
            chatText.text += "<color=blue>" + message + "</color>\n";
        }
        else
        {
            chatText.text += "<color=black>" + message + "</color>\n";
        }

        StartCoroutine(ScrollDown());
    }

    public void SendChatMessage()
    {
        if (chatInput.text != ""){ 
            Debug.Log("SendChatMessage");
            string message = "<b>" + username + ":</b> " + chatInput.text;
            chatInput.text = "";
            SendChatMessageServerRpc(message, NetworkManager.Singleton.LocalClientId);

            chatInput.ActivateInputField();
            chatInput.Select();
        }
    }
    
    IEnumerator ScrollDown()
    {
        yield return new WaitForSeconds(0.1f);
        scrollbar.value = 0;
    }

    IEnumerator ClientConnected()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("ClientConnected");
        chatInput.text = "";
        SendChatMessageServerRpc("Client <b><" + username + "></b> has been connected", 0);
    }
}
