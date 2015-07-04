namespace Trizbort
{
  partial class MainForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.canvas1 = new Trizbort.UserControls.Canvas();
      this.SuspendLayout();
      // 
      // canvas1
      // 
      this.canvas1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.canvas1.Location = new System.Drawing.Point(0, 0);
      this.canvas1.Name = "canvas1";
      this.canvas1.Size = new System.Drawing.Size(1008, 647);
      this.canvas1.TabIndex = 0;
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1008, 647);
      this.Controls.Add(this.canvas1);
      this.Name = "MainForm";
      this.Text = "MainForm";
      this.ResumeLayout(false);

    }

    #endregion

    private UserControls.Canvas canvas1;
  }
}