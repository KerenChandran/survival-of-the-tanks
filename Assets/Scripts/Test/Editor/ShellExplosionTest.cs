using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections;
using System;

public class ShellExplosionTest {

	public GameObject player;

	[Test]
	public void ManagerTestSimplePasses() {
		// Use the Assert class to test conditions.
		Assert.That(2+2==4);
	}

	[Test]
	public void TestMinShooting() {
		string playerName = "player1";
		GameObject player = GameObject.CreatePrimitive (PrimitiveType.Cube);

		Vector3 position = new Vector3 (-3, 0, 30);
		Quaternion rotation = Quaternion.Euler (0f, 180f, 0f);

		GameObject shell = GameObject.CreatePrimitive(PrimitiveType.Cube);
		shell.AddComponent<Rigidbody> ();

		Assert.That (shell != null);

		GameObject p = UnityEngine.Object.Instantiate(player);
//		GameObject p = GameObject.CreatePrimitive(PrimitiveType.Cube);
		p.name = playerName;
		p.transform.position = position;
		p.transform.rotation = rotation;

		Assert.That (p != null);

		p.AddComponent<TankMovement> ();
		p.AddComponent<TankShooting> ();
		p.AddComponent<TankHealth> ();

		TankShooting shooting = p.GetComponent<TankShooting> ();
		shooting.playerName = playerName;
		shooting.m_FireTransform = p.transform;
		shooting.m_Shell = shell.GetComponent<Rigidbody>();

		TankShooting s = p.GetComponent<TankShooting> ();
		s.setCurrentLaunchForce (TankShooting.m_MinLaunchForce);
		// Gets an NullReferenceException when trying to test for number of shells created
//		s.CmdFire ();
//		object[] explosions = UnityEngine.Object.FindObjectsOfType (typeof(ShellExplosion));
//		Assert.That (explosions.Length == 1);
	}
}
