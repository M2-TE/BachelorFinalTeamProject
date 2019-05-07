
using Networking;
using System;

class Program
{
	static void Main(string[] args)
	{
		MonoServer server = new MonoServer();
		server.Start();

		Console.ReadLine();
	}
}