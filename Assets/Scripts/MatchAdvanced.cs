using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchAdvanced : MonoBehaviour {
	public int Index;

	public RobotControllerAdvanced Robot1;
	public RobotControllerAdvanced Robot2;

	public Vector3 FirstRobotStartPosition, SecondRobotStartPosition;
	public Vector3 FirstRobotStartRotation, SecondRobotStartRotation;
	public float BonusPointsForFirst, BonusPointsForSecond;

	public NeuralNetwork NetworkForRobot1;
	public NeuralNetwork NetworkForRobot2;

	public bool MatchFinished;

	void Start()
	{
		transform.position = Index * new Vector3(20f, 0, 0);
	}

	public void UpdateInit()
	{
		if (MatchFinished) {
			StopGame();
			return; }
		ProcessNeuralOutput(Robot1, NetworkForRobot1);
		ProcessNeuralOutput(Robot2, NetworkForRobot2);

		if (Robot1.IsInContact && Robot1.IsAnyRobotSensorActivated())
		{
			BonusPointsForFirst += 0.01f;
		}
		if (Robot2.IsInContact && Robot2.IsAnyRobotSensorActivated())
		{
			BonusPointsForSecond += 0.01f;
		}

		if (CheckRobotDeath(Robot1))
		{
			NetworkForRobot2.AddFitness(1f + BonusPointsForSecond);
			NetworkForRobot1.AddFitness(-2f);
			MatchFinished = true;
		}
		else if (CheckRobotDeath(Robot2))
		{
			NetworkForRobot1.AddFitness(1f + BonusPointsForFirst);
			NetworkForRobot2.AddFitness(-2f);
			MatchFinished = true;
		}

		Robot1.RobotNameText.text = NetworkForRobot1.Name;
		Robot2.RobotNameText.text = NetworkForRobot2.Name;
	}

	bool CheckRobotDeath(RobotControllerAdvanced RobotObject)
	{
		GameObject UnderObject = RobotObject.GetObjectUnderRobot();
		if (UnderObject == null)
		{
			return true;
		}
		return false;
	}
	void ProcessNeuralOutput(RobotControllerAdvanced RobotObject, NeuralNetwork network)
	{
		List<float> input = new List<float>();
		for (int i = 0; i < RobotObject.CornerScannerSensorData.Length; i++) {
			input.Add(RobotObject.CornerScannerSensorData[i]? 1 : 0);
		}
		for (int i = 0; i < RobotObject.RobotScannerSensorData.Length; i++)
		{
			input.Add(RobotObject.RobotScannerSensorData[i] ? 1 : 0);
		}

		float[] values = network.FeedForward(input.ToArray());

		RobotObject.Move(values[0]);
		RobotObject.Turn(values[1]);
		return;
	}

	void StopGame() {
		Robot1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		Robot2.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
	}

	public void Reset()
	{
		Robot1.transform.localPosition = FirstRobotStartPosition;
		Robot2.transform.localPosition = SecondRobotStartPosition;

		Robot1.transform.rotation = Quaternion.Euler(FirstRobotStartRotation);
		Robot2.transform.rotation = Quaternion.Euler(SecondRobotStartRotation);

		BonusPointsForFirst = 0;
		BonusPointsForSecond = 0;

		Robot1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
		Robot2.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
	}
}
