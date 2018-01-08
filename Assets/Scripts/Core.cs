using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using UnityEngine.UI;
public class Core : MonoBehaviour
{
	float TimeStart;

	public int NetworkCount;
	List<NeuralNetwork> Networks = new List<NeuralNetwork>();

	public Text MatchText;
	public Text PlayersText;

	public GameObject FieldPrefab;
	public List<Match> Matches = new List<Match>();
	public Dictionary<string, float> LifetimeStats = new Dictionary<string, float>();

	int tempValue = 0;
	public int CurrentGeneration = 0;

	int camPosition = 0;

	public void ControlTimeScale(float scale) {
		Time.timeScale = scale;
		Time.fixedDeltaTime = 0.02f / scale;
	}
	void Awake()
	{
		for (int i = 0; i < NetworkCount; i++)
		{
			if(File.Exists(Application.persistentDataPath + "/Data/Network" + i + ".txt")) {
				StreamReader reader = new StreamReader(Application.persistentDataPath + "/Data/Network" + i + ".txt");
				Networks.Add(new NeuralNetwork(reader.ReadToEnd()));
				int index = Convert.ToInt32(Networks[i].Name.Substring(1));
				if(index > tempValue) {
					tempValue = index;
				}
			}
			else {
				Networks.Add(new NeuralNetwork(new int[] { 2, 8, 8, 2 }));
				Networks[i].Name = "#" + tempValue;
				tempValue++;
			}
		}
		tempValue++;

		int matchId = 0;
		for (int i = 0; i < NetworkCount - 1; i++)
		{
			for (int j = i + 1; j < NetworkCount; j++)
			{
				GameObject field = Instantiate(FieldPrefab);
				Matches.Add(field.GetComponent<Match>());
				Matches[matchId].Index = matchId;
				matchId++;
			}
		}

		ResetGame();
	}

	void FixedUpdate() {

		if(Input.GetKey(KeyCode.RightArrow)) {
			Camera.main.transform.localPosition += new Vector3(25f * Time.deltaTime, 0f, 0f);
		}
		else if(Input.GetKey(KeyCode.LeftArrow)) {
			Camera.main.transform.localPosition += new Vector3(-25f * Time.deltaTime, 0f, 0f);
		}
		bool finish = false;
		if(Time.time - TimeStart > 10f) {
			finish = true;
		}

		string PlayersTextString = "Players:\n";

		for (int i = 0; i < NetworkCount; i++)
		{
			PlayersTextString += Networks[i].Name + ": " + Networks[i].GetFitness() + "\n";
		}

		PlayersText.text = PlayersTextString;

		for (int i = 0; i < Matches.Count; i++) {
			Matches[i].UpdateInit();

			if(!Matches[i].MatchFinished && !finish) {
				finish = false;
			}
		}

		if(finish) {
			for (int i = 0; i < Matches.Count; i++)
			{
				if (!Matches[i].MatchFinished)
				{
					Matches[i].NetworkForRobot1.AddFitness(-1);
					Matches[i].NetworkForRobot2.AddFitness(-1);
				}
			}

			ResetGame();
		}
	}

	void ResetGame()
	{
		for (int i = 0; i < Matches.Count; i++)
		{
			Matches[i].Reset();
		}

		CurrentGeneration++;
		TimeStart = Time.time;

		MatchText.text = "Gen #" + CurrentGeneration;

		Networks.Sort();
		for (int i = 0; i < NetworkCount; i++)
		{
			if(!LifetimeStats.ContainsKey(Networks[i].Name)) {
				LifetimeStats.Add(Networks[i].Name, Networks[i].GetFitness());
			}
			else {
				LifetimeStats[Networks[i].Name] += Networks[i].GetFitness();
			}
		}
		for (int i = 0; i < NetworkCount / 2; i++)
		{
			LifetimeStats.Remove(Networks[i].Name);
			Networks[i] = new NeuralNetwork(Networks[i + NetworkCount / 2]);
			Networks[i].Mutate();
			Networks[i].Name = "#" + tempValue;
			tempValue++;
			LifetimeStats.Add(Networks[i].Name, Networks[i].GetFitness());


			Networks[i + NetworkCount / 2] = new NeuralNetwork(Networks[i + NetworkCount / 2]);
		}

		for (int i = 0; i < NetworkCount; i++)
		{
			Networks[i].SetFitness(0);
		}

		string PlayersTextString = "Players:\n";

		foreach(string Key in LifetimeStats.Keys) {
			PlayersTextString += Key + ": " + LifetimeStats[Key] + "\n";
		}

		PlayersText.text = PlayersTextString;

		int matchId = 0;
		for (int i = 0; i < NetworkCount - 1; i++) {
			for (int j = i + 1; j < NetworkCount; j++) {
				Matches[matchId].NetworkForRobot1 = Networks[i];
				Matches[matchId].NetworkForRobot2 = Networks[j];
				Matches[matchId].MatchFinished = false;
				matchId++;
			}
		}
	}

	void OnDisable()
	{
		if (!Directory.Exists(Application.persistentDataPath + "/Data"))
		{
			Directory.CreateDirectory(Application.persistentDataPath + "/Data");
		}
		for (int i = 0; i < NetworkCount; i++)
		{
			StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/Data/Network" + i + ".txt", false);
			writer.Write(Networks[i].ExportToString());
			writer.Close();
		}
	}
}