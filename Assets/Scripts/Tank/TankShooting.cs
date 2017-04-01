using UnityEngine;
using UnityEngine.UI;

public class TankShooting : MonoBehaviour
{      
	public Rigidbody m_Shell;            
	public Transform m_FireTransform;    
	public Slider m_AimSlider;           
	public AudioSource m_ShootingAudio;  
	public AudioClip m_ChargingClip;     
	public AudioClip m_FireClip;         
	public float m_MinLaunchForce = 15f; 
	public float m_MaxLaunchForce = 30f; 
	public float m_MaxChargeTime = 0.75f;
	public bool isLocalPlayer = false;
	public string playerName;

	private string m_FireButton = "Fire";         
	private float m_CurrentLaunchForce;  
	private float m_ChargeSpeed;         
	private bool m_Fired;


	private void OnEnable()
	{
		m_CurrentLaunchForce = m_MinLaunchForce;
		m_AimSlider.value = m_MinLaunchForce;
	}


	private void Start()
	{
		m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
	}


	private void Update()
	{
		if (!isLocalPlayer) {
			return;
		}

		// Track the current state of the fire button and make decisions based on the current launch force.
		m_AimSlider.value = m_MinLaunchForce;

		if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired) {
			// At max charged and not fired
			m_CurrentLaunchForce = m_MaxLaunchForce;
//			CmdFire ();
			NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager> ();
			n.CommandShoot (m_CurrentLaunchForce);
		} else if (Input.GetButtonDown (m_FireButton)) {
			// Have we just pressed button for the first time?
			m_Fired = false;
			m_CurrentLaunchForce = m_MinLaunchForce;

			m_ShootingAudio.clip = m_ChargingClip;
			m_ShootingAudio.Play ();

		} else if (Input.GetButton (m_FireButton) && !m_Fired) {
			// Holding Fire Button
			m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

			m_AimSlider.value = m_CurrentLaunchForce;
		} else if (Input.GetButtonUp (m_FireButton) && !m_Fired) {
			// Released Fire Button
//			CmdFire();
			NetworkManager n = NetworkManager.instance.GetComponent<NetworkManager> ();
			n.CommandShoot (m_CurrentLaunchForce);
		}
	}


	public void CmdFire()
	{
		// Instantiate and launch the shell.
		m_Fired = true;

		print("Player: " + playerName);
		print ("m_FireTransform: " + m_FireTransform);
		print ("m_CurrentLaunchForce: " + m_CurrentLaunchForce);

		Rigidbody shellInstance = Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;
		shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

		ShellExplosion s = shellInstance.GetComponent<ShellExplosion> ();
		s.playerFrom = playerName;

		m_ShootingAudio.clip = m_FireClip;
		m_ShootingAudio.Play ();

		m_CurrentLaunchForce = m_MinLaunchForce;
	}

	public void setCurrentLaunchForce(float launchForce) {
		m_CurrentLaunchForce = launchForce;
	}
}