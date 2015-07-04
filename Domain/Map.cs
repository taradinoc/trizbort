using System.Collections;
using System.Collections.Generic;
using SharpDX.Direct2D1;

namespace Trizbort.Domain
{
  public class Map
  {
    public IList<Room> Rooms { get; set; } = new List<Room>();

    public void Draw(WindowRenderTarget renderTarget2D)
    {
      foreach (var room in Rooms)
      {
        room.Draw(renderTarget2D);
      }
    }
  }
}