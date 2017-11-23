using UnityEngine;
using System.Collections;
//http://www.paladinstudios.com/2013/07/10/how-to-create-an-online-multiplayer-game-with-unity/

public class PlayerMove : MonoBehaviour {

	private float speed = 15f;
	
	void Start () 
	{
	
	}	
	
	void Update()
	{
		if (GetComponent<NetworkView>().isMine)
		{
			InputMovement();
		}
		else
		{
			SyncedMovement();
		}
	}
	
	void InputMovement () 
	{		
		Vector3 movement = new Vector3(0, Input.GetAxis ("Vertical") * speed * Time.deltaTime, 0);
		transform.Translate(movement);
		
		transform.position = new Vector2(transform.position.x, Mathf.Clamp(transform.position.y, -5.15f, 5.15f));
	}
	
	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		if (transform.position.x > -1 && transform.position.x < -1)
		{
			transform.position = syncEndPosition;		
		}
		else
		{
			transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
		}
	}
	
	private float lastSynchronizationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;
	
	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;

		if (stream.isWriting)
		{
			syncPosition = transform.position;
			stream.Serialize (ref syncPosition);
		}
		else
		{
			stream.Serialize(ref syncPosition);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronizationTime;
			lastSynchronizationTime = Time.time;
			
			syncStartPosition = transform.position;
			syncEndPosition = syncPosition;
		}
	}
}
