using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;
using UnityEngine.UI;
public class Core : MonoBehaviour
{

	public RobotController NewRobot;
	public RobotController OldRobot;

	public Vector3 FirstRobotStartPosition, SecondRobotStartPosition;
	public Vector3 FirstRobotStartRotation, SecondRobotStartRotation;
	public float SecondsElapsed;
	float TimeStart, TimeEnd;

	public bool IS_NEURAL_PLAYING = false;

	public int RobotCount;
	public List<RobotNetwork> Robots = new List<RobotNetwork>();

	public List<Matchup> Matches = new List<Matchup>();
	int currentMatch = -1;
	int network1, network2;
	int tempRobotIndex = 0;
	int currentGeneration = 0;
	public Text MatchText;
	public Text PlayersText;

	public void AdjustTimeScale(float TimeScale)
	{
		Time.timeScale = TimeScale;
		Time.fixedDeltaTime = 0.02f / TimeScale;
	}
	void Awake()
	{
		if (!IS_NEURAL_PLAYING)
		{
			return;
		}

		for (int i = 0; i < RobotCount; i++)
		{
			Robots.Add(new RobotNetwork("#" + i, 2, 8, 4));
			tempRobotIndex++;
		}

		for (int i = 0; i < RobotCount - 1; i++)
		{
			for (int j = i + 1; j < RobotCount; j++)
			{
				Matches.Add(new Matchup(i, j));
			}
		}

		List<DataSet> trainData = new List<DataSet>();

		trainData.Add(new DataSet(new double[] { 0, 0 }, new double[] { 0, 0, 1, 0 }));
		trainData.Add(new DataSet(new double[] { 1, 0 }, new double[] { 0, 1, 0, 0 }));
		trainData.Add(new DataSet(new double[] { 0, 1 }, new double[] { 1, 0, 0, 0 }));
		trainData.Add(new DataSet(new double[] { 1, 1 }, new double[] { 0, 1, 0, 0 }));

		ResetGame();
	}

	void FixedUpdate()
	{
		if (Time.time - TimeStart > 10.0)
		{
			Robots[network1].Victories--;
			Robots[network2].Victories--;
			ResetGame();
		}

		ProcessUserInput(NewRobot, Robots[network1].network);
		ProcessUserInput(OldRobot, Robots[network2].network);
		if (CheckRobotDeath(NewRobot))
		{
			Robots[network2].Victories++;
			ResetGame();

		}
		else if (CheckRobotDeath(OldRobot))
		{
			Robots[network1].Victories++;
			ResetGame();
		}
	}

	void ProcessUserInput(RobotController RobotObject, NeuralNet network)
	{

		if (IS_NEURAL_PLAYING)
		{
			int seesCorner = RobotObject.CornerScannerSensorData ? 1 : 0;
			int seesRobot = RobotObject.RobotScannerSensorData ? 1 : 0;
			double[] values = network.Compute(new double[] { seesCorner, seesRobot });

			float verticalAxis = (float)values[0] - (float)values[1];
			float horizontalAxis = (float)values[3] - (float)values[2];

			RobotObject.Move(verticalAxis);
			RobotObject.Turn(horizontalAxis);
			return;
		}

		RobotObject.Move(Input.GetAxis("Vertical"));
		RobotObject.Turn(Input.GetAxis("Horizontal"));
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

	RobotsComparer comparer = new RobotsComparer();
	void ResetGame()
	{
		NewRobot.transform.position = FirstRobotStartPosition;
		NewRobot.transform.rotation = Quaternion.Euler(FirstRobotStartRotation);
		OldRobot.transform.position = SecondRobotStartPosition;
		NewRobot.transform.rotation = Quaternion.Euler(SecondRobotStartRotation);

		if (currentMatch == Matches.Count - 1)
		{
			currentGeneration++;
			//Finished, sort and remove losers, etc
			Robots.Sort(comparer);
			List<int> KeptNetworks = new List<int>();
			List<int> RecalcNetworks = new List<int>();
			for (int i = 0; i < RobotCount; i++)
			{
				if ((i < RobotCount * 0.2 || Random.Range(0f, 1f) > 0.9f) && Robots[i].Victories >= 0)
				{
					KeptNetworks.Add(i);
					continue;
				}
				RecalcNetworks.Add(i);
				Robots[i] = null;
			}

			for (int i = 0; i < RecalcNetworks.Count; i++)
			{
				if (KeptNetworks.Count >= 2)
				{
					Robots[RecalcNetworks[i]] = new RobotNetwork("#" + tempRobotIndex, Breed(
						Robots[GetRandom(KeptNetworks)].network, Robots[GetRandom(KeptNetworks)].network
					));
				}
				else {
					Robots[RecalcNetworks[i]] = new RobotNetwork("#" + tempRobotIndex, 2, 8, 4);
				}
				tempRobotIndex++;
			}


			for (int i = 0; i < RobotCount; i++)
			{
				Robots[i].Victories = 0;
				Robots[i].network = Mutate(Robots[i].network);
			}
			currentMatch = 0;
		}

		currentMatch++;
		network1 = Matches[currentMatch].FirstIndex;
		network2 = Matches[currentMatch].SecondIndex;
		TimeStart = Time.time;

		NewRobot.RobotNameText.text = Robots[network1].Name;
		OldRobot.RobotNameText.text = Robots[network2].Name;
		MatchText.text = "Gen #" + currentGeneration + " | Match #" + currentMatch + ": " + Robots[network1].Name + "vs" + Robots[network2].Name;
		string PlayersTextString = "Players: \n";
		for (int i = 0; i < RobotCount; i++)
		{
			PlayersTextString += Robots[i].Name + " : " + Robots[i].Victories + "\n";
		}
		PlayersText.text = PlayersTextString;
	}

	public int GetRandom(List<int> list)
	{
		return list[Random.Range(0, list.Count)];
	}

	public float MutationPercentage = 0.1f;

	public NeuralNet Mutate(NeuralNet n1)
	{
		for (int i = 0; i < n1.InputLayer.Count; i++)
		{
			if (Random.Range(0f, 1f) > 1f - MutationPercentage)
			{
				n1.InputLayer[i].Bias = Random.Range(-1f, 1f);
			}
		}

		for (int i = 0; i < n1.OutputLayer.Count; i++)
		{
			if (Random.Range(0f, 1f) > 1f - MutationPercentage)
			{
				n1.OutputLayer[i].Bias = Random.Range(-1f, 1f);
			}
		}

		for (int i = 0; i < n1.HiddenLayers[0].Count; i++)
		{
			if (Random.Range(0f, 1f) > 1f - MutationPercentage)
			{
				n1.HiddenLayers[0][i].Bias = Random.Range(-1f, 1f);
			}
		}

		return n1;
	}
	public NeuralNet Breed(NeuralNet r1, NeuralNet r2)
	{
		NeuralNet networkNew = new NeuralNet(2, 8, 4);

		for (int i = 0; i < networkNew.InputLayer.Count; i++)
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				networkNew.InputLayer[i] = r2.InputLayer[i];
			}
			else
			{
				networkNew.InputLayer[i] = r1.InputLayer[i];
			}
		}

		for (int i = 0; i < networkNew.HiddenLayers[0].Count; i++)
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				networkNew.HiddenLayers[0][i] = r2.HiddenLayers[0][i];
			}
			else
			{
				networkNew.HiddenLayers[0][i] = r1.HiddenLayers[0][i];
			}
		}

		for (int i = 0; i < networkNew.OutputLayer.Count; i++)
		{
			if (Random.Range(0f, 1f) > 0.5f)
			{
				networkNew.OutputLayer[i] = r2.OutputLayer[i];
			}
			else
			{
				networkNew.OutputLayer[i] = r1.OutputLayer[i];
			}
		}

		return networkNew;
	}
}

public struct Matchup
{
	public int FirstIndex, SecondIndex;
	public Matchup(int i1, int i2)
	{
		FirstIndex = i1;
		SecondIndex = i2;
	}
}


public class RobotsComparer : IComparer<RobotNetwork>
{
	public int Compare(RobotNetwork x, RobotNetwork y)
	{
		if (x.Victories > y.Victories)
		{
			return -1;
		}
		return 1;
	}
}