using System;
using TeamSpeak3Api;

namespace TSBot
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      start();
      Console.ReadLine();
    }

    public static async void start()
    {
      //var botname = "B1GSt4R | Bot";
      var host = "176.9.155.44";
      var rc = new TeamSpeakClient(host, 10011);
      await rc.Connect();
      await rc.Login("B1GSt4R", "BoFCrtQ7");
      await rc.UseServer(1);
      //var me = await rc.WhoAmI();
    }
  }
}
