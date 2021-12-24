using System.Globalization;
using System.Threading.Tasks;
using TeamSpeak3Api.Intern;

namespace TeamSpeak3Api
{
  /// <summary>
  /// Main Class
  /// </summary>
  public class TeamSpeakClient
  {
    public QueryClient Client { get; }
    public string Host { get; set; }
    public int port { get; set; }
    public TeamSpeakClient(string host = "localhost", int port = 10011)
    {
      Host = host;
      this.port = port;
      Client = new QueryClient(host, port);
    }
    public Task Connect()
    {
      return Client.Connect();
    }

    public Task Login(string userName, string password)
    {
      return Client.Send("login", new QueryParameter("client_login_name", userName), new QueryParameter("client_login_password", password));
    }

    //public Task Logout()
    //{
    //  Task.FromResult(0);
    //  //_keepAliveCancellationTokenSource.Cancel();
    //  //return Client.Send("logout");
    //}

    public Task UseServer(int serverId)
    {
      return Client.Send("use", new QueryParameter("sid", serverId.ToString(CultureInfo.InvariantCulture)));
    }

    //public async Task<object> WhoAmI()
    //{
    //  //var res = await Client.Send("whoami").ConfigureAwait(false);
    //  //var proxied = DataProxy.SerializeGeneric<WhoAmI>(res);
    //  //return proxied.FirstOrDefault();
    //}
  }
}
