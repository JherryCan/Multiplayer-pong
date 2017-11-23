using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BallAndScore : MonoBehaviour {

	private GameObject networkManager;
	private int scoreLeft = 0;
	private int scoreRight = 0;
	
	// Use this for initialization
	void Start () 
	{
		if (Network.isClient)
			this.enabled = false;
			
		networkManager = GameObject.Find ("TheNetwork");
	}
	
	void OnTriggerEnter2D(Collider2D other) 
	{
		if (Network.isServer)
		{
			if (other.gameObject.tag == "Ball")
			{
				Network.Destroy(other.gameObject);
				GetComponent<NetworkView>().RPC ("AddScore", RPCMode.All);
				
				if (scoreLeft < 5 && scoreRight < 5)
				{
					networkManager.GetComponent<NetworkManager>().SpawnBallRPC();
				}
			}
		}
	}	
	
	[RPC]
	void AddScore()
	{
		if (this.gameObject.name == "LeftBump")
		{
			scoreLeft++;
			if (scoreLeft != 5)
			{
				GameObject.Find ("Player2Score").GetComponent<Text>().text = scoreLeft.ToString();
			}
			else
			{
				GameObject.Find ("Player2Score").GetComponent<Text>().text = "Win!";
			}
		}
		else
		{
			scoreRight++;
			if (scoreRight != 5)
			{
				GameObject.Find ("Player1Score").GetComponent<Text>().text = scoreRight.ToString();
			}
			else
			{
				GameObject.Find ("Player1Score").GetComponent<Text>().text = "Win!";
			}
		}		
	}
}
