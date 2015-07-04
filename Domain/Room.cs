using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

namespace Trizbort.Domain
{
  public class Room : Element
  {
    public string Name { get; set; } 
    public override Vector2 Position { get; set; }

    public override void Draw(WindowRenderTarget renderTarget2D)
    {
      renderTarget2D.DrawRectangle(new RectangleF(Position.X, Position.Y, 10f, 10f),new SolidColorBrush(renderTarget2D,Color.Blue),1f );
    }

  }
}