using NeuralNetwork;
public class RobotNetwork {
	public string Name;
	public NeuralNet network;
	public int Victories;

	public RobotNetwork(string Name, int input, int hidden, int output) {
		this.Name = Name;
		network = new NeuralNet(input, hidden, output);
		Victories = 0;
	}

	public RobotNetwork(string Name, NeuralNet n) {
		this.Name = Name;
		network = n;
		Victories = 0;
	}
}