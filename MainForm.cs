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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using Trizbort.Export;
using Trizbort.Properties;

namespace Trizbort
{
  internal partial class MainForm : Form
  {
    private static readonly TimeSpan IdleProcessingEveryNSeconds = TimeSpan.FromSeconds(0.2);
    private readonly string mCaption;
    private DateTime mLastUpdateUiTime;
    private Map currentMap;


    public MainForm()
    {
      InitializeComponent();

      mCaption = Text;

      Application.Idle += onIdle;
      mLastUpdateUiTime = DateTime.MinValue;

      m_automapBar.StopClick += onMAutomapBarOnStopClick;

      var map = createNewMap();

      currentMap = map;

      addCanvasToTab(tabPage1, map);
      tabPage1.Controls.Add(map.Canvas);

      tabPage1.Text = map.Name;

//      Canvas.ZoomChanged += adjustZoomed;
//      Canvas.Map = new Map() {Name = "Default Map"};
//      Project.Maps.Add(Canvas.Map);
    }

    private void addCanvasToTab(TabPage tabPage, Map map)
    {
      map.Canvas.Dock = DockStyle.Fill;
      map.Canvas.MinimapVisible = true;
      map.Canvas.BackColor = Color.White;
      tabPage.Controls.Add(map.Canvas);
    }

    private Map createNewMap()
    {
      // setup initial tab and canvas
      var map = new Map
      {
        Name = "New Map"
      };

      map.Canvas = new Canvas(map);

      map.Canvas.ZoomChanged += adjustZoomed;
      Project.Maps.Add(map);
      return map;
    }

    public override sealed string Text { get { return base.Text; }
      set { base.Text = value; } }

    private  void adjustZoomed(object sender, EventArgs e)
    {
      txtZoom.Value = (int) (currentMap.Canvas.ZoomFactor*100.0f);
    }

    private void onMAutomapBarOnStopClick(object sender, EventArgs e)
    {
      currentMap.Canvas.StopAutomapping();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
      currentMap.Canvas.MinimapVisible = Settings.ShowMiniMap;

      var args = Environment.GetCommandLineArgs();
      if (args.Length > 1 && File.Exists(args[1]))
      {
        try
        {
          BeginInvoke((MethodInvoker) delegate { OpenProject(args[1]); });
        }
        catch (Exception)
        {
          // ignored
        }
      }
      NewVersionDialog.CheckForUpdatesAsync(this, false);
    }

    private void FileNewMenuItem_Click(object sender, EventArgs e)
    {
      if (!checkLoseProject())
        return;

      Project.Current = new Project();
      Settings.Reset();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (!checkLoseProject())
      {
        e.Cancel = true;
        return;
      }

      Settings.SaveApplicationSettings();
      currentMap.Canvas.StopAutomapping();

      base.OnClosing(e);
    }

    private bool checkLoseProject()
    {
      if (Project.Current.IsDirty)
      {
        // see if the user would like to save
        var result = MessageBox.Show(this, $"Do you want to save changes to {Project.Current.Name}?", Text, MessageBoxButtons.YesNoCancel);
        switch (result)
        {
          case DialogResult.Yes:
            // user would like to save
            if (!saveProject())
            {
              // didn't actually save; treat as cancel
              return false;
            }

            // user saved; carry on
            return true;

          case DialogResult.No:
            // user wouldn't like to save; carry on
            return true;

          default:
            // user cancelled; cancel
            return false;
        }
      }

      // project doesn't need saving; carry on
      return true;
    }

    private void OpenProject()
    {
      if (!checkLoseProject())
        return;

      using (var dialog = new OpenFileDialog())
      {
        dialog.InitialDirectory = PathHelper.SafeGetDirectoryName(Settings.LastProjectFileName);
        dialog.Filter = $"{Project.FilterString}|All Files|*.*||";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          Settings.LastProjectFileName = dialog.FileName;
          OpenProject(dialog.FileName);
        }
      }
    }

    private void OpenProject(string fileName)
    {
      var project = new Project {FileName = fileName};
      if (project.Load())
      {
        Project.Current = project;
        //AboutMap();
        Settings.RecentProjects.Add(fileName);
        return;
      }
    }

    private void aboutMap()
    {
      var project = Project.Current;
      if (!string.IsNullOrEmpty(project.Title) || !string.IsNullOrEmpty(project.Author) || !string.IsNullOrEmpty(project.Description))
      {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(project.Title))
        {
          builder.AppendLine(project.Title);
        }
        if (!string.IsNullOrEmpty(project.Author))
        {
          if (builder.Length > 0)
          {
            builder.AppendLine();
          }
          builder.AppendLine($"by {project.Author}");
        }
        if (!string.IsNullOrEmpty(project.Description))
        {
          if (builder.Length > 0)
          {
            builder.AppendLine();
          }
          builder.AppendLine(project.Description);
        }
        MessageBox.Show(builder.ToString(), Application.ProductName, MessageBoxButtons.OK);
      }
    }

    private bool saveProject()
    {
      if (!Project.Current.HasFileName)
      {
        return saveAsProject();
      }

      if (Project.Current.Save())
      {
        Settings.RecentProjects.Add(Project.Current.FileName);
        return true;
      }
      return false;
    }

    private bool saveAsProject()
    {
      using (var dialog = new SaveFileDialog())
      {
        if (!string.IsNullOrEmpty(Project.Current.FileName))
        {
          dialog.FileName = Project.Current.FileName;
        }
        else
        {
          dialog.InitialDirectory = PathHelper.SafeGetDirectoryName(Settings.LastProjectFileName);
        }
        dialog.Filter = $"{Project.FilterString}|All Files|*.*||";
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          Settings.LastProjectFileName = dialog.FileName;
          Project.Current.FileName = dialog.FileName;
          if (Project.Current.Save())
          {
            Settings.RecentProjects.Add(Project.Current.FileName);
            return true;
          }
        }
      }

      return false;
    }

    private void FileOpenMenuItem_Click(object sender, EventArgs e)
    {
      OpenProject();
    }

    private void FileSaveMenuItem_Click(object sender, EventArgs e)
    {
      saveProject();
    }

    private void FileSaveAsMenuItem_Click(object sender, EventArgs e)
    {
      saveAsProject();
    }

    private void smartSaveToolStripMenuItem_Click(object sender, EventArgs e)
    {
      smartSave();
    }

    private void smartSave()
    {

      if (!Settings.SaveToPDF && !Settings.SaveToImage)
      {
        MessageBox.Show("Your settings are set to not save anything. Please check your App Settings if this is not what you want.");
        return;
      }

      bool mSaved = false;
      if (!Project.Current.HasFileName || Project.Current.IsDirty)
      {
        if (MessageBox.Show("Your project needs to be saved before we can do a SmartSave.  Would you like to save the project now?", "Save Project?", MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
        {
          saveProject();
          mSaved = true;
        }
      }
      else
      {
        mSaved = true;
      }


      if (mSaved)
      {
        if (Project.Current.HasFileName)
        {
          bool bSaveError = false;
          string sPDFFile = string.Empty;
          if (Settings.SaveToPDF)
          {
            sPDFFile = exportPDF();
            if (sPDFFile == string.Empty)
            {
              MessageBox.Show("There was an error saving the PDF file during the SmartSave.  Please make sure the PDF is not already opened.", "Smart Save", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
              bSaveError = true;
            }
          }
          string sImageFile = string.Empty;
          if (Settings.SaveToImage)
          {
            sImageFile = exportImage();
            if (sImageFile == string.Empty)
            {
              MessageBox.Show("There was an error saving the Image file during the SmartSave.  Please make sure the Image is not already opened.", "Smart Save", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
              bSaveError = true;
            }

          }

          if (!bSaveError)
          {
            string sText = string.Empty;
            if (Settings.SaveToPDF)
            {
              sText += $"PDF file has been saved to {sPDFFile}";
            }

            if (Settings.SaveToImage)
            {
              if (sText != string.Empty)
                sText += Environment.NewLine;
              sText += $"Image file has been saved to {sImageFile}";
            }

            MessageBox.Show(sText, "Smart Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
          }
        }
      }
      else
      {
        MessageBox.Show("No files have been saved during the SmartSave.");
      }
    }

    private void appSettingsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Settings.ShowAppDialog();
    }

    private string exportImage()
    {
      var folder = PathHelper.SafeGetDirectoryName(Project.Current.FileName);
      var fileName = PathHelper.SafeGetFilenameWithoutExtension(Project.Current.FileName);

      var extension = getExtensionForDefaultImageType();

      var imageFile = Path.Combine(folder, fileName + extension);
      try
      {
        saveImage(imageFile);
      }
      catch (Exception)
      {
        return string.Empty;
      }

      return imageFile;
    }

    private static string getExtensionForDefaultImageType()
    {
      string extension = ".png";
      switch (Settings.DefaultImageType)
      {
        case 0:
          extension = ".png";
          break;
        case 1:
          extension = ".jpg";
          break;
        case 2:
          extension = ".bmp";
          break;
        case 3:
          extension = ".emf";
          break;
      }
      return extension;
    }

    private string exportPDF()
    {
      var folder = PathHelper.SafeGetDirectoryName(Project.Current.FileName);
      var fileName = PathHelper.SafeGetFilenameWithoutExtension(Project.Current.FileName);
      var pdfFile = Path.Combine(folder, fileName + ".pdf");
      try
      {
        savePDF(pdfFile);
      }
      catch (Exception)
      {
        return string.Empty;
      }
      return pdfFile;
    }

    private void FileExportPDFMenuItem_Click(object sender, EventArgs e)
    {
      using (var dialog = new SaveFileDialog())
      {
        dialog.Filter = "PDF Files|*.pdf|All Files|*.*||";
        dialog.Title = "Export PDF";
        dialog.InitialDirectory = PathHelper.SafeGetDirectoryName(Settings.LastExportImageFileName);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          try
          {
            savePDF(dialog.FileName);
          }
          catch (Exception ex)
          {
            MessageBox.Show(Program.MainForm, $"There was a problem exporting the map:\n\n{ex.Message}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
      }
    }

    private void savePDF(string fileName)
    {
      Settings.LastExportImageFileName = fileName;

      var doc = new PdfDocument();
      doc.Info.Title = Project.Current.Title;
      doc.Info.Author = Project.Current.Author;
      doc.Info.Creator = Application.ProductName;
      doc.Info.CreationDate = DateTime.Now;
      doc.Info.Subject = Project.Current.Description;
      var page = doc.AddPage();

      var size = currentMap.Canvas.ComputeCanvasBounds(true).Size;
      page.Width = new XUnit(size.X);
      page.Height = new XUnit(size.Y);
      using (var graphics = XGraphics.FromPdfPage(page))
      {
        currentMap.Canvas.Draw(graphics, true, size.X, size.Y);
      }

      doc.Save(fileName);
    }

    private void FileExportImageMenuItem_Click(object sender, EventArgs e)
    {
      using (var dialog = new SaveFileDialog())
      {
        dialog.Filter = "PNG Images|*.png|JPEG Images|*.jpg|BMP Images|*.bmp|Enhanced Metafiles (EMF)|*.emf|All Files|*.*||";
        dialog.Title = "Export Image";
        dialog.DefaultExt = getExtensionForDefaultImageType();
        dialog.InitialDirectory = PathHelper.SafeGetDirectoryName(Settings.LastExportImageFileName);
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          Settings.LastExportImageFileName = Path.GetDirectoryName(dialog.FileName)+@"\";
          saveImage(dialog.FileName);
        }
      }
    }

    private void saveImage(string fileName)
    {
      var format = ImageFormat.Png;
      var ext = Path.GetExtension(fileName);
      if (StringComparer.InvariantCultureIgnoreCase.Compare(ext, ".jpg") == 0
          || StringComparer.InvariantCultureIgnoreCase.Compare(ext, ".jpeg") == 0)
      {
        format = ImageFormat.Jpeg;
      }
      else if (StringComparer.InvariantCultureIgnoreCase.Compare(ext, ".bmp") == 0)
      {
        format = ImageFormat.Bmp;
      }
      else if (StringComparer.InvariantCultureIgnoreCase.Compare(ext, ".emf") == 0)
      {
        format = ImageFormat.Emf;
      }

      var size = currentMap.Canvas.ComputeCanvasBounds(true).Size*(Settings.SaveAt100 ? 1.0f : currentMap.Canvas.ZoomFactor);
      size.X = Numeric.Clamp(size.X, 16, 8192);
      size.Y = Numeric.Clamp(size.Y, 16, 8192);

      try
      {
        if (Equals(format, ImageFormat.Emf))
        {
          // export as a metafile
          using (var nativeGraphics = Graphics.FromHwnd(currentMap.Canvas.Handle))
          {
            using (var stream = new MemoryStream())
            {
              try
              {
                var dc = nativeGraphics.GetHdc();
                using (var metafile = new Metafile(stream, dc))
                {
                  using (var imageGraphics = Graphics.FromImage(metafile))
                  {
                    using (var graphics = XGraphics.FromGraphics(imageGraphics, new XSize(size.X, size.Y)))
                    {
                      currentMap.Canvas.Draw(graphics, true, size.X, size.Y);
                    }
                  }
                  var handle = metafile.GetHenhmetafile();
                  var copy = CopyEnhMetaFile(handle, fileName);
                  DeleteEnhMetaFile(copy);
                }
              }
              finally
              {
                nativeGraphics.ReleaseHdc();
              }
            }
          }
        }
        else
        {
          // export as an image
          using (var bitmap = new Bitmap((int) Math.Ceiling(size.X), (int) Math.Ceiling(size.Y)))
          {
            using (var imageGraphics = Graphics.FromImage(bitmap))
            {
              using (var graphics = XGraphics.FromGraphics(imageGraphics, new XSize(size.X, size.Y)))
              {
                currentMap.Canvas.Draw(graphics, true, size.X, size.Y);
              }
            }
            bitmap.Save(fileName, format);
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(Program.MainForm, $"There was a problem exporting the map:\n\n{ex.Message}",
          Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private void FileExportInform7MenuItem_Click(object sender, EventArgs e)
    {
      var fileName = Settings.LastExportInform7FileName;
      if (ExportCode<Inform7Exporter>(ref fileName))
      {
        Settings.LastExportInform7FileName = fileName;
      }
    }

    private void inform7ToTextToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ExportCode<Inform7Exporter>();
    }


    private void FileExportInform6MenuItem_Click(object sender, EventArgs e)
    {
      var fileName = Settings.LastExportInform6FileName;
      if (ExportCode<Inform6Exporter>(ref fileName))
      {
        Settings.LastExportInform6FileName = fileName;
      }
    }

    private void inform6ToTextToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ExportCode<Inform6Exporter>();
    }

    private void FileExportTadsMenuItem_Click(object sender, EventArgs e)
    {
      var fileName = Settings.LastExportTadsFileName;
      if (ExportCode<TadsExporter>(ref fileName))
      {
        Settings.LastExportTadsFileName = fileName;
      }
    }

    private void tADSToTextToolStripMenuItem_Click(object sender, EventArgs e)
    {
      ExportCode<TadsExporter>();
    }

    private void ExportCode<T>() where T: CodeExporter, new()
    {
      using (var exporter = new T())
      {
        string s = exporter.Export();
        Clipboard.SetText(s,TextDataFormat.Text);
      }
    }

    private bool ExportCode<T>(ref string lastExportFileName) where T : CodeExporter, new()
    {
      using (var exporter = new T())
      {
        using (var dialog = new SaveFileDialog())
        {
          // compose filter string for file dialog
          var filterString = string.Empty;
          var filters = exporter.FileDialogFilters;
          foreach (var filter in filters)
          {
            if (!string.IsNullOrEmpty(filterString))
            {
              filterString += "|";
            }
            filterString += $"{filter.Key}|*{filter.Value}";
          }

          if (!string.IsNullOrEmpty(filterString))
          {
            filterString += "|";
          }
          filterString += "All Files|*.*||";
          dialog.Filter = filterString;

          // set default filter by extension
          var extension = PathHelper.SafeGetExtension(lastExportFileName);
          for (var filterIndex = 0; filterIndex < filters.Count; ++filterIndex)
          {
            if (StringComparer.InvariantCultureIgnoreCase.Compare(extension, filters[filterIndex].Value) == 0)
            {
              dialog.FilterIndex = filterIndex + 1; // 1 based index
              break;
            }
          }

          // show dialog
          dialog.Title = exporter.FileDialogTitle;
          dialog.InitialDirectory = PathHelper.SafeGetDirectoryName(lastExportFileName);
          if (dialog.ShowDialog() == DialogResult.OK)
          {
            try
            {
              // export source code
              exporter.Export(dialog.FileName);
              lastExportFileName = dialog.FileName;
              return true;
            }
            catch (Exception ex)
            {
              MessageBox.Show(Program.MainForm, $"There was a problem exporting the map:\n\n{ex.Message}", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          }
        }
      }

      return false;
    }

    private void FileExitMenuItem_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void EditAddRoomMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.AddRoom(false);
    }

    private void EditDeleteMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.DeleteSelection();
    }

    private void EditPropertiesMenuItem_Click(object sender, EventArgs e)
    {
      if (currentMap.Canvas.HasSingleSelectedElement && currentMap.Canvas.SelectedElement.HasDialog)
      {
        currentMap.Canvas.SelectedElement.ShowDialog();
      }
    }

    private void PlainLinesMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ApplyNewPlainConnectionSettings();
    }

    private void ToggleDottedLines_Click(object sender, EventArgs e)
    {
      switch (currentMap.Canvas.NewConnectionStyle)
      {
        case ConnectionStyle.Solid:
          currentMap.Canvas.NewConnectionStyle = ConnectionStyle.Dashed;
          break;
        case ConnectionStyle.Dashed:
          currentMap.Canvas.NewConnectionStyle = ConnectionStyle.Solid;
          break;
      }
      currentMap.Canvas.ApplyConnectionStyle(currentMap.Canvas.NewConnectionStyle);
    }

    private void ToggleDirectionalLines_Click(object sender, EventArgs e)
    {
      switch (currentMap.Canvas.NewConnectionFlow)
      {
        case ConnectionFlow.TwoWay:
          currentMap.Canvas.NewConnectionFlow = ConnectionFlow.OneWay;
          break;
        case ConnectionFlow.OneWay:
          currentMap.Canvas.NewConnectionFlow = ConnectionFlow.TwoWay;
          break;
      }
      currentMap.Canvas.ApplyConnectionFlow(currentMap.Canvas.NewConnectionFlow);
    }

    private void UpLinesMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.NewConnectionLabel = ConnectionLabel.Up;
      currentMap.Canvas.ApplyConnectionLabel(currentMap.Canvas.NewConnectionLabel);
    }

    private void DownLinesMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.NewConnectionLabel = ConnectionLabel.Down;
      currentMap.Canvas.ApplyConnectionLabel(currentMap.Canvas.NewConnectionLabel);
    }

    private void InLinesMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.NewConnectionLabel = ConnectionLabel.In;
      currentMap.Canvas.ApplyConnectionLabel(currentMap.Canvas.NewConnectionLabel);
    }

    private void OutLinesMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.NewConnectionLabel = ConnectionLabel.Out;
      currentMap.Canvas.ApplyConnectionLabel(currentMap.Canvas.NewConnectionLabel);
    }

    private void ReverseLineMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ReverseLineDirection();
    }

    private void onIdle(object sender, EventArgs e)
    {
      var now = DateTime.Now;
      if (now - mLastUpdateUiTime > IdleProcessingEveryNSeconds)
      {
        mLastUpdateUiTime = now;
        updateCommandUi();
      }
    }

    private void updateCommandUi()
    {
      // caption
      Text = $"{Project.Current.Name}{(Project.Current.IsDirty ? "*" : string.Empty)} - {mCaption} - {Application.ProductVersion}";

      // line drawing options
      m_toggleDottedLinesButton.Checked = currentMap.Canvas.NewConnectionStyle == ConnectionStyle.Dashed;
      m_toggleDottedLinesMenuItem.Checked = m_toggleDottedLinesButton.Checked;
      m_toggleDirectionalLinesButton.Checked = currentMap.Canvas.NewConnectionFlow == ConnectionFlow.OneWay;
      m_toggleDirectionalLinesMenuItem.Checked = m_toggleDirectionalLinesButton.Checked;
      m_plainLinesMenuItem.Checked = !m_toggleDirectionalLinesMenuItem.Checked && !m_toggleDottedLinesMenuItem.Checked && currentMap.Canvas.NewConnectionLabel == ConnectionLabel.None;
      m_upLinesMenuItem.Checked = currentMap.Canvas.NewConnectionLabel == ConnectionLabel.Up;
      m_downLinesMenuItem.Checked = currentMap.Canvas.NewConnectionLabel == ConnectionLabel.Down;
      m_inLinesMenuItem.Checked = currentMap.Canvas.NewConnectionLabel == ConnectionLabel.In;
      m_outLinesMenuItem.Checked = currentMap.Canvas.NewConnectionLabel == ConnectionLabel.Out;

      // selection-specific commands
      var hasSelectedElement = currentMap.Canvas.SelectedElement != null;
      m_editDeleteMenuItem.Enabled = hasSelectedElement;
      m_editPropertiesMenuItem.Enabled = currentMap.Canvas.HasSingleSelectedElement;
      m_editIsDarkMenuItem.Enabled = hasSelectedElement;
      m_editSelectNoneMenuItem.Enabled = hasSelectedElement;
      m_editSelectAllMenuItem.Enabled = currentMap.Canvas.SelectedElementCount < Project.Maps.Sum(map=>map.Elements.Count);
      m_editCopyMenuItem.Enabled = currentMap.Canvas.SelectedElement != null;
      m_editCopyColorToolMenuItem.Enabled = currentMap.Canvas.HasSingleSelectedElement && (currentMap.Canvas.SelectedElement is Room);
      m_editPasteMenuItem.Enabled = (!String.IsNullOrEmpty(Clipboard.GetText())) && ((Clipboard.GetText().Replace("\r\n", "|").Split('|')[0] == "Elements") || (Clipboard.GetText().Replace("\r\n", "|").Split('|')[0] == "Colors"));
      m_editRenameMenuItem.Enabled = currentMap.Canvas.HasSingleSelectedElement && (currentMap.Canvas.SelectedElement is Room);
      m_editIsDarkMenuItem.Enabled = currentMap.Canvas.HasSingleSelectedElement && (currentMap.Canvas.SelectedElement is Room);
      m_editIsDarkMenuItem.Checked = currentMap.Canvas.HasSingleSelectedElement && (currentMap.Canvas.SelectedElement is Room) && ((Room)currentMap.Canvas.SelectedElement).IsDark;
      m_reverseLineMenuItem.Enabled = currentMap.Canvas.HasSelectedElement<Connection>();

      // automapping
      m_automapStartMenuItem.Enabled = !currentMap.Canvas.IsAutomapping;
      m_automapStopMenuItem.Enabled = currentMap.Canvas.IsAutomapping;
      m_automapBar.Visible = currentMap.Canvas.IsAutomapping;
      m_automapBar.Status = currentMap.Canvas.AutomappingStatus;

      // minimap
      m_viewMinimapMenuItem.Checked = currentMap.Canvas.MinimapVisible;

      updateToolStripImages();
      currentMap.Canvas.UpdateScrollBars();

      //Debug.WriteLine(Canvas.Focused ? "Focused!" : "NOT FOCUSED");
    }

    private void FileRecentProject_Click(object sender, EventArgs e)
    {
      if (!checkLoseProject())
      {
        return;
      }

      var fileName = (string) ((ToolStripMenuItem) sender).Tag;
      OpenProject(fileName);
    }

    private void updateToolStripImages()
    {
      foreach (ToolStripItem item in m_toolStrip.Items)
      {
        if (!(item is ToolStripButton))
          continue;

        var button = (ToolStripButton) item;
        button.BackgroundImage = button.Checked ? Resources.ToolStripBackground2 : Resources.ToolStripBackground;
      }
    }

    private void ViewResetMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ResetZoomOrigin();
    }

    private void ViewZoomInMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ZoomIn();
    }

    private void ViewZoomOutMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ZoomOut();
    }

    private void ViewZoomFiftyPercentMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ZoomFactor = 0.5f;
    }

    private void ViewZoomOneHundredPercentMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ZoomFactor = 1.0f;
    }

    private void ViewZoomTwoHundredPercentMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ZoomFactor = 2.0f;
    }

    private void EditRenameMenuItem_Click(object sender, EventArgs e)
    {
      if (currentMap.Canvas.HasSingleSelectedElement && currentMap.Canvas.SelectedElement.HasDialog)
      {
        currentMap.Canvas.SelectedElement.ShowDialog();
      }
    }

    private void EditIsDarkMenuItem_Click(object sender, EventArgs e)
    {
      foreach (var room in currentMap.Canvas.SelectedElements.Where(element => element.GetType() == typeof(Room)).Cast<Room>()) {
        room.IsDark = !room.IsDark;
      }
    }

    private void ProjectSettingsMenuItem_Click(object sender, EventArgs e)
    {
      Settings.ShowMapDialog();
      currentMap.Canvas.Refresh();
    }

    private void ProjectResetToDefaultSettingsMenuItem_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show("Restore default settings?\n\nThis will revert any changes to settings in this project.", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
      {
        Settings.Reset();
      }
    }

    private void HelpAndSupportMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        Process.Start("http://trizbort.genstein.net/?help");
      }
      catch (Exception)
      {
        NewVersionDialog.CannotLaunchWebSite();
      }
    }

    private void CheckForUpdatesMenuItem_Click(object sender, EventArgs e)
    {
      NewVersionDialog.CheckForUpdatesAsync(this, true);
    }

    private void HelpAboutMenuItem_Click(object sender, EventArgs e)
    {
      using (var dialog = new AboutDialog())
      {
        dialog.ShowDialog();
      }
    }

    private void AutomapStartMenuItem_Click(object sender, EventArgs e)
    {
      using (var dialog = new AutomapDialog())
      {
        if (dialog.ShowDialog() == DialogResult.OK)
        {
          currentMap.Canvas.StartAutomapping(dialog.Data);
        }
      }
    }

    private void AutomapStopMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.StopAutomapping();
    }

    private void ViewMinimapMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.MinimapVisible = !currentMap.Canvas.MinimapVisible;
      Settings.ShowMiniMap = currentMap.Canvas.MinimapVisible;
    }

    private void FileMenu_DropDownOpening(object sender, EventArgs e)
    {
      setupMruMenu();

      setupExportMenu();
    }

    private void setupExportMenu()
    {
      if (Project.Maps.Sum(map => map.Elements.OfType<Room>().Count()) > 0)
      {
        m_fileExportInform7MenuItem.Enabled = true;
        m_fileExportInform6MenuItem.Enabled = true;
        m_fileExportTADSMenuItem.Enabled = true;
      }
      else
      {
        m_fileExportInform7MenuItem.Enabled = false;
        m_fileExportInform6MenuItem.Enabled = false;
        m_fileExportTADSMenuItem.Enabled = false;
      }
    }

    private void setupMruMenu()
    {
      var existingItems = m_fileRecentMapsMenuItem.DropDownItems.Cast<ToolStripItem>().ToList();
      foreach (var existingItem in existingItems)
      {
        existingItem.Click -= FileRecentProject_Click;
        existingItem.Dispose();
      }
      if (Settings.RecentProjects.Count == 0)
      {
        m_fileRecentMapsMenuItem.Enabled = false;
      }
      else
      {
        m_fileRecentMapsMenuItem.Enabled = true;
        var index = 1;
        var removedFiles = new List<string>();
        foreach (var recentProject in Settings.RecentProjects)
        {
          if (File.Exists(recentProject))
          {
            var menuItem = new ToolStripMenuItem($"&{index++} {recentProject}") {Tag = recentProject};
            menuItem.Click += FileRecentProject_Click;
            m_fileRecentMapsMenuItem.DropDownItems.Add(menuItem);
          }
          else
          {
            removedFiles.Add(recentProject);
          }
        }
        if (removedFiles.Any())
        {
          removedFiles.ForEach(p => Settings.RecentProjects.Remove(p));
        }
      }
    }

    private void EditSelectAllMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectAll();
    }

    private void EditSelectNoneMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectedElement = null;
    }

    private void ViewEntireMapMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.ZoomToFit();
    }

    [DllImport("gdi32.dll")]
    private static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string lpszFile);

    [DllImport("gdi32.dll")]
    private static extern int DeleteEnhMetaFile(IntPtr hemf);

    private void m_editCopyMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.CopySelectedElements();
    }

    private void m_editCopyColorToolMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.CopySelectedColor();
    }

    private void m_editPasteMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.Paste(false);
    }

    private void m_editChangeRegionMenuItem_Click(object sender, EventArgs e)
    {
      if (currentMap.Canvas.HasSingleSelectedElement && currentMap.Canvas.SelectedElement.HasDialog)
      {
        var element = currentMap.Canvas.SelectedElement as Room;
        if (element != null)
        {
          var room = element;
          room.ShowDialog(PropertiesStartType.Region);
          
        }
      }
    }

    private void txtZoom_ValueChanged(object sender, EventArgs e)
    {
      if (txtZoom.Value <= 0) txtZoom.Value = 10;
      currentMap.Canvas.ChangeZoom((float)Convert.ToDouble(txtZoom.Value) / 100.0f);
    }

    private void mapStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var frm = new MapStatisticsView();
      frm.ShowDialog();
    }

    private void selectAllRoomsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectAllRooms();
    }

    private void selectedUnconnectedRoomsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectAllUnconnectedRooms();
    }

    private void selectAllConnectionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectAllConnections();
    }

    private void selectDanglingConnectionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectDanglingConnections();
    }

    private void selectSelfLoopingConnectionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectSelfLoopingConnections();
    }

    private void selectRoomsWObjectsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectRoomsWithObjects();
    }

    private void selectRoomsWoObjectsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      currentMap.Canvas.SelectRoomsWithoutObjects();
    }
  }
}