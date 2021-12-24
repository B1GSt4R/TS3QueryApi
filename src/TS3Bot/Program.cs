using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TeamSpeak3QueryApi.Net.Specialized;
using TeamSpeak3QueryApi.Net.Specialized.Notifications;
using TeamSpeak3QueryApi.Net.Specialized.Responses;

namespace TS3Bot
{
  public class Channel
  {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int ChannelId { get; set; }
    public int OwnerId { get; set; }
    public bool IsPublic { get; set; }
    public Channel()
    {
      this.Id = Guid.NewGuid();
    }
    public Channel(int channelId, int ownerId, string name = null) : this()
    {
      this.ChannelId = channelId;
      this.OwnerId = ownerId;
      this.Name = name;
    }
  }
  
  class Program
  {
    static void Main(string[] args)
    {
      Bot bot = new Bot();
      bot.StartTimer();
      Console.ReadKey();
    }
  }

  public class Bot : IDisposable
  {
    public static List<Channel> Channels = new List<Channel>()
    {
      new Channel(43, 5, "B1GSt4R"),
      new Channel(4, 6, "SpLaSh"),
      new Channel(3, 21, "Krypt0n"),
    };

    public static List<int> InhaberListe = new List<int>()
    {
      5,
      6,
      21,
      99,
      //120
    };

    public TeamSpeakClient rc { get; set; } = null;

    public void Dispose()
    {
      rc.Dispose();
    }

    public async void StartTimer()
    {
      Start();
      int min = 1;
      while (true)
      {
        await Task.Delay(min * 60 * 1000);
        var me = await rc.WhoAmI(); 
        var ts = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        Console.WriteLine($"[{ts}] Bot Refreshed");
        //this.Dispose();
      }
    }

    public async void Start()
    {
      var botname = "B1GSt4R | Bot";
      var host = "176.9.155.44";
      rc = new TeamSpeakClient(host, 10011);
      await rc.Connect();
      await rc.Login("B1GSt4R", "BoFCrtQ7");
      await rc.UseServer(1);
      var me = await rc.WhoAmI();
      if (me != null && !me.NickName.Contains(botname))
        await rc.ChangeNickName(botname, me);
      await rc.RegisterServerNotification();
      await rc.RegisterChannelNotification(4);
      //await rc.RegisterChannelNotification(3);
      //await rc.RegisterChannelNotification(43);

      var channels = await rc.GetChannels();
      for (int i = 0; i < Channels.Count; i++)
      {
        var x = channels.ToList().Where(x => x.Id.Equals(Channels[i].ChannelId)).FirstOrDefault();
        //Channels[i].IsPublic = x.;
      }
      var ts = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
      Console.WriteLine($"[{ts}] init done");

      rc.Subscribe<ChannelEdited>(data =>
      {
        foreach (var c in data)
        {
          int index = Channels.FindIndex(x => x.ChannelId.Equals(c.ChannelId));
          Channels[index].IsPublic = c.IsUnencrypted;
        }
      });

      rc.Subscribe<ClientMoved>(async data =>
      {
        foreach (var c in data)
        {
          var clients = await rc.GetClients();
          Channel current = Channels.Find(x => x.ChannelId.Equals(c.TargetChannel));
          if (current != null)
          {
            var channelClients = clients.Where(x => x.ChannelId.Equals(c.TargetChannel)).ToList();
            var owner = channelClients.Find(x => x.DatabaseId.Equals(current.OwnerId));
            if (owner == null || !current.IsPublic)
            {
              var uid = await rc.ClientUidFromClientId(c.ClientIds[0]);
              var dbid = await rc.DatabaseIdFromClientUid(uid.ClientUid);
              bool ownerJoin = current.OwnerId.Equals(dbid) || InhaberListe.Contains(dbid.ClientDatabaseId);
              if (!ownerJoin && c.InvokerId == 0)
              {
                var kickmsg = "Solange der Channel Eigentümer nicht anwesend ist und der Channel Private ist darfst du nicht beitreten!";
                await rc.KickClient(c.ClientIds, KickOrigin.Channel, kickmsg);

                var ts = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                Console.WriteLine($"[{ts}] Client " + uid.Nickname + $" Moved. \n" + JsonConvert.SerializeObject(c));
                //foreach (var clientid in c.ClientIds)
                //{

                //  await rc.PokeClient(clientid, kickmsg);
                //}
              }
            }
          }
        }
        //clients = clients.Where(x => x.Type == ClientType.FullClient && Channels.Find(s => s.ChannelId.Equals(x.ChannelId)) != null).ToList();
        //GetClientInfo owner = clients.Where(x => Channels.Find(s => s.OwnerId.Equals(x.Id)) != null).FirstOrDefault();
        //Console.WriteLine(owner.GetType());
      });
      rc.Subscribe<ClientEnterView>(data =>
      {
        foreach (var c in data)
        {
          var ts = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
          Console.WriteLine($"[{ts}] Client " + c.NickName + " joined.");
        }
      });
      rc.Subscribe<ClientLeftView>(async data =>
      {
        foreach (var c in data)
        {
          var ts = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
          Console.WriteLine($"[{ts}] Client " + c.Id + $" leaved.\n {JsonConvert.SerializeObject(c)}");
        }
      });
    }
  }
}
