using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SocketIO;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager instance;
	public Canvas canvas;
	public SocketIOComponent socket;
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
		socket.On ("playerConnected", onPlayerConnected);
		socket.On ("play", onPlay);
		socket.On ("playerMove", onPlayerMove);
		socket.On ("playerRotate", onPlayerRotate);
		socket.On ("playerShoot", onPlayerShoot);
		socket.On ("health", onHealth);
		socket.On ("playerDisconnect", onPlayerDisconnect);
	}

	public void JoinGame() {
		StartCoroutine (ConnectToServer ());
	}

	#region Commands

	IEnumerator ConnectToServer() {
		yield return new WaitForSeconds (0.5f);
		socket.Emit ("playerConnected");
		yield return new WaitForSeconds (1f);

		string playerName = playerNameInput.text;

		PlayerJSON playerJSON = new PlayerJSON (playerName);
		string data = JsonUtility.ToJson (playerJSON);
		socket.Emit("play", new JSONObject(data));

		canvas.gameObject.SetActive (false);
	}

	public void CommandMove(Vector3 movement) {
		string data = JsonUtility.ToJson (new PositionJSON(movement));
		socket.Emit("playerMove", new JSONObject(data));
	}

	public void CommandRotate(Quaternion rotation) {
		string data = JsonUtility.ToJson (new RotationJSON(rotation));
		socket.Emit("playerRotate", new JSONObject(data));
	}

	public void CommandShoot() {
		socket.Emit ("playerShoot");
	}

	public void CommandHealthChange(string playerFrom, string playerTo, float damage) {
		HealthChangeJSON healthJSON = new HealthChangeJSON (playerTo, damage, playerFrom);
		socket.Emit("playerHealth", new JSONObject(JsonUtility.ToJson(healthJSON)));
	}
	#endregion



	#region Listening

	void onPlayerConnected(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserJSON userData = UserJSON.CreateFromJSON (data);
		Vector3 position = new Vector3 (userData.position [0], userData.position [1], userData.position [2]);
		Quaternion rotation = Quaternion.Euler (0f, userData.rotation, 0f);

		var tank = GameObject.Find (userData.name) as GameObject;
		if (tank != null) {
			return;
		}
		GameObject p = Instantiate(player, position, rotation) as GameObject;
		p.name = userData.name;

		TankMovement movement = p.GetComponent<TankMovement> ();
		movement.isLocalPlayer = false;

		TankShooting shooting = p.GetComponent<TankShooting> ();
		shooting.isLocalPlayer = false;
		shooting.playerName = userData.name;

		TankHealth health = p.GetComponent<TankHealth> ();
		health.m_CurrentHealth = userData.health;
		health.OnHealthChange ();
	}

	void onPlay(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserJSON currentUserData = UserJSON.CreateFromJSON (data);
		Vector3 position = new Vector3 (currentUserData.position [0], currentUserData.position [1], currentUserData.position [2]);
		Quaternion rotation = Quaternion.Euler (0f, currentUserData.rotation, 0f);

		GameObject p = Instantiate(player, position, rotation) as GameObject;
		p.name = currentUserData.name;

		TankShooting shooting = p.GetComponent<TankShooting> ();
		shooting.isLocalPlayer = true;
		shooting.playerName = currentUserData.name;

		TankMovement movement = p.GetComponent<TankMovement> ();
		movement.isLocalPlayer = true;

	}

	void onPlayerMove(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserJSON userData = UserJSON.CreateFromJSON (data);

		if (userData.name == playerNameInput.text) {
			return;
		}

		GameObject p = GameObject.Find (userData.name) as GameObject;
		if (p != null) {
			p.transform.position = new Vector3 (userData.position [0], userData.position [1], userData.position [2]);
		}
	}

	void onPlayerRotate(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserJSON userData = UserJSON.CreateFromJSON (data);

		if (userData.name == playerNameInput.text) {
			return;
		}

		GameObject p = GameObject.Find (userData.name) as GameObject;
		if (p != null) {
			p.transform.rotation = Quaternion.Euler (0f, userData.rotation, 0f);
		}
	}

	void onPlayerShoot(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		ShootJSON shootData = ShootJSON.CreateFromJSON (data);

		GameObject p = GameObject.Find (shootData.name);
		TankShooting s = p.GetComponent<TankShooting> ();
		s.CmdFire ();
	}

	void onHealth(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserHealthJSON healthData = UserHealthJSON.CreateFromJSON (data);

		GameObject p = GameObject.Find (healthData.name);
		TankHealth h = p.GetComponent<TankHealth> ();
		h.m_CurrentHealth = healthData.health;
		h.OnHealthChange ();
	}

	void onPlayerDisconnect(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserJSON userData = UserJSON.CreateFromJSON (data);
		Destroy (GameObject.Find (userData.name));
	}


	#endregion



	#region JSONMessageClasses

	[Serializable]
	public class PlayerJSON
	{
		public string name;

		public PlayerJSON(string _name) {
			name = _name;
		}
	}

	// Notifies when another player joins the game
	[Serializable]
	public class UserJSON
	{
		public string name;
		public float[] position;
		public float rotation;
		public float health;

		public static UserJSON CreateFromJSON(string data) {
			return JsonUtility.FromJson<UserJSON> (data);
		}
	}

	[Serializable]
	public class PositionJSON
	{
		public float[] position;

		public PositionJSON (Vector3 positionVector) {
			position = new float[] {positionVector.x, positionVector.y, positionVector.z};
		}
	}

	[Serializable]
	public class RotationJSON
	{
		public float rotation;

		public RotationJSON (Quaternion rotationAngle) {
			rotation = rotationAngle.eulerAngles.y;
		}
	}

	// Notifies server when bullet contacted a player
	[Serializable]
	public class HealthChangeJSON {
		public string name;
		public float healthChange;
		public string from;

		public HealthChangeJSON (string _name, float _healthChange, string _from) {
			name = _name;
			healthChange = _healthChange;
			from = _from;
		}
	}

	// Instantiates Bullet
	[Serializable]
	public class ShootJSON {
		public string name;

		public static ShootJSON CreateFromJSON(string data) {
			return JsonUtility.FromJson<ShootJSON> (data);
		}
	}

	// Reports to clients about health
	[Serializable]
	public class UserHealthJSON {
		public string name;
		public float health;

		public static UserHealthJSON CreateFromJSON (string data) {
			return JsonUtility.FromJson<UserHealthJSON> (data);
		}
	}




	#endregion

}
