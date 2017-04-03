/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SocketIO;
using System;

public class UIManager : MonoBehaviour {
	public SocketIOComponent socket;
	public InputField playerNameInput;
	public Canvas userNameCanvas;
	public Canvas battleRoomCanvas;
	public Text battleRooms;

	void Start () {
		socket.On ("availableRooms", availableRooms);
	}

	public void availableRooms (SocketIOEvent socketIOEvent) {
		string data = socketIOEvent.ToString ();
		RoomsJSON roomsData = RoomsJSON	.CreateFromJSON (data);

		string output = "";
		for (int i = 0; i < roomsData.rooms.Length; i++) {
			output += roomsData.rooms [i].roomId + " - " + roomsData.rooms [i].name;
		}

		battleRooms.text = output;
	}

	public void sendUserName () {
		string playerName = playerNameInput.text;
		PlayerJSON playerJSON = new PlayerJSON (playerName);
		string data = JsonUtility.ToJson (playerJSON);
		socket.Emit ("updatePlayerName", new JSONObject (data));

		userNameCanvas.gameObject.SetActive (false);
		NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager> ();
		n.playerName = playerName;
	}

	[Serializable]
	public class PlayerJSON
	{
		public string name;

		public PlayerJSON(string _name) {
			name = _name;
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
		public string roomId;

		public RoomJSON (string _name, string _roomId) {
			name = _name;
			roomId = _roomId;
		}
	}
}
*/