using System.Globalization;
using System.Linq;

namespace TeamSpeak3Api.Intern
{
  public class QueryParameterValue : IParameterValue
  {
    public string Value { get; set; }
    public QueryParameterValue(string value = null) => this.Value = value;
    public QueryParameterValue(int value) => this.Value = value.ToString(CultureInfo.CurrentCulture);
    public QueryParameterValue(bool value) => this.Value = value ? "1" : "0";
    public string GetParameterLine(string parameterName) => string.Concat(parameterName, '=', Value ?? string.Empty);

    public static implicit operator QueryParameterValue(string from) => new QueryParameterValue(from);
    public static implicit operator QueryParameterValue(int from) => new QueryParameterValue(from);
    public static implicit operator QueryParameterValue(bool from) => new QueryParameterValue(from);
  }
}
