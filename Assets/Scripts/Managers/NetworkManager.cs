using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager instance;
	public Canvas canvas;
	//public SocketIOComponent socket;
	public InputField playerNameInput;
	public GameObject player;

	public void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
		DontDestroyOnLoad (gameObject);
	}

	// Use this for initialization
	void Start () {
		//TODO: Subscription
	}

	public void JoinGame() {
		StartCoroutine (ConnectToServer ());
	}

	#region Commands

	IEnumerator ConnectToServer() {
		yield return new WaitForSeconds (0.5f);
	}

	#endregion



	#region Listening

	#endregion



	#region JSONMessageClasses

	[Serializable]
	public class PlayerJSON
	{
		public string name;
		public List<PointJSON> playerSpawnPoints;

		public PlayerJSON(string _name, List<SpawnPoint> _playerSpawnPoints) {
			playerSpawnPoints = new List<PointJSON>();
			name = _name;

			foreach(SpawnPoint playerSpawnPoint in _playerSpawnPoints) {
				playerSpawnPoint.Add(new PointJSON(playerSpawnPoint));
			}
		}
	}

	[Serializable]
	public class PointJSON {
		
		public PointJSON(SpawnPoint spawnPoint) {
			
		}
	}




	#endregion

}
