using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match : MonoBehaviour {
	public int Index;

	public RobotController Robot1;
	public RobotController Robot2;

	public Vector3 FirstRobotStartPosition, SecondRobotStartPosition;
	public Vector3 FirstRobotStartRotation, SecondRobotStartRotation;
	public float BonusPointsForFirst, BonusPointsForSecond;

	public NeuralNetwork NetworkForRobot1;
	public NeuralNetwork NetworkForRobot2;

	public bool MatchFinished;

	void Start() {
		transform.position = Index * new Vector3(20f, 0, 0);
	}

	public void UpdateInit() {
		if (MatchFinished) { return; }
		ProcessNeuralOutput(Robot1, NetworkForRobot1);
		ProcessNeuralOutput(Robot2, NetworkForRobot2);

		if(Robot1.IsInContact && Robot1.RobotScannerSensorData) {
			BonusPointsForFirst += 0.01f;
		}
		if(Robot2.IsInContact && Robot2.RobotScannerSensorData) {
			BonusPointsForSecond += 0.01f;
		}

		if(CheckRobotDeath(Robot1)) {
			NetworkForRobot2.AddFitness(1f + BonusPointsForSecond);
			NetworkForRobot1.AddFitness(-2f);
			MatchFinished = true;
		}
		else if(CheckRobotDeath(Robot2)) {
			NetworkForRobot1.AddFitness(1f + BonusPointsForFirst);
			NetworkForRobot2.AddFitness(-2f);
			MatchFinished = true;
		}

		Robot1.RobotNameText.text = NetworkForRobot1.Name;
		Robot2.RobotNameText.text = NetworkForRobot2.Name;
	}

	bool CheckRobotDeath(RobotController RobotObject)
	{
		GameObject UnderObject = RobotObject.GetObjectUnderRobot();
		if (UnderObject == null)
		{
			return true;
		}
		return false;
	}
	void ProcessNeuralOutput(RobotController RobotObject, NeuralNetwork network)
	{
		int seesCorner = RobotObject.CornerScannerSensorData ? 1 : 0;
		int seesRobot = RobotObject.RobotScannerSensorData ? 1 : 0;
		float[] values = network.FeedForward(new float[] { seesCorner, seesRobot });

		RobotObject.Move(values[0]);
		RobotObject.Turn(values[1]);
		return;
	}

	public void Reset() {
		Robot1.transform.localPosition = FirstRobotStartPosition;
		Robot2.transform.localPosition = SecondRobotStartPosition;

		Robot1.transform.rotation = Quaternion.Euler(FirstRobotStartRotation);
		Robot2.transform.rotation = Quaternion.Euler(SecondRobotStartRotation);

		BonusPointsForFirst = 0;
		BonusPointsForSecond = 0;
	}
}
