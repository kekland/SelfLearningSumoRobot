using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{

	public GameObject CornerScanner;
	MeshRenderer CornerScannerRenderer;

	public GameObject RobotScanner;
	MeshRenderer RobotScannerRenderer;

	public float Velocity;
	public float AngularVelocity;

	public bool CornerScannerSensorData;
	public bool RobotScannerSensorData;

	public LayerMask CornerScannerLayerMask;
	public LayerMask RobotScannerLayerMask;
	Rigidbody robotRB;
	void Start()
	{
		robotRB = GetComponent<Rigidbody>();
		CornerScannerRenderer = CornerScanner.GetComponent<MeshRenderer>();
		RobotScannerRenderer = RobotScanner.GetComponent<MeshRenderer>();
	}

	void Update()
	{
		CornerScannerSensorData = DidCornerScannerHitCorner();
		RobotScannerSensorData = DidRobotScannerHitRobot();
		ModifyScannerMaterial();
	}

	bool DidCornerScannerHitCorner()
	{

		//Construct ray object
		Ray ray = new Ray(CornerScanner.transform.position, new Vector3(0f, -1f, 0f));
		return !Physics.Raycast(ray, CornerScannerLayerMask.value);
	}

	bool DidRobotScannerHitRobot()
	{
		RaycastHit hit;

		//Construct ray object
		Ray ray = new Ray(RobotScanner.transform.position, transform.forward);
		return Physics.Raycast(ray, RobotScannerLayerMask.value);
	}


	public Material ScannerHitCornerMaterial;
	public Material ScannerDefaultMaterial;
	void ModifyScannerMaterial()
	{
		//If scanner did hit corner - substitute material with red, otherwise - green
		CornerScannerRenderer.material = CornerScannerSensorData ?  ScannerDefaultMaterial : ScannerHitCornerMaterial;
		RobotScannerRenderer.material = RobotScannerSensorData ? ScannerDefaultMaterial : ScannerHitCornerMaterial;
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
}
