using System.Collections.Generic;

namespace Trizbort
{
  public class Map
  {
    public BoundList<Element> Elements { get; } = new BoundList<Element>();
    public string Name { get; set; }
    public Canvas Canvas { get; set; }

    public Map()
    {
      Elements.Removed += onElementRemoved;
    }

    /// <summary>
    ///   Handle element removal by removing elements which refer to it. May recurse.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void onElementRemoved(object sender, ItemEventArgs<Element> e)
    {
      var doomed = new List<Element>();
      foreach (var element in Elements)
      {
        var element1 = element as Connection;
        if (element1 != null  )
        {
          var connection = element1;
          foreach (var vertex in connection.VertexList)
          {
            if (vertex.Port != null && vertex.Port.Owner == e.Item)
            {
              doomed.Add(element1);
            }
          }
        }
      }

      foreach (var element in doomed)
      {
        Elements.Remove(element);
      }
    }

  }
}