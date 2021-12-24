using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeamSpeak3Api.Intern
{
  /// <summary>
  /// Basic class for accessing TeamSpeak Query API on a Server.
  /// </summary>
  public class QueryClient : IDisposable
  {
    #region const
    public const string DefaultHost = "127.0.0.1";
    public const int DefaultPort = 10011;
    #endregion

    #region Settings
    public string Host { get; }
    public int Port { get; }
    public bool IsConnected { get; private set; }
    #endregion

    #region Attributes
    public TcpClient Client { get; }
    private NetworkStream _Stream { get; set; }
    private StreamReader _Reader { get; set; }
    private StreamWriter _Writer { get; set; }
    private CancellationTokenSource _Cts { get; set; }
    internal Stopwatch Idle { get; } = new Stopwatch();

    private QueryCommand _QueryCommand { get; set; }
    #endregion

    #region ctor
    public QueryClient() : this(DefaultHost, DefaultPort) { }

    public QueryClient(string host) : this(host, DefaultPort) { }

    public QueryClient(string host, int port)
    {
      if (string.IsNullOrWhiteSpace(host)) throw new ArgumentNullException(nameof(host));
      if (!ValidationHelper.ValidatePort(port)) throw new ArgumentOutOfRangeException(nameof(port));
      this.Host = host;
      this.Port = port;
      this.Client = new TcpClient();
    }

    ~QueryClient()
    {
      Dispose(false);
    }
    #endregion

    #region Tasks
    public async Task<CancellationTokenSource> Connect()
    {
      await Client.ConnectAsync(Host, Port).ConfigureAwait(false);
      if (!Client.Connected) throw new InvalidOperationException("Could not connect.");
      _Stream = Client.GetStream();
      _Reader = new StreamReader(_Stream);
      _Writer = new StreamWriter(_Stream) { NewLine = "\n" };

      IsConnected = true;

      await _Reader.ReadLineAsync().ConfigureAwait(false);
      await _Reader.ReadLineAsync().ConfigureAwait(false); // Ignore welcome message
      await _Reader.ReadLineAsync().ConfigureAwait(false);

      Idle.Restart(); //Should restart since you're freshly connected.

      return ResponseProcessingLoop();
    }
    public void Disconnect()
    {
      Idle.Stop();
      if (_Cts == null) return;
      _Cts.Cancel();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        _Cts?.Cancel();
        _Cts?.Dispose();
        Client?.Dispose();
        _Stream?.Dispose();
        _Reader?.Dispose();
        _Writer?.Dispose();
      }
    }

    private CancellationTokenSource ResponseProcessingLoop()
    {
      _Cts = new CancellationTokenSource();
      Task.Run(async () =>
      {
        while (!_Cts.Token.IsCancellationRequested)
        {
          string line = null;
          try
          {
            line = await _Reader.ReadLineAsync().WithCancellation(_Cts.Token).ConfigureAwait(false);
          }
          catch (OperationCanceledException)
          {
            break;
          }

          Debug.WriteLine(line);

          if (line == null)
          {
            _Cts.Cancel();
            continue;
          }

          if (string.IsNullOrWhiteSpace(line))
            continue;

          var s = line.Trim();
          if (s.StartsWith("error", StringComparison.OrdinalIgnoreCase))
          {
            //Debug.Assert(_currentCommand != null);

            //var error = ParseError(s);
            //_currentCommand.Error = error;
            InvokeResponse(_QueryCommand);
          }
          else if (s.StartsWith("notify", StringComparison.OrdinalIgnoreCase))
          {
            s = s.Remove(0, "notify".Length);
            //var not = ParseNotification(s);
            //InvokeNotification(not);
          }
          else
          {
            //Debug.Assert(_currentCommand != null);
            _QueryCommand.RawResponse = s;
            //_currentCommand.ResponseDictionary = ParseResponse(s);
          }
        }

        IsConnected = false;
      });
      return _Cts;
    }
    #endregion

    #region Send
    public async Task<QueryResponseDictionary[]> Send(string cmd) => await Send(cmd, null);
    public async Task<QueryResponseDictionary[]> Send(string cmd, params QueryParameter[] parameters) => await Send(cmd, parameters, null);
    public async Task<QueryResponseDictionary[]> Send(string cmd, QueryParameter[] parameters, string[] options)
    {
      if (string.IsNullOrWhiteSpace(cmd)) throw new ArgumentNullException(nameof(cmd));

      QueryCommand qcmd = _QueryCommand = new QueryCommand(cmd, options, parameters);
      await ExecuteSend(qcmd).ConfigureAwait(false);
      Idle.Restart();

      return await qcmd.Execute.Task.ConfigureAwait(false);
    }

    private async Task ExecuteSend(QueryCommand cmd)
    {
      await _Writer.WriteLineAsync(cmd.RawCommandText).ConfigureAwait(false);
      await _Writer.FlushAsync().ConfigureAwait(false);
    }
    #endregion

    private static void InvokeResponse(QueryCommand forCommand)
    {
      //if (forCommand.Error.Id != 0)
      //{
      //  forCommand.Defer.TrySetException(new QueryException(forCommand.Error));
      //}
      //else
      //{
      //  forCommand.Defer.TrySetResult(forCommand.ResponseDictionary);
      //}
      forCommand.Execute.TrySetResult(forCommand.Result);
    }
  }
}
