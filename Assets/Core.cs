using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;

public class Core : MonoBehaviour {

	public RobotController RobotObject;

	public Vector3 RobotStartPosition;
	public float SecondsElapsed;
	float TimeStart, TimeEnd;

	public bool IS_NEURAL_PLAYING = false;

	NeuralNet net;
	List<DataSet> trainData = new List<DataSet>();
	void Awake() {
		if(!IS_NEURAL_PLAYING) {
			return;
		}
		net = new NeuralNet(2, 8, 4);
		//Inputs: DOES_SEE_CORNER DOES_SEE_ROBOT
		//Outputs: FWD BWD LEF RIG
		trainData.Add(new DataSet(new double[] { 0, 0 }, new double[] { 0, 0, 1, 0 }));
		trainData.Add(new DataSet(new double[] { 1, 0 }, new double[] { 0, 1, 0, 0 }));
		trainData.Add(new DataSet(new double[] { 0, 1 }, new double[] { 1, 0, 0, 0 }));
		trainData.Add(new DataSet(new double[] { 1, 1 }, new double[] { 0, 1, 0, 0 }));

		net.Train(trainData, 0.01);
	}

	void Start () {
		ResetGame();
	}

	void Update () {
		ProcessUserInput();
		CheckRobotDeath();
	}

	void ProcessUserInput() {
		if(IS_NEURAL_PLAYING) {
			int seesCorner = RobotObject.CornerScannerSensorData ? 1 : 0;
			int seesRobot = RobotObject.RobotScannerSensorData ? 1 : 0;
			double[] values = net.Compute(new double[] { seesCorner, seesRobot });

			float verticalAxis = (float)values[0] - (float)values[1];
			float horizontalAxis = (float)values[3] - (float)values[2];

			RobotObject.Move(verticalAxis);
			RobotObject.Turn(horizontalAxis);
			return;
		}
		RobotObject.Move(Input.GetAxis("Vertical"));
		RobotObject.Turn(Input.GetAxis("Horizontal"));
	}

	void CheckRobotDeath() {
		GameObject UnderObject = RobotObject.GetObjectUnderRobot();
		if(UnderObject == null) {
			TimeEnd = Time.time;
			Debug.Log(TimeEnd - TimeStart);
			ResetGame();
		}
	}

	void ResetGame() {
		RobotObject.transform.position = RobotStartPosition;
		TimeStart = Time.time;
	}
}
