using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

[Serializable]
public class NeuralNetwork : IComparable<NeuralNetwork>
{

	//Layers, neurons and weights objects
	private int[] layers;
	private float[][] neurons;
	private float[][][] weights;
	private float fitness;
	public float prevFitness;

	public string Name = "Unnamed Network";

	//Initialize with setting a number of neurons in each layer
	public NeuralNetwork(int[] layers)
	{
		this.layers = new int[layers.Length];
		for (int i = 0; i < layers.Length; i++)
		{
			this.layers[i] = layers[i];
		}

		//Initialize neurons and weights
		InitNeurons();
		InitWeights();
	}


	//Initialize with copy of other neural network
	public NeuralNetwork(NeuralNetwork copy)
	{
		this.layers = new int[copy.layers.Length];
		for (int i = 0; i < copy.layers.Length; i++)
		{
			this.layers[i] = copy.layers[i];
		}
		InitNeurons();
		InitWeights();

		for (int i = 0; i < copy.weights.Length; i++)
		{
			for (int j = 0; j < copy.weights[i].Length; j++)
			{
				for (int k = 0; k < copy.weights[i][j].Length; k++)
				{
					this.weights[i][j][k] = copy.weights[i][j][k];
				}
			}
		}

		this.Name = copy.Name;
	}

	void InitNeurons()
	{
		//Create list of values of neuron on each layer
		List<float[]> neuronList = new List<float[]>();

		//Create everything
		for (int i = 0; i < layers.Length; i++)
		{
			neuronList.Add(new float[layers[i]]);
		}

		//Assign it to our object with conversion to array
		neurons = neuronList.ToArray();
	}


	void InitWeights()
	{
		//Create list of weights on each layer for each connection between neurons
		List<float[][]> weightsList = new List<float[][]>();

		//Iterate through layers (without the Input layer)
		for (int i = 1; i < layers.Length; i++)
		{
			List<float[]> layerWeightList = new List<float[]>();

			int neuronsInPreviousLayer = layers[i - 1];

			for (int j = 0; j < neurons[i].Length; j++)
			{
				float[] neuronWeights = new float[neuronsInPreviousLayer];

				for (int k = 0; k < neuronWeights.Length; k++)
				{
					neuronWeights[k] = Random.Range(-0.5f, 0.5f);
				}

				layerWeightList.Add(neuronWeights);
			}

			weightsList.Add(layerWeightList.ToArray());
		}

		weights = weightsList.ToArray();
	}

	public float[] FeedForward(float[] inputs)
	{
		//Set values for Input layer
		for (int i = 0; i < inputs.Length; i++)
		{
			neurons[0][i] = inputs[i];
		}

		//Calculate values for layers down below
		for (int i = 1; i < layers.Length; i++)
		{
			for (int j = 0; j < neurons[i].Length; j++)
			{

				//Calculate weighted sum
				float value = 0.25f;
				for (int k = 0; k < neurons[i - 1].Length; k++)
				{
					value += weights[i - 1][j][k] * neurons[i - 1][k];
				}

				//Pass value to normalizer function
				neurons[i][j] = (float)Math.Tanh(value);
			}
		}

		//Return Output layer
		return neurons[neurons.Length - 1];
	}

	public void Mutate()
	{
		//Mutate weights
		for (int i = 0; i < weights.Length; i++)
		{
			for (int j = 0; j < weights[i].Length; j++)
			{
				for (int k = 0; k < weights[i][j].Length; k++)
				{
					float weight = weights[i][j][k];

					float randNumber = Random.Range(0f, 1f) * 100f;

					if (randNumber <= 2f)
					{
						weight *= -1f;
					}
					else if (randNumber <= 4f)
					{
						weight = UnityEngine.Random.Range(-0.5f, 0.5f);
					}
					else if (randNumber <= 6f)
					{
						float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
						weight *= factor;
					}
					else if (randNumber <= 8f)
					{
						float factor = UnityEngine.Random.Range(0f, 1f);
						weight *= factor;
					}

					weights[i][j][k] = weight;
				}
			}
		}
	}

	public void AddFitness(float fit)
	{
		fitness += fit;
	}

	public void SetFitness(float fit)
	{
		fitness = fit;
	}

	public float GetFitness()
	{
		return fitness;
	}

	public int CompareTo(NeuralNetwork other)
	{
		if (other == null)
		{
			return 1;
		}

		if (fitness > other.fitness)
		{
			return 1;
		}
		else if (fitness < other.fitness)
		{
			return -1;
		}

		if (prevFitness > other.prevFitness)
		{
			return 1;
		}
		else if(other.prevFitness > prevFitness) {
			return -1;
		}
		return 0;
	}

	public string ExportToString()
	{
		string s = Name + "\n";
		s += layers.Length.ToString() + "\n";
		for (int i = 0; i < layers.Length; i++) {
			s += layers[i].ToString() + " ";
		}
		s += "\n";
		for (int i = 0; i < weights.Length; i++)
		{
			for (int j = 0; j < weights[i].Length; j++)
			{
				for (int k = 0; k < weights[i][j].Length; k++)
				{
					s += weights[i][j][k].ToString() + " ";
				}
			}
		}
		return s;
	}

	public NeuralNetwork(string s) {
		string[] lines = s.Split('\n');

		Name = lines[0];
		layers = new int[Convert.ToInt32(lines[1])];
		string[] layerNumbers = lines[2].Split(' ');

		for (int i = 0; i < layerNumbers.Length - 1; i++) {
			layers[i] = Convert.ToInt32(layerNumbers[i]);
		}

		InitNeurons();
		InitWeights();

		string[] weightNumbers = lines[3].Split(' ');
		int weightNum = 0;
		for (int i = 0; i < weights.Length; i++)
		{
			for (int j = 0; j < weights[i].Length; j++)
			{
				for (int k = 0; k < weights[i][j].Length; k++)
				{
					weights[i][j][k] = Convert.ToSingle(weightNumbers[weightNum]);
					weightNum++;
				}
			}
		}
	}
}
