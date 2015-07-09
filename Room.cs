/*
    Copyright (c) 2010-2015 by Genstein and Jason Lautzenheiser.

    This file is (or was originally) part of Trizbort, the Interactive Fiction Mapper.

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using PdfSharp.Drawing;

namespace Trizbort
{
  /// <summary>
  ///   A room in the project.
  /// </summary>
  public class Room : Element, ISizeable
  {
    private const CompassPoint DEFAULT_OBJECTS_POSITION = CompassPoint.South;
    private readonly List<string> mDescriptions = new List<string>();
    private readonly TextBlock mName = new TextBlock();
    private readonly TextBlock mSubTitle = new TextBlock();
    private readonly TextBlock mObjects = new TextBlock();
    private bool mIsDark;
    private CompassPoint mObjectsPosition = DEFAULT_OBJECTS_POSITION;
    // Added for linking connections when pasting
    private Vector mPosition;
    // Added for Room specific colors (White shows global color)
    private Color mRoomborder = Color.Transparent;
    private Color mRoomfill = Color.Transparent;
    private Color mRoomlargetext = Color.Transparent;
    private string mRoomRegion;
    private Color mRoomsmalltext = Color.Transparent;
    private Color mSecondfill = Color.Transparent;
    private string mSecondfilllocation = "Bottom";
    private BorderDashStyle mBorderStyle = BorderDashStyle.Solid;
    private Vector mSize;
    private const int MAX_OBJECTS=1000;

    public Room(Project project) : base(project)
    {
      Name = "Cave";
      Region = Trizbort.Region.DefaultRegion;
      Size = new Vector(3*Settings.GridSize, 2*Settings.GridSize);
      Position = new Vector(-Size.X/2, -Size.Y/2);

      // connections may connect to any of our "corners"
      PortList.Add(new CompassPort(CompassPoint.North, this));
      PortList.Add(new CompassPort(CompassPoint.NorthNorthEast, this));
      PortList.Add(new CompassPort(CompassPoint.NorthEast, this));
      PortList.Add(new CompassPort(CompassPoint.EastNorthEast, this));
      PortList.Add(new CompassPort(CompassPoint.East, this));
      PortList.Add(new CompassPort(CompassPoint.EastSouthEast, this));
      PortList.Add(new CompassPort(CompassPoint.SouthEast, this));
      PortList.Add(new CompassPort(CompassPoint.SouthSouthEast, this));
      PortList.Add(new CompassPort(CompassPoint.South, this));
      PortList.Add(new CompassPort(CompassPoint.SouthSouthWest, this));
      PortList.Add(new CompassPort(CompassPoint.SouthWest, this));
      PortList.Add(new CompassPort(CompassPoint.WestSouthWest, this));
      PortList.Add(new CompassPort(CompassPoint.West, this));
      PortList.Add(new CompassPort(CompassPoint.WestNorthWest, this));
      PortList.Add(new CompassPort(CompassPoint.NorthWest, this));
      PortList.Add(new CompassPort(CompassPoint.NorthNorthWest, this));
    }

    // Added this second constructor to be used when loading a room
    // This constructor is significantly faster as it doesn't look for gap in the element IDs
    public Room(Project project, int totalIDs) : base(project, totalIDs)
    {
      Name = "Cave";
      Region = Trizbort.Region.DefaultRegion;
      Size = new Vector(3*Settings.GridSize, 2*Settings.GridSize);
      Position = new Vector(-Size.X/2, -Size.Y/2);

      // connections may connect to any of our "corners"
      PortList.Add(new CompassPort(CompassPoint.North, this));
      PortList.Add(new CompassPort(CompassPoint.NorthNorthEast, this));
      PortList.Add(new CompassPort(CompassPoint.NorthEast, this));
      PortList.Add(new CompassPort(CompassPoint.EastNorthEast, this));
      PortList.Add(new CompassPort(CompassPoint.East, this));
      PortList.Add(new CompassPort(CompassPoint.EastSouthEast, this));
      PortList.Add(new CompassPort(CompassPoint.SouthEast, this));
      PortList.Add(new CompassPort(CompassPoint.SouthSouthEast, this));
      PortList.Add(new CompassPort(CompassPoint.South, this));
      PortList.Add(new CompassPort(CompassPoint.SouthSouthWest, this));
      PortList.Add(new CompassPort(CompassPoint.SouthWest, this));
      PortList.Add(new CompassPort(CompassPoint.WestSouthWest, this));
      PortList.Add(new CompassPort(CompassPoint.West, this));
      PortList.Add(new CompassPort(CompassPoint.WestNorthWest, this));
      PortList.Add(new CompassPort(CompassPoint.NorthWest, this));
      PortList.Add(new CompassPort(CompassPoint.NorthNorthWest, this));
    }

    /// <summary>
    ///   Get/set the name of the room.
    /// </summary>
    public string Name
    {
      get { return mName.Text; }
      set
      {
        value = value ?? string.Empty;
        if (mName.Text != value)
        {
          mName.Text = value;
          RaiseChanged();
        }
      }
    }

    /// <summary>
    ///   Get/set the subtitle of the room.
    /// </summary>
    public string SubTitle
    {
      get { return mSubTitle.Text; }
      set
      {
        value = value ?? string.Empty;
        if (mSubTitle.Text != value)
        {
          mSubTitle.Text = value;
          RaiseChanged();
        }
      }
    }

    /// <summary>
    ///   Get/set whether the room is dark or lit.
    /// </summary>
    public bool IsDark
    {
      get { return mIsDark; }
      set
      {
        if (mIsDark != value)
        {
          mIsDark = value;
          RaiseChanged();
        }
      }
    }

    /// <summary>
    ///   Get/set the list of objects in the room.
    /// </summary>
    public string Objects
    {
      get { return mObjects.Text; }
      set
      {
        value = value ?? string.Empty;
        if (mObjects.Text != value)
        {
          mObjects.Text = value;
          RaiseChanged();
        }
      }
    }

    /// <summary>
    ///   Get/set the position, relative to the room,
    ///   at which the object list is drawn on the map.
    /// </summary>
    public CompassPoint ObjectsPosition
    {
      get { return mObjectsPosition; }
      set
      {
        if (mObjectsPosition != value)
        {
          mObjectsPosition = value;
          RaiseChanged();
        }
      }
    }

    public BorderDashStyle BorderStyle
    {
      get { return mBorderStyle; }
      set
      {
        if (mBorderStyle != value)
        {
          mBorderStyle = value;
          RaiseChanged();
        }
      }
    }

    public string Region
    {
      get { return mRoomRegion; }
      set
      {
        if (mRoomRegion != value)
        {
          mRoomRegion = value;
          RaiseChanged();
        }
      }
    }

    public override Depth Depth => Depth.Medium;
    public override bool HasDialog => true;

    public bool IsConnected
    {
      get
      {
        // TODO: This is needlessly expensive
        foreach (var element in Map.Elements)
        {
          if (!(element is Connection))
          {
            continue;
          }

          var connection = (Connection) element;
          foreach (var vertex in connection.VertexList)
          {
            var port = vertex.Port;
            if (port != null && port.Owner == this)
            {
              return true;
            }
          }
        }

        return false;
      }
    }

    public bool ArbitraryAutomappedPosition { get; set; }

    public string PrimaryDescription
    {
      get
      {
        if (mDescriptions.Count > 0)
        {
          return mDescriptions[0];
        }
        return null;
      }
    }

    public bool HasDescription => mDescriptions.Count > 0;

    // Added for Room specific colors
    public Color RoomFill
    {
      get { return mRoomfill; }
      set
      {
        if (mRoomfill != value)
        {
          mRoomfill = value;
          RaiseChanged();
        }
      }
    }

    // Added for Room specific colors
    public Color SecondFill
    {
      get { return mSecondfill; }
      set
      {
        if (mSecondfill != value)
        {
          mSecondfill = value;
          RaiseChanged();
        }
      }
    }

    // Added for Room specific colors
    public String SecondFillLocation
    {
      get { return mSecondfilllocation; }
      set
      {
        if (mSecondfilllocation != value)
        {
          mSecondfilllocation = value;
          RaiseChanged();
        }
      }
    }

    // Added for Room specific colors
    public Color RoomBorder
    {
      get { return mRoomborder; }
      set
      {
        if (mRoomborder != value)
        {
          mRoomborder = value;
          RaiseChanged();
        }
      }
    }

    // Added for Room specific colors
    public Color RoomLargeText
    {
      get { return mRoomlargetext; }
      set
      {
        if (mRoomlargetext != value)
        {
          mRoomlargetext = value;
          RaiseChanged();
        }
      }
    }

    // Added for Room specific colors
    public Color RoomSmallText
    {
      get { return mRoomsmalltext; }
      set
      {
        if (mRoomsmalltext != value)
        {
          mRoomsmalltext = value;
          RaiseChanged();
        }
      }
    }

    // Added for linking connections when pasting
    public int OldID { get; set; }

    public override sealed Vector Position
    {
      get { return mPosition; }
      set
      {
        if (mPosition != value)
        {
          mPosition = value;
          ArbitraryAutomappedPosition = false;
          RaiseChanged();
        }
      }
    }

    public float X => mPosition.X;
    public float Y => mPosition.Y;

    public Vector Size
    {
      get { return mSize; }
      set
      {
        if (mSize != value)
        {
          mSize = value;
          RaiseChanged();
        }
      }
    }

    public float Width => mSize.X;

    public float Height => mSize.Y;

    public Rect InnerBounds => new Rect(Position, Size);

    public override string ToString()
    {
      return $"Room: {Name}";
    }

    public override string GetToolTipFooter()
    {
      return Objects;
    }

    private bool isDefaultRegion()
    {
      return Region == Trizbort.Region.DefaultRegion || string.IsNullOrEmpty(Region);
    }

    public override string GetToolTipHeader()
    {
      return $"{Name}{(!isDefaultRegion() ? $" ({Region})" : string.Empty)}";
    }

    public override bool HasTooltip()
    {
      return true;
    }

    public override eTooltipColor GetToolTipColor()
    {
      return eTooltipColor.BlueMist;
    }


    public override float Distance(Vector pos, bool includeMargins)
    {
      var bounds = UnionBoundsWith(Rect.Empty, includeMargins);
      return pos.DistanceFromRect(bounds);
    }

    public override bool Intersects(Rect rect)
    {
      return InnerBounds.IntersectsWith(rect);
    }

    public override Vector GetPortPosition(Port port)
    {
      // map the compass points onto our bounding rectangle
      var compass = (CompassPort) port;
      return InnerBounds.GetCorner(compass.CompassPoint);
    }

    public override Vector GetPortStalkPosition(Port port)
    {
      var outerBounds = InnerBounds;
      outerBounds.Inflate(Settings.ConnectionStalkLength);
      var compass = (CompassPort) port;
      var inner = InnerBounds.GetCorner(compass.CompassPoint);
      var outer = outerBounds.GetCorner(compass.CompassPoint);
      switch (compass.CompassPoint)
      {
        case CompassPoint.EastNorthEast:
        case CompassPoint.EastSouthEast:
        case CompassPoint.WestNorthWest:
        case CompassPoint.WestSouthWest:
          return new Vector(outer.X, inner.Y);
        case CompassPoint.NorthNorthEast:
        case CompassPoint.NorthNorthWest:
        case CompassPoint.SouthSouthEast:
        case CompassPoint.SouthSouthWest:
          return new Vector(inner.X, outer.Y);
        default:
          return outer;
      }
    }

    public override string GetToolTipText()
    {
      var sText = $"{PrimaryDescription}";

      return sText;
    }

    public Port PortAt(CompassPoint compassPoint)
    {
      return PortList.Cast<CompassPort>().FirstOrDefault(port => port.CompassPoint == compassPoint);
    }

    public override void PreDraw(DrawingContext context)
    {
      var topLeft = InnerBounds.GetCorner(CompassPoint.NorthWest);
      var topRight = InnerBounds.GetCorner(CompassPoint.NorthEast);
      var bottomLeft = InnerBounds.GetCorner(CompassPoint.SouthWest);
      var bottomRight = InnerBounds.GetCorner(CompassPoint.SouthEast);

      var top = new LineSegment(topLeft, topRight);
      var right = new LineSegment(topRight, bottomRight);
      var bottom = new LineSegment(bottomRight, bottomLeft);
      var left = new LineSegment(bottomLeft, topLeft);

      context.LinesDrawn.Add(top);
      context.LinesDrawn.Add(right);
      context.LinesDrawn.Add(bottom);
      context.LinesDrawn.Add(left);
    }

    public override void Draw(XGraphics graphics, Palette palette, DrawingContext context)
    {
      var random = new Random(Name.GetHashCode());

        var topLeft = InnerBounds.GetCorner(CompassPoint.NorthWest);
        var topRight = InnerBounds.GetCorner(CompassPoint.NorthEast);
        var bottomLeft = InnerBounds.GetCorner(CompassPoint.SouthWest);
        var bottomRight = InnerBounds.GetCorner(CompassPoint.SouthEast);

        var topCenter = InnerBounds.GetCorner(CompassPoint.North);
        var rightCenter = InnerBounds.GetCorner(CompassPoint.East);
        var bottomCenter = InnerBounds.GetCorner(CompassPoint.South);
        var leftCenter = InnerBounds.GetCorner(CompassPoint.West);

        var top = new LineSegment(topLeft, topRight);
        var right = new LineSegment(topRight, bottomRight);
        var bottom = new LineSegment(bottomRight, bottomLeft);
        var left = new LineSegment(bottomLeft, topLeft);

        var halfTopRight = new LineSegment(topCenter, topRight);
        var halfTopLeft = new LineSegment(topCenter, topLeft);
        var halfBottomRight = new LineSegment(bottomRight, bottomCenter);
        var halfBottomLeft = new LineSegment(bottomLeft, bottomCenter);
        var centerVertical = new LineSegment(bottomCenter, topCenter);

        var centerHorizontal = new LineSegment(leftCenter, rightCenter);
        var halfRightBottom = new LineSegment(rightCenter, bottomRight);
        var halfLeftBottom = new LineSegment(bottomLeft, leftCenter);
        var halfRightTop = new LineSegment(rightCenter, topRight);
        var halfLeftTop = new LineSegment(topLeft, leftCenter);

        var slantUp = new LineSegment(bottomLeft, topRight);
        var slantDown = new LineSegment(bottomRight, topLeft);

        context.LinesDrawn.Add(top);
        context.LinesDrawn.Add(right);
        context.LinesDrawn.Add(bottom);
        context.LinesDrawn.Add(left);

        if (context.Selected)
        {
          var tBounds = InnerBounds;
          tBounds.Inflate(5);

          var topLeftSelect = tBounds.GetCorner(CompassPoint.NorthWest);
          var topRightSelect = tBounds.GetCorner(CompassPoint.NorthEast);
          var bottomLeftSelect = tBounds.GetCorner(CompassPoint.SouthWest);
          var bottomRightSelect = tBounds.GetCorner(CompassPoint.SouthEast);

          var topSelect = new LineSegment(topLeftSelect, topRightSelect);
          var rightSelect = new LineSegment(topRightSelect, bottomRightSelect);
          var bottomSelect = new LineSegment(bottomRightSelect, bottomLeftSelect);
          var leftSelect = new LineSegment(bottomLeftSelect, topLeftSelect);

          var pathSelected = palette.Path();
          Drawing.AddLine(pathSelected, topSelect, random);
          Drawing.AddLine(pathSelected, rightSelect, random);
          Drawing.AddLine(pathSelected, bottomSelect, random);
          Drawing.AddLine(pathSelected, leftSelect, random);
          var brushSelected = new SolidBrush(Color.Gold);
          graphics.DrawPath(brushSelected, pathSelected);
        }

      // get region color
      var regionColor = Settings.Regions.FirstOrDefault(p => p.RegionName.Equals(Region, StringComparison.OrdinalIgnoreCase)) ?? Settings.Regions.FirstOrDefault(p => p.RegionName.Equals(Trizbort.Region.DefaultRegion, StringComparison.OrdinalIgnoreCase));
      Brush brush = new SolidBrush(regionColor.RColor);

      // Room specific fill brush (White shows global color)
      if (RoomFill != Color.Transparent ) { brush = new SolidBrush(RoomFill); }

      if (!Settings.DebugDisableLineRendering && BorderStyle != BorderDashStyle.None )
      {
        var path = palette.Path();
        Drawing.AddLine(path, top, random);
        Drawing.AddLine(path, right, random);
        Drawing.AddLine(path, bottom, random);
        Drawing.AddLine(path, left, random);

        graphics.DrawPath(brush, path);

        // Second fill for room specific colors with a split option
        if (SecondFill != Color.Transparent)
        {
          // Set the second fill color
          brush = new SolidBrush(SecondFill);

          // Define the second path based on the second fill location
          var secondPath = palette.Path();
          switch (SecondFillLocation)
          {
            case "Bottom":
              Drawing.AddLine(secondPath, centerHorizontal, random);
              Drawing.AddLine(secondPath, halfRightBottom, random);
              Drawing.AddLine(secondPath, bottom, random);
              Drawing.AddLine(secondPath, halfLeftBottom, random);
              break;
            case "BottomRight":
              Drawing.AddLine(secondPath, slantUp, random);
              Drawing.AddLine(secondPath, right, random);
              Drawing.AddLine(secondPath, bottom, random);
              break;
            case "BottomLeft":
              Drawing.AddLine(secondPath, slantDown, random);
              Drawing.AddLine(secondPath, bottom, random);
              Drawing.AddLine(secondPath, left, random);
              break;
            case "Left":
              Drawing.AddLine(secondPath, halfTopLeft, random);
              Drawing.AddLine(secondPath, left, random);
              Drawing.AddLine(secondPath, halfBottomLeft, random);
              Drawing.AddLine(secondPath, centerVertical, random);
              break;
            case "Right":
              Drawing.AddLine(secondPath, halfTopRight, random);
              Drawing.AddLine(secondPath, right, random);
              Drawing.AddLine(secondPath, halfBottomRight, random);
              Drawing.AddLine(secondPath, centerVertical, random);
              break;
            case "TopRight":
              Drawing.AddLine(secondPath, top, random);
              Drawing.AddLine(secondPath, right, random);
              Drawing.AddLine(secondPath, slantDown, random);
              break;
            case "TopLeft":
              Drawing.AddLine(secondPath, top, random);
              Drawing.AddLine(secondPath, slantUp, random);
              Drawing.AddLine(secondPath, left, random);
              break;
            case "Top":
              Drawing.AddLine(secondPath, centerHorizontal, random);
              Drawing.AddLine(secondPath, halfRightTop, random);
              Drawing.AddLine(secondPath, top, random);
              Drawing.AddLine(secondPath, halfLeftTop, random);
              break;
          }
          // Draw the second fill over the first
          graphics.DrawPath(brush, secondPath);
        }
        if (IsDark)
        {
          var state = graphics.Save();
          graphics.IntersectClip(path);
          var solidbrush = (SolidBrush) palette.BorderBrush;

          graphics.DrawPolygon(solidbrush, new[] { topRight.ToPointF(), new PointF(topRight.X - Settings.DarknessStripeSize, topRight.Y), new PointF(topRight.X, topRight.Y + Settings.DarknessStripeSize) }, XFillMode.Alternate);
          graphics.Restore(state);
        }

        if (RoomBorder == Color.Transparent)
        {
          var pen = palette.BorderPen;
          pen.DashStyle = BorderStyle.ConvertToDashStyle();
          graphics.DrawPath(pen, path);
        }
        else
        {
          var roomBorderPen = new Pen(RoomBorder, Settings.LineWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round, DashStyle = BorderStyle.ConvertToDashStyle() };
          graphics.DrawPath(roomBorderPen, path);
        }
      }

      var font = Settings.LargeFont;
      var roombrush = new SolidBrush(regionColor.TextColor);
      // Room specific fill brush (White shows global color)
      if (RoomLargeText != Color.Transparent) { roombrush = new SolidBrush(RoomLargeText); }

      var textBounds = InnerBounds;
      textBounds.Inflate(-5, -5);

      if (textBounds.Width > 0 && textBounds.Height > 0)
      {
        if (!Settings.DebugDisableTextRendering)
        {
          var actualTextRect = mName.Draw(graphics, font, roombrush, textBounds.Position, textBounds.Size, XStringFormats.Center);
          var subTitleBounds = new Rect(actualTextRect.X, (actualTextRect.Y + actualTextRect.Height-4 ), actualTextRect.Width, actualTextRect.Height);
          mSubTitle.Draw(graphics, Settings.LineFont, roombrush, subTitleBounds.Position, subTitleBounds.Size, XStringFormats.Center);
        }
      }

      var expandedBounds = InnerBounds;
      expandedBounds.Inflate(Settings.ObjectListOffsetFromRoom, Settings.ObjectListOffsetFromRoom);
      var drawnObjectList = false;

      font = Settings.SmallFont;
      brush = palette.SmallTextBrush;
      // Room specific fill brush (White shows global color)
      var bUseObjectRoomBrush = false;
      if (RoomSmallText != Color.Transparent)
      {
        bUseObjectRoomBrush = true;
        brush = new SolidBrush(RoomSmallText);
      }

      if (!string.IsNullOrEmpty(Objects))
      {
        var format = new XStringFormat();
        var pos = expandedBounds.GetCorner(mObjectsPosition);
        if (!Drawing.SetAlignmentFromCardinalOrOrdinalDirection(format, mObjectsPosition))
        {
          // object list appears inside the room below its name
          format.LineAlignment = XLineAlignment.Far;
          format.Alignment = XStringAlignment.Near;
          var height = InnerBounds.Height/2f - font.Height/2f;
          var bounds = new Rect(InnerBounds.Left + Settings.ObjectListOffsetFromRoom, InnerBounds.Bottom - height, InnerBounds.Width - Settings.ObjectListOffsetFromRoom, height - Settings.ObjectListOffsetFromRoom);
          brush = (bUseObjectRoomBrush ? new SolidBrush(RoomSmallText) : roombrush);
          if (bounds.Width > 0 && bounds.Height > 0)
          {
            mObjects.Draw(graphics, font, brush, bounds.Position, bounds.Size, format);
          }
          drawnObjectList = true;
        }
        else if (mObjectsPosition == CompassPoint.North || mObjectsPosition == CompassPoint.South)
        {
          pos.X += Settings.ObjectListOffsetFromRoom;
        }

        if (!drawnObjectList)
        {
          if (!Settings.DebugDisableTextRendering)
          {
            var aObjects = mObjects.Text.Split('\n');
            var tString = aObjects.Take(MAX_OBJECTS).Aggregate(string.Empty, (current, aObject) => current + (aObject + "\n"));
            var displayObjects = new TextBlock() {Text = tString};

            displayObjects.Draw(graphics, font, brush, pos, Vector.Zero, format);

          }
        }
      }
    }

    public override Rect UnionBoundsWith(Rect rect, bool includeMargins)
    {
      var bounds = InnerBounds;
      if (includeMargins)
      {
        bounds.Inflate(Settings.LineWidth + Settings.ConnectionStalkLength);
      }
      return rect.Union(bounds);
    }

    public void ShowDialog(PropertiesStartType startPoint)
    {
      showRoomDialog(startPoint);
    }


    public override void ShowDialog()
    {
      showRoomDialog();
    }

    private void showRoomDialog(PropertiesStartType start = PropertiesStartType.RoomName)
    {
      using (var dialog = new RoomPropertiesDialog(start))
      {
        dialog.RoomName = Name;
        dialog.Description = PrimaryDescription;
        dialog.RoomSubTitle = SubTitle;
        dialog.IsDark = IsDark;
        dialog.Objects = Objects;
        dialog.ObjectsPosition = ObjectsPosition;
        dialog.BorderStyle = BorderStyle;

        dialog.RoomFillColor = RoomFill;
        dialog.SecondFillColor = SecondFill;
        dialog.SecondFillLocation = SecondFillLocation;
        dialog.RoomBorderColor = RoomBorder;
        dialog.RoomTextColor = RoomLargeText;
        dialog.ObjectTextColor = RoomSmallText;
        dialog.RoomRegion = Region;


        if (dialog.ShowDialog() == DialogResult.OK)
        {
          Name = dialog.RoomName;
          SubTitle = dialog.RoomSubTitle;
          if (PrimaryDescription != dialog.Description)
          {
            ClearDescriptions();
            AddDescription(dialog.Description);
          }
          IsDark = dialog.IsDark;
          Objects = dialog.Objects;
          BorderStyle = dialog.BorderStyle;
          ObjectsPosition = dialog.ObjectsPosition;
          // Added for Room specific colors
          RoomFill = dialog.RoomFillColor;
          // Added for Room specific colors
          SecondFill = dialog.SecondFillColor;
          // Added for Room specific colors
          SecondFillLocation = dialog.SecondFillLocation;
          // Added for Room specific colors
          RoomBorder = dialog.RoomBorderColor;
          // Added for Room specific colors
          RoomLargeText = dialog.RoomTextColor;
          // Added for Room specific colors
          RoomSmallText = dialog.ObjectTextColor;
          Region = dialog.RoomRegion;
        }
      }
    }

    public void Save(XmlScribe scribe)
    {
      scribe.Attribute("name", Name);
      scribe.Attribute("subtitle", SubTitle);
      scribe.Attribute("x", Position.X);
      scribe.Attribute("y", Position.Y);
      scribe.Attribute("w", Size.X);
      scribe.Attribute("h", Size.Y);
      scribe.Attribute("region", string.IsNullOrEmpty(Region) ? Trizbort.Region.DefaultRegion : Region);
      scribe.Attribute("borderstyle",BorderStyle.ToString());
      if (IsDark)
      {
        scribe.Attribute("isDark", IsDark);
      }
      scribe.Attribute("description", PrimaryDescription);

      var colorValue = Colors.SaveColor(RoomFill);
      scribe.Attribute("roomFill", colorValue);

      colorValue = Colors.SaveColor(SecondFill);
      scribe.Attribute("secondFill", colorValue);
      scribe.Attribute("secondFillLocation", SecondFillLocation);

      colorValue = Colors.SaveColor(RoomBorder);
      scribe.Attribute("roomBorder", colorValue);

      colorValue = Colors.SaveColor(RoomLargeText);
      scribe.Attribute("roomLargeText", colorValue);

      colorValue = Colors.SaveColor(RoomSmallText);
      scribe.Attribute("roomSmallText", colorValue);

      // Up to this point was added to turn colors to Hex code for xmpl saving/loading

      if (!string.IsNullOrEmpty(Objects) || ObjectsPosition != DEFAULT_OBJECTS_POSITION)
      {
        scribe.StartElement("objects");
        if (ObjectsPosition != DEFAULT_OBJECTS_POSITION)
        {
          scribe.Attribute("at", ObjectsPosition);
        }
        if (!string.IsNullOrEmpty(Objects))
        {
          scribe.Value(Objects.Replace("\r", string.Empty).Replace("|", "\\|").Replace("\n", "|"));
        }
        scribe.EndElement();
      }
    }

 

    public void Load(XmlElementReader element)
    {
      Name = element.Attribute("name").Text;
      SubTitle = element.Attribute("subtitle").Text;
      ClearDescriptions();
      AddDescription(element.Attribute("description").Text);
      Position = new Vector(element.Attribute("x").ToFloat(), element.Attribute("y").ToFloat());
      Size = new Vector(element.Attribute("w").ToFloat(), element.Attribute("h").ToFloat());
      Region = element.Attribute("region").Text;
      IsDark = element.Attribute("isDark").ToBool();
      if (element.Attribute("borderstyle").Text != "")
        BorderStyle = (BorderDashStyle)Enum.Parse(typeof(BorderDashStyle), element.Attribute("borderstyle").Text);

      if (Project.Version.CompareTo(new Version(1, 5, 8, 3)) < 0)
      {
        if (element.Attribute("roomFill").Text != "" && element.Attribute("roomFill").Text != "#FFFFFF") { RoomFill = ColorTranslator.FromHtml(element.Attribute("roomFill").Text); }
        if (element.Attribute("secondFill").Text != "" && element.Attribute("roomFill").Text != "#FFFFFF") { SecondFill = ColorTranslator.FromHtml(element.Attribute("secondFill").Text); }
        if (element.Attribute("secondFillLocation").Text != "" && element.Attribute("roomFill").Text != "#FFFFFF") { SecondFillLocation = element.Attribute("secondFillLocation").Text; }
        if (element.Attribute("roomBorder").Text != "" && element.Attribute("roomFill").Text != "#FFFFFF") { RoomBorder = ColorTranslator.FromHtml(element.Attribute("roomBorder").Text); }
        if (element.Attribute("roomLargeText").Text != "" && element.Attribute("roomFill").Text != "#FFFFFF") { RoomLargeText = ColorTranslator.FromHtml(element.Attribute("roomLargeText").Text); }
        if (element.Attribute("roomSmallText").Text != "" && element.Attribute("roomFill").Text != "#FFFFFF") { RoomSmallText = ColorTranslator.FromHtml(element.Attribute("roomSmallText").Text); }
      }
      else
      {
        if (element.Attribute("roomFill").Text != "") { RoomFill = ColorTranslator.FromHtml(element.Attribute("roomFill").Text); }
        if (element.Attribute("secondFill").Text != "") { SecondFill = ColorTranslator.FromHtml(element.Attribute("secondFill").Text); }
        if (element.Attribute("secondFillLocation").Text != "") { SecondFillLocation = element.Attribute("secondFillLocation").Text; }
        if (element.Attribute("roomBorder").Text != "") { RoomBorder = ColorTranslator.FromHtml(element.Attribute("roomBorder").Text); }
        if (element.Attribute("roomLargeText").Text != "") { RoomLargeText = ColorTranslator.FromHtml(element.Attribute("roomLargeText").Text); }
        if (element.Attribute("roomSmallText").Text != "") { RoomSmallText = ColorTranslator.FromHtml(element.Attribute("roomSmallText").Text); }
      }
      Objects = element["objects"].Text.Replace("|", "\r\n").Replace("\\\r\n", "|");
      ObjectsPosition = element["objects"].Attribute("at").ToCompassPoint(ObjectsPosition);
    }

    public List<Connection> GetConnections()
    {
      return GetConnections(null);
    }
    
    public List<Connection> GetConnections(CompassPoint? compassPoint)
    {
      var connections = new List<Connection>();

      // TODO: This is needlessly expensive, traversing as it does the entire project's element list.
      foreach (var element in Map.Elements.OfType<Connection>())
      {
        var connection = element;
        foreach (var vertex in connection.VertexList)
        {
          var port = vertex.Port;
          if (port == null || port.Owner != this || !(port is CompassPort))
            continue;

          var compassPort = (CompassPort) vertex.Port;
          
          if (compassPoint == null)
            connections.Add(connection);
          else
            if (compassPort.CompassPoint == compassPoint)
            {
              // found a connection with a vertex joined to our room's port at the given compass point
              connections.Add(connection);
            }
        }
      }
      return connections;
    }

    public IList<string> ListOfObjects()
    {
      var tObjects = Objects.Replace("\r", string.Empty).Replace("|", "\\|").Replace("\n", "|");
      var objects = tObjects.Split('|').Where(p => p != string.Empty).ToList();
      return objects;
    }

    public void ClearDescriptions()
    {
      if (mDescriptions.Count > 0)
      {
        mDescriptions.Clear();
        RaiseChanged();
      }
    }

    public void AddDescription(string description)
    {
      if (string.IsNullOrEmpty(description))
      {
        // never add an empty description
        return;
      }

      if (mDescriptions.Any(existing => existing == description))
      {
        return;
      }

      // we don't have this (non-empty) description already; add it
      mDescriptions.Add(description);
      RaiseChanged();
    }

    public bool MatchDescription(string description)
    {
      if (string.IsNullOrEmpty(description))
      {
        // match a lack of description if we have no descriptions
        return mDescriptions.Count == 0;
      }

      return mDescriptions.Any(existing => existing == description);

      // no match
    }

    public String ClipboardPrint()
    {
      var clipboardText = "";
      clipboardText += Name + ":";
      clipboardText += Position.X + ":";
      clipboardText += Position.Y + ":";
      clipboardText += Size.X + ":";
      clipboardText += Size.Y + ":";
      clipboardText += IsDark + ":";
      clipboardText += PrimaryDescription + ":";
      clipboardText += Region + ":";
      clipboardText += BorderStyle + ":";


      var colorValue = Colors.SaveColor(RoomFill);
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(SecondFill);
      clipboardText += colorValue + ":";
      clipboardText += SecondFillLocation + ":";

      colorValue = Colors.SaveColor(RoomBorder);
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(RoomLargeText); 
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(RoomSmallText);
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(RoomBorder);
      clipboardText += colorValue;


      if (!string.IsNullOrEmpty(Objects) || ObjectsPosition != DEFAULT_OBJECTS_POSITION)
      {
        string objectsDirection;
        CompassPointHelper.ToName(ObjectsPosition, out objectsDirection);
        clipboardText += ":" + objectsDirection + ":";
        if (!string.IsNullOrEmpty(Objects))
        {
          clipboardText += (Objects.Replace("\r\n", ":"));
        }
      }

      return clipboardText;
    }


    public String ClipboardColorPrint()
    {
      var clipboardText = string.Empty;

      var colorValue = Colors.SaveColor(RoomFill);
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(SecondFill); 
      clipboardText += colorValue + ":";
      clipboardText += SecondFillLocation + ":";

      colorValue = Colors.SaveColor(RoomBorder);
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(RoomLargeText);
      clipboardText += colorValue + ":";

      colorValue = Colors.SaveColor(RoomSmallText);
      clipboardText += colorValue;

      return clipboardText;
    }

    internal class CompassPort : Port
    {
      public CompassPort(CompassPoint compassPoint, Room room)
        : base(room)
      {
        CompassPoint = compassPoint;
        Room = room;
      }

      public CompassPoint CompassPoint { get; }

      public override string ID
      {
        get
        {
          string name;
          if (CompassPointHelper.ToName(CompassPoint, out name))
          {
            return name;
          }
          return string.Empty;
        }
      }

      public Room Room { get; private set; }
    }
  }
}

public enum PropertiesStartType
{
  RoomName, 
  Region
}