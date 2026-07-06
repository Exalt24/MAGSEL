using UnityEngine;
using UnityEngine.UI;
using FishNet.Managing;
using FishNet.Transporting;

public class SimpleNetworkUI : MonoBehaviour
{
    [Header("UI References")]
    public InputField ipInputField;
    public Button serverButton;
    public Button clientButton;
    public Text statusText;
    
    [Header("Network")]
    public NetworkManager networkManager;
    
    void Start()
    {
        // Default to localhost
        ipInputField.text = "127.0.0.1";
        
        serverButton.onClick.AddListener(StartServer);
        clientButton.onClick.AddListener(StartClient);
    }
    
    void StartServer()
    {
        statusText.text = "Starting Server...";
        networkManager.ServerManager.StartConnection();
        
        // Show server IP for others to connect
        string serverIP = GetLocalIPAddress();
        statusText.text = $"Server running on: {serverIP}:7770";
    }
    
    void StartClient()
    {
        string targetIP = ipInputField.text;
        statusText.text = $"Connecting to {targetIP}...";
        
        // Set the client address in the transport
        Transport transport = networkManager.TransportManager.Transport;
        transport.SetClientAddress(targetIP);
        
        networkManager.ClientManager.StartConnection();
    }
    
    string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        return "127.0.0.1";
    }
}