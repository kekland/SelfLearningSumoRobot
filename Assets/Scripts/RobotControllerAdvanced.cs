using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RobotControllerAdvanced : MonoBehaviour {
	//Fwd-Right-Bottom-Left
	public GameObject[] CornerScanner;
	MeshRenderer[] CornerScannerRenderer;

	public GameObject[] RobotScanner;
	MeshRenderer[] RobotScannerRenderer;

	public float Velocity;
	public float AngularVelocity;

	public bool[] CornerScannerSensorData = new bool[4];
	public bool[] RobotScannerSensorData = new bool[4];
	public bool IsInContact;

	public LayerMask CornerScannerLayerMask;
	public LayerMask RobotScannerLayerMask;
	Rigidbody robotRB;

	public Text RobotNameText;
	void Start()
	{
		robotRB = GetComponent<Rigidbody>();
		CornerScannerRenderer = new MeshRenderer[CornerScanner.Length];
		RobotScannerRenderer = new MeshRenderer[RobotScanner.Length];
		for (int i = 0; i < CornerScanner.Length; i++)
		{
			CornerScannerRenderer[i] = CornerScanner[i].GetComponent<MeshRenderer>();
		}
		for (int i = 0; i < RobotScanner.Length; i++)
		{
			RobotScannerRenderer[i] = RobotScanner[i].GetComponent<MeshRenderer>();
		}
	}

	void Update()
	{
		CheckCornerScanners();
		CheckRobotScanners();
		ModifyScannerMaterial();
	}

	void CheckCornerScanners()
	{
		for (int i = 0; i < CornerScanner.Length; i++)
		{
			Ray ray = new Ray(CornerScanner[i].transform.position, new Vector3(0f, -1f, 0f));
			CornerScannerSensorData[i] = Physics.Raycast(ray, CornerScannerLayerMask.value);
		}
	}

	void CheckRobotScanners()
	{
		for (int i = 0; i < RobotScanner.Length; i++)
		{
			float thickness = 0.5f;
			Ray ray = new Ray(RobotScanner[i].transform.position, RobotScanner[i].transform.forward);
			RobotScannerSensorData[i] = !Physics.SphereCast(ray, thickness, 9f, RobotScannerLayerMask);
		}
	}

	public bool IsAnyRobotSensorActivated() {
		for (int i = 0; i < RobotScannerSensorData.Length; i++) {
			if (RobotScannerSensorData[i]) { return true; }
		}
		return false;
	}

	public Material ScannerHitCornerMaterial;
	public Material ScannerDefaultMaterial;
	void ModifyScannerMaterial()
	{
		for (int i = 0; i < RobotScanner.Length; i++)
		{
			//If scanner did hit corner - substitute material with red, otherwise - green
			CornerScannerRenderer[i].material = CornerScannerSensorData[i] ? ScannerDefaultMaterial : ScannerHitCornerMaterial;
			RobotScannerRenderer[i].material = RobotScannerSensorData[i] ? ScannerDefaultMaterial : ScannerHitCornerMaterial;
		}
	}

	public void Move(float VelocityMultiplier)
	{
		VelocityMultiplier = Mathf.Clamp(VelocityMultiplier, -1f, 1f);
		//robotRB.AddRelativeForce(new Vector3(0f, 0f, VelocityMultiplier * Velocity));
		robotRB.MovePosition(transform.position + (VelocityMultiplier * Velocity) * transform.forward * Time.deltaTime);
		//transform.position += (VelocityMultiplier * Velocity) * transform.forward * Time.deltaTime;
	}

	public void Turn(float AngularMultiplier)
	{
		AngularMultiplier = Mathf.Clamp(AngularMultiplier, -1f, 1f);
		//robotRB.AddRelativeTorque(0f, AngularVelocity * AngularMultiplier, 0f);
		robotRB.MoveRotation(Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, AngularVelocity * AngularMultiplier * Time.deltaTime, 0f)));
		//transform.Rotate(0f, AngularVelocity * AngularMultiplier * Time.deltaTime, 0f);
	}

	public GameObject GetObjectUnderRobot()
	{
		RaycastHit hit;

		//Construct ray object
		Ray ray = new Ray(transform.position, new Vector3(0f, -1f, 0f));
		if (Physics.Raycast(ray, out hit))
		{
			//If scanner hit corner - return true and vice versa
			return hit.collider.gameObject;
		}
		return null;
	}

	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.layer == 9)
		{
			IsInContact = true;
		}
	}

	void OnCollisionExit(Collision other)
	{
		if (other.gameObject.layer == 9)
		{
			IsInContact = false;
		}
	}
}
