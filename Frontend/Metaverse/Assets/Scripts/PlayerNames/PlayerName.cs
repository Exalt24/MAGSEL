using UnityEngine;
using TMPro;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI playerNameText;
    
    private readonly SyncVar<string> playerName = new SyncVar<string>(string.Empty);

    public override void OnStartClient()
    {
        playerNameText.gameObject.SetActive(false);
        playerName.OnChange += SetupNameOnClient;
        
        if (IsOwner)
        {
            string defaultName = GlobalUserData.userName;
            SetupNameOnServerRpc(defaultName);
        } 
        else
        {
            playerNameText.gameObject.SetActive(true);
        }
    }

    public override void OnStopClient()
    {
        playerName.OnChange -= SetupNameOnClient;
    }

    // Called when the SyncVar 'playerName' changes.
    private void SetupNameOnClient(string oldName, string newName, bool asServer)
    {
        playerNameText.text = newName;
    }

    // ServerRpc to update the player name on the server.
    [ServerRpc]
    private void SetupNameOnServerRpc(string name)
    {
        playerName.Value = name;
    }
}
