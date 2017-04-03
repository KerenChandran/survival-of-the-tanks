using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using SocketIO;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager instance;
	public Canvas userNameCanvas;

	public SocketIOComponent socket;

	public Text battleRooms;
	public InputField battleRoomIdInput;
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
		string battleRoomId = battleRoomIdInput.text;
		BattleInfoJSON roomData = new BattleInfoJSON (battleRoomId);
		string roomDataString = JsonUtility.ToJson (roomData);


		socket.Emit ("joinRoom", new JSONObject (roomDataString));

		yield return new WaitForSeconds (0.5f);
		socket.Emit ("playerConnected", new JSONObject(roomDataString));
		yield return new WaitForSeconds (1f);

		PlayerJSON playerJSON = new PlayerJSON (playerNameInput.text, battleRoomId);
		string data = JsonUtility.ToJson (playerJSON);
		socket.Emit("play", new JSONObject(data));

		userNameCanvas.gameObject.SetActive (false);
	}

	public void CommandMove(Vector3 movement) {
		string data = JsonUtility.ToJson (new PositionJSON(movement));
		socket.Emit("playerMove", new JSONObject(data));
	}

	public void CommandRotate(Quaternion rotation) {
		string data = JsonUtility.ToJson (new RotationJSON(rotation));
		socket.Emit("playerRotate", new JSONObject(data));
	}

	public void CommandShoot(float launchForce) {
		string data = JsonUtility.ToJson (new LaunchForceJSON(launchForce));
		socket.Emit ("playerShoot", new JSONObject(data));
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
		Color playerColor;
		ColorUtility.TryParseHtmlString (userData.playerColor, out playerColor);

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

		MeshRenderer[] renderers = p.GetComponentsInChildren<MeshRenderer> ();
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = playerColor;
		}
	}

	void onPlay(SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.data.ToString ();
		UserJSON currentUserData = UserJSON.CreateFromJSON (data);
		Vector3 position = new Vector3 (currentUserData.position [0], currentUserData.position [1], currentUserData.position [2]);
		Quaternion rotation = Quaternion.Euler (0f, currentUserData.rotation, 0f);
		Color playerColor;
		ColorUtility.TryParseHtmlString (currentUserData.playerColor, out playerColor);

		GameObject p = Instantiate(player, position, rotation) as GameObject;
		p.name = currentUserData.name;

		TankShooting shooting = p.GetComponent<TankShooting> ();
		shooting.isLocalPlayer = true;
		shooting.playerName = currentUserData.name;

		TankMovement movement = p.GetComponent<TankMovement> ();
		movement.isLocalPlayer = true;

		MeshRenderer[] renderers = p.GetComponentsInChildren<MeshRenderer> ();
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = playerColor;
		}
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
		s.setCurrentLaunchForce (shootData.launchForce);
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

	// Notifies when another player joins the game
	[Serializable]
	public class UserJSON
	{
		public string name;
		public float[] position;
		public float rotation;
		public float health;
		public string playerColor;

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

	[Serializable]
	public class LaunchForceJSON
	{
		public float launchForce;

		public LaunchForceJSON (float launchForce) {
			this.launchForce = launchForce;
		}
	}

	// Instantiates Bullet
	[Serializable]
	public class ShootJSON {
		public string name;
		public float launchForce;

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

	[Serializable]
	public class PlayerJSON
	{
		public string name;
		public string roomId;

		public PlayerJSON(string _name, string _roomId) {
			name = _name;
			roomId = _roomId;
		}
	}

	[Serializable]
	public class BattleInfoJSON
	{
		public string roomId;

		public BattleInfoJSON(string _battleRoomId) {
			roomId = _battleRoomId;
		}
	}

	[Serializable]
	public class RoomsJSON {
		public RoomJSON[] rooms;

		public static RoomsJSON CreateFromJSON (string data) {
			return JsonUtility.FromJson<RoomsJSON> (data);
		}
	}

	[Serializable]
	public class RoomJSON {
		public string name;
		public string id;

		public RoomJSON (string _name, string _id) {
			name = _name;
			id = _id;
		}
	}

	#endregion

}
