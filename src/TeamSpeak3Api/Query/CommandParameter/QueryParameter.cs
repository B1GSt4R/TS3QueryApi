namespace TeamSpeak3Api.Intern
{
  public class QueryParameter
  {
    public string Name { get; set; }
    public IParameterValue Value { get; set; }
    public QueryParameter(string name = null, QueryParameterValue value = null)
    {
      this.Name = name;
      this.Value = value;
    }

    public string GetParameterLine()
    {
      return Value.GetParameterLine(Name);
    }
  }
}
