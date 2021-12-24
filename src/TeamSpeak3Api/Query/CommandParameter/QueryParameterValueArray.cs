using System.Linq;

namespace TeamSpeak3Api.Intern
{
  public class QueryParameterValueArray : IParameterValue
  {
    private QueryParameterValue[] ParameterValues { get; set; }
    public QueryParameterValueArray() : this(null) { }
    public QueryParameterValueArray(QueryParameterValue[] parameterValues)
    {
      this.ParameterValues = parameterValues;
    }

    public string GetParameterLine(string parameterName)
    {
      if (ParameterValues == null) return string.Empty;
      string[] result = ParameterValues.Select(x => x.GetParameterLine(parameterName)).ToArray();
      return string.Join("|", result);
    }

    public static implicit operator QueryParameterValueArray(QueryParameterValue[] from) => new QueryParameterValueArray(from);
  }
}
