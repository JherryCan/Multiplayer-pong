using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour {

	string registeredGameName = "Pong_Server";
	float refreshRequestLength = 3.0f;
	HostData[] hostData;
	
	//private string connectIP = "127.0.0.1";
	//private int connectPort = 25002;
	
	private GameObject startServerButton;
	private GameObject refreshServersButton;
	private GameObject newServerButton;
	
	private GameObject infoPanel;
	private GameObject quitButton;
	private GameObject spawningCounter;
	
	public Transform ball;
	public Transform player1Spawn;
	public Transform player2Spawn;
	
	void Start () 
	{
		startServerButton = GameObject.Find ("StartServerButton");
		refreshServersButton = GameObject.Find ("RefreshServersButton");
		newServerButton = GameObject.Find ("NewServerButton");
		quitButton = GameObject.Find ("QuitButton");
		newServerButton.SetActive(false);
		infoPanel = GameObject.Find ("InfoPanel");
		infoPanel.SetActive(false);
		
		spawningCounter = GameObject.Find ("SpawningCounter");
		spawningCounter.SetActive(false);
	}
	
	public void StartServer_Click()
	{
		//Network.InitializeSecurity();
		Network.InitializeServer(4, 25002, false);
		//Network.InitializeServer(4, connectPort, false);
		MasterServer.RegisterHost (registeredGameName, "Pong Server", "Comment");
	}
	
	private IEnumerator RefreshHostListCoroutine()
	{	
		Debug.Log ("Refreshing...");
		refreshServersButton.transform.FindChild("Text").GetComponent<Text>().text = "Refreshing...";
		MasterServer.RequestHostList(registeredGameName);
		
		float timeEnd = Time.time + refreshRequestLength;
		
		while (Time.time < timeEnd)
		{
			hostData = MasterServer.PollHostList();
			yield return new WaitForEndOfFrame();
		}
		
		if(hostData == null || hostData.Length == 0)
		{
			Debug.Log ("No active servers.");
			refreshServersButton.transform.FindChild("Text").GetComponent<Text>().text = "Refresh Server List";
		}
		else 
		{
			Debug.Log (hostData.Length + " servers found");
			newServerButton.transform.FindChild("Text").GetComponent<Text>().text = "Connect to " + hostData[0].gameName;
			newServerButton.SetActive(true);
			refreshServersButton.transform.FindChild("Text").GetComponent<Text>().text = "Refresh Server List";
		}
		
		//newServerButton.transform.FindChild("Text").GetComponent<Text>().text = "Connect to " + connectIP;
		//newServerButton.SetActive(true);
	}
	
	public void RefreshHostList_Click()
	{
		StartCoroutine(RefreshHostListCoroutine());
	}
	
	public void NewServerButton_Click()
	{
		Network.Connect(hostData[0]);
		//Network.Connect(connectIP, connectPort);
		
		Destroy (startServerButton);
		Destroy (newServerButton);
		Destroy (refreshServersButton);
	}
	
	//server player
	void OnServerInitialized()
	{
		Debug.Log ("Server started.");
				
		Destroy (startServerButton);
		Destroy (newServerButton);
		Destroy (refreshServersButton);
		//Destroy (spawnButton);
		
		//SpawnObjects();	
		Network.Instantiate(Resources.Load("Prefabs/Player"), player1Spawn.position, Quaternion.identity, 0);
		
		GetComponent<NetworkView>().RPC ("ChangeName", RPCMode.AllBuffered, Network.player.ToString());
		
		quitButton.transform.SetParent(infoPanel.transform);
		infoPanel.SetActive(true);		
		infoPanel.transform.FindChild("Player2ReadyButton").gameObject.SetActive(false);
		//MyPlayer = Network.player.ToString();
	}
	
	//client players
	void OnConnectedToServer()
	{
		Network.Instantiate(Resources.Load("Prefabs/Player"), player2Spawn.position, Quaternion.identity, 0);
		
		GetComponent<NetworkView>().RPC ("ChangeName", RPCMode.AllBuffered, Network.player.ToString());
				
		quitButton.transform.SetParent(infoPanel.transform);	
		infoPanel.SetActive(true);
		infoPanel.transform.FindChild("Player1ReadyButton").gameObject.SetActive(false);
	}
	
	[RPC]
	void ChangeName(string player)
	{	
		switch (player)
		{
		case "0":
			GameObject.Find ("Player(Clone)").gameObject.name = "Player1";
			break;
		case "1":
			GameObject.Find ("Player(Clone)").gameObject.name = "Player2";
			GameObject.Find ("Player2").transform.FindChild("PlayerText").GetComponent<TextMesh>().text = "Player 2";
			break;
		case "2":
			GameObject.Find ("Player(Clone)").gameObject.name = "Player3";
			GameObject.Find ("Player3").transform.FindChild("PlayerText").GetComponent<TextMesh>().text = "Player 3";
			break;			
		}
	}
	
	void OnPlayerConnected(NetworkPlayer player)
	{
		Debug.Log ("Player " + player.ToString() + " connected. From: " + player.ipAddress + ":" + player.port );	
		GetComponent<NetworkView>().RPC("ChangeStatus", RPCMode.AllBuffered, player.ToString(), "connected");
	}
	
	[RPC]
	void ChangeStatus(string player, string status)
	{	
		switch (player)
		{
			case "0":
				GameObject.Find ("Player1Status").transform.GetComponent<Text>().text = status;
				break;
			case "1":
				//Debug.Log ("change status for: " + Network.player.ToString());
				GameObject.Find ("Player2Status").transform.GetComponent<Text>().text = status;
				break;			
		}
	}
	
	public void Ready_Click()
	{
		Debug.Log (Network.player.ToString());
		switch (Network.player.ToString())
		{
			case "0":
				GetComponent<NetworkView>().RPC("ChangeStatus", RPCMode.AllBuffered, "0", "ready");
				Destroy (GameObject.Find("Player1ReadyButton"));
				break;
			case "1":
				GetComponent<NetworkView>().RPC("ChangeStatus", RPCMode.All, "1", "ready");
				Destroy (GameObject.Find("Player2ReadyButton"));				
				break;
		}
	}
	
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		Debug.Log ("Player disconnected from: " + player.ipAddress + ":" + player.port);
		GetComponent<NetworkView>().RPC("ChangeStatus", RPCMode.All, player.ToString(), "disconnected");
		//Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	
	void OnApplicationQuit()
	{
		if(Network.isServer)
		{
			GetComponent<NetworkView>().RPC("ChangeStatus", RPCMode.All, "0", "disconnected");
			Network.Disconnect(200);
			MasterServer.UnregisterHost();
		}
		
		if (Network.isClient)
		{
			Network.Disconnect(200);
		}
	}
	
	void OnMasterServerEvent(MasterServerEvent masterServerEvent)
	{
		if(masterServerEvent == MasterServerEvent.RegistrationSucceeded)
		{
			//Debug.Log ("Registration successful.");
		}
	}
	
	private bool ballSpawned = false;
	
	// Update is called once per frame
	void Update () 
	{
		if (Network.isClient)
		{
			GetComponent<NetworkView>().RPC("SendPing", RPCMode.All, Network.player.ToString(), Network.GetAveragePing(Network.connections[int.Parse(Network.player.ToString())-1]));
		}
		
		if (!ballSpawned)
		{
			if (Network.isServer)
			{
				int playersReady = 0;
				
				if (GameObject.Find ("Player1Status").transform.GetComponent<Text>().text == "ready")
				{	
					playersReady++;
				}
				
				if (GameObject.Find ("Player2Status").transform.GetComponent<Text>().text == "ready")
				{	
					playersReady++;
				}
				
				if (playersReady == 2)
				{
					SpawnBallRPC();
					
					ballSpawned = true;
				}
			}
		}
	}

	[RPC]
	void SendPing(string player, int ping)
	{
		switch (player)
		{
			case "1":
				//Debug.Log ("change status for: " + Network.player.ToString());
				GameObject.Find ("Player2Ping").transform.GetComponent<Text>().text = ping.ToString();
				break;
		}
	}
	
	public void SpawnBallRPC()
	{
		GetComponent<NetworkView>().RPC ("SpawnBall", RPCMode.All);
	}
	
	[RPC]
	void SpawnBall() 
	{
		spawningCounter.GetComponent<Text>().text = "3";
		spawningCounter.SetActive(true);
		StartCoroutine("SpawningBall");	
	}
	
	IEnumerator SpawningBall()
	{
		yield return new WaitForSeconds(1);
		spawningCounter.GetComponent<Text>().text = "2";
		yield return new WaitForSeconds(1);
		spawningCounter.GetComponent<Text>().text = "1";
		yield return new WaitForSeconds(1);
		if (Network.isServer)
		{
			Network.Instantiate (ball, new Vector3(0,0,0), Quaternion.identity, 0);
		}
		spawningCounter.SetActive(false);
	}
	
	void SpawnObjects()
	{
		Network.Instantiate(Resources.Load ("Prefabs/BottomBump"), new Vector3(0f, -5f, -8f), Quaternion.identity, 0);
		Network.Instantiate(Resources.Load ("Prefabs/TopBump"), new Vector3(0f, 5f, -8f), Quaternion.identity, 0);
		Network.Instantiate(Resources.Load ("Prefabs/LeftBump"), new Vector3(-9.7f, 0f, 0f), Quaternion.identity, 0);
		Network.Instantiate(Resources.Load ("Prefabs/RightBump"), new Vector3(9.7f, 0f, 0f), Quaternion.identity, 0);
		//Network.Instantiate(Resources.Load ("Prefabs/Ball"), new Vector3(0f, 0f, 0f), Quaternion.identity, 0);
	}
}
