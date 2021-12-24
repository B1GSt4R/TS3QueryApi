using System.Collections.Generic;

namespace TeamSpeak3Api.Intern
{
  public static class ListExtensions
  {
    public static void MoveToBottom<T>(this IList<T> basis, T obj)
    {
      basis.Remove(obj);
      basis.Add(obj);
    }
  }
}
