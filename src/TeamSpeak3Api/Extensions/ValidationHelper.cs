using System.Net;

namespace TeamSpeak3Api
{
  public static class ValidationHelper
  {
    public static bool ValidatePort(int port) => port >= IPEndPoint.MinPort && port <= IPEndPoint.MaxPort;
  }
}
