using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMovement : MonoBehaviour {

	public float m_MovementSpeed = 5f;
	public float m_TurnSpeed = 45f;
	public float m_CameraDistance = 16f;
	public float m_CameraHeight = 16f;

	public AudioClip m_EngineIdling;
	public AudioClip m_EngineDriving;
	public AudioSource m_MovementAudio;

	private float m_OriginalPitch;
	public float m_PitchRange = 0.2f;
	public bool isLocalPlayer = false;

	private float m_MoveAmount;
	private float m_TurnAmount;
	private Rigidbody m_Rigidbody;

	private Vector3 m_OldMovement;
	private Vector3 m_Movement;
	private Quaternion m_OldTurn;
	private Quaternion m_Turn;

	private Transform m_MainCamera;
	private Vector3 m_CameraOffset;


	// Use this for initialization
	void Start () {
		m_OriginalPitch = m_MovementAudio.pitch;

		m_Rigidbody = GetComponent<Rigidbody> ();

		m_CameraOffset = new Vector3 (0f, m_CameraHeight, -m_CameraDistance);

		m_MainCamera = Camera.main.transform;

		MoveCamera ();


		m_OldMovement = transform.position;
		m_Movement = m_OldMovement;

		m_OldTurn = transform.rotation;
		m_Turn = m_OldTurn;
	}
		
	private void Update()
	{
		if (!isLocalPlayer) {
			return;
		}
		// Store the player's input and make sure the audio for the engine is playing.
		m_TurnAmount = Input.GetAxis ("Horizontal");
		m_MoveAmount = Input.GetAxis ("Vertical");

		// Move and turn the tank.
		Move ();
		Turn ();
		MoveCamera ();

		EngineAudio ();
	}


	private void EngineAudio()
	{
		// Play the correct audio clip based on whether or not the tank is moving and what audio is currently playing.
		if (Mathf.Abs (m_MoveAmount) < 0.1f && Mathf.Abs (m_TurnAmount) < 0.1f) {
			if (m_MovementAudio.clip == m_EngineDriving) {
				m_MovementAudio.clip = m_EngineIdling;
				m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
				m_MovementAudio.Play ();
			}
		} else {
			if (m_MovementAudio.clip == m_EngineIdling) {
				m_MovementAudio.clip = m_EngineDriving;
				m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
				m_MovementAudio.Play ();
			}
		}

	}

//	private void FixedUpdate()
//	{
//		if (!isLocalPlayer) {
//			return;
//		}
//		// Move and turn the tank.
//		Move ();
//		Turn ();
//		MoveCamera ();
//	}


	private void Move()
	{
		// Adjust the position of the tank based on the player's input.
		m_Movement = transform.forward * m_MoveAmount * m_MovementSpeed * Time.deltaTime;

		if (m_OldMovement != m_Movement) {
			m_OldMovement = m_Movement;
			NetworkManager.instance.GetComponent<NetworkManager> ().CommandMove (m_Rigidbody.position + m_Movement);
		}

		m_Rigidbody.MovePosition (m_Rigidbody.position + m_Movement);
	}


	private void Turn()
	{
		// Adjust the rotation of the tank based on the player's input.
		float turn = m_TurnAmount * m_TurnSpeed * Time.deltaTime;
		m_Turn = Quaternion.Euler (0f, turn, 0f);

		if (m_OldTurn != m_Turn) {
			m_OldTurn = m_Turn;
			NetworkManager.instance.GetComponent<NetworkManager> ().CommandRotate (m_Rigidbody.rotation * m_Turn);
		}


		m_Rigidbody.MoveRotation (m_Rigidbody.rotation * m_Turn);
	}

	private void MoveCamera() {
		if (isLocalPlayer) {
			m_MainCamera.position = transform.position;
			m_MainCamera.rotation = transform.rotation;
			m_MainCamera.Translate (m_CameraOffset);
			m_MainCamera.LookAt (transform);
		}
	}
}
