using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamSpeak3Api.Intern
{
  public class QueryCommand
  {
    #region Attributes
    public string CommandText { get; set; }
    public string[] Options { get; set; }
    public IReadOnlyCollection<QueryParameter> Parameters { get; set; }
    public string RawCommandText { get => GetRawText(); }
    public string RawResponse { get; set; }
    public TaskCompletionSource<QueryResponseDictionary[]> Execute { get; }
    public QueryResponseDictionary[] Result { get; private set; }
    #endregion

    #region ctor
    public QueryCommand(string cmd, string[] options, IReadOnlyCollection<QueryParameter> parameters)
    {
      this.CommandText = cmd;
      this.Options = options ?? new string[0];
      this.Parameters = parameters;
      this.Execute = new TaskCompletionSource<QueryResponseDictionary[]>();
    }
    #endregion

    #region functions
    public string GetRawText()
    {
      StringBuilder result = new StringBuilder(this.CommandText);

      foreach (string opt in this.Options)
      {
        result.Append(" -").Append(opt.ToLowerInvariant());
      }

      // Parameter arrays should be the last parameters in the list
      var lastParamArray = this.Parameters.SingleOrDefault(p => p.Value is QueryParameterValueArray);
      if (lastParamArray != null) this.Parameters.ToList().MoveToBottom(lastParamArray);

      foreach (QueryParameter para in this.Parameters)
      {
        result.Append(' ').Append(para.GetParameterLine());
      }

      return result.ToString();
    }
    #endregion
  }
}
