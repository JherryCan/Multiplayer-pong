using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
	
	float speedX;
	float speedY;
	// Use this for initialization
	void Start () 
	{
		speedX = Random.Range(0, 2) == 0 ? -1 : 1;
		speedY = Random.Range(0, 2) == 0 ? -1 : 1;
		
		GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range (7,9) * speedX, Random.Range (7, 9) * speedY);
	}

	private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = Vector3.zero;
		Vector3 syncVelocity = Vector3.zero;
		
		if (stream.isWriting)
		{
			syncPosition = transform.position;
			stream.Serialize (ref syncPosition);
			
			syncVelocity = GetComponent<Rigidbody2D>().velocity;
			stream.Serialize(ref syncVelocity);
		}
		else
		{			
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			
			transform.position = syncPosition;
			GetComponent<Rigidbody2D>().velocity = syncVelocity;
		}
		
		/*
		stream.Serialize(ref syncPosition);
		stream.Serialize(ref syncVelocity);
		
		syncTime = 0f;
		syncDelay = Time.time - lastSynchronizationTime;
		lastSynchronizationTime = Time.time;
		
		syncEndPosition = syncPosition + syncVelocity * syncDelay;
		syncStartPosition = rigidbody2D.position;
		*/
		
	}
	
	void Update()
	{
	}
}
