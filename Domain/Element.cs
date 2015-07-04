using SharpDX;
using SharpDX.Direct2D1;

namespace Trizbort.Domain
{
  public abstract class Element
  {
    public int ID { get; set; }

    // overrides
    public virtual Vector2 Position { get; set; }
    public abstract void Draw(WindowRenderTarget renderTarget2D);
  }
}