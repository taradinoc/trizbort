using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Windows;
using Trizbort.Domain;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;

namespace Trizbort.UserControls
{
  public sealed partial class Canvas : RenderControl
  {
    public DirectInput di = new DirectInput();
    public Mouse mouse;

    public Factory Factory2D { get; private set; }
    public SharpDX.DirectWrite.Factory FactoryDWrite { get; private set; }
    public WindowRenderTarget RenderTarget2D { get; private set; }

    private readonly Timer tickTimer;

    private void gameLoop(object state)
    {
      var mouseState = mouse.GetCurrentState();

      if (mouseState.Buttons[0])
        MessageBox.Show("Left Button");

      if (mouseState.Buttons[2])
        MessageBox.Show("Right Button");
    }

    public Vector2 Origin { get; set; } = new Vector2(0,0);
    public Map map = new Map();

    public Canvas()
    {
      InitializeComponent();

      //testing init map
      map.Rooms = new List<Room>
      {
        new Room {Name = "Room1", Position = new Vector2(20f, 20f)},
        new Room {Name = "Room2", Position = new Vector2(100f, 100f)}
      };

      this.Paint += renderControlPaint;
      initInput();
      initGraphics();
      tickTimer = new Timer();
      tickTimer.Interval = 1000;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
      MessageBox.Show($"Mouse Click:{e}");
    }

    private void initInput()
    {
//      mouse = new Mouse(di);
//      mouse.Acquire();
    }

    private void renderControlPaint(object sender, PaintEventArgs e)
    {
      try
      {
        RenderTarget2D.BeginDraw();
        RenderTarget2D.Clear(Color.White);
        map.Draw(RenderTarget2D);
        RenderTarget2D.EndDraw();
      }
      catch (Exception excp)
      {
        MessageBox.Show(excp.Message);
      }
    }

    private void initGraphics()
    {
      Factory2D = new Factory();
      FactoryDWrite = new SharpDX.DirectWrite.Factory();

      var properties = new HwndRenderTargetProperties
      {
        Hwnd = Handle,
        PixelSize = new Size2(this.ClientSize.Width, this.ClientSize.Height),
        PresentOptions = PresentOptions.None
      };

      RenderTarget2D = new WindowRenderTarget(Factory2D, new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)), properties)
      {
        AntialiasMode = AntialiasMode.PerPrimitive,
        TextAntialiasMode = TextAntialiasMode.Default
      };

    }



  }
}
