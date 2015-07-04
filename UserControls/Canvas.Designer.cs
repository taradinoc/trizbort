namespace Trizbort.UserControls
{
  sealed partial class Canvas
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.m_hScrollBar = new System.Windows.Forms.HScrollBar();
      this.m_vScrollBar = new System.Windows.Forms.VScrollBar();
      this.mCornerPanel = new System.Windows.Forms.Panel();
      this.SuspendLayout();
      // 
      // m_hScrollBar
      // 
      this.m_hScrollBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_hScrollBar.Location = new System.Drawing.Point(0, 413);
      this.m_hScrollBar.Name = "m_hScrollBar";
      this.m_hScrollBar.Size = new System.Drawing.Size(526, 16);
      this.m_hScrollBar.TabIndex = 3;
      // 
      // m_vScrollBar
      // 
      this.m_vScrollBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_vScrollBar.Location = new System.Drawing.Point(526, 0);
      this.m_vScrollBar.Name = "m_vScrollBar";
      this.m_vScrollBar.Size = new System.Drawing.Size(19, 413);
      this.m_vScrollBar.TabIndex = 2;
      // 
      // mCornerPanel
      // 
      this.mCornerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.mCornerPanel.Location = new System.Drawing.Point(526, 410);
      this.mCornerPanel.Name = "mCornerPanel";
      this.mCornerPanel.Size = new System.Drawing.Size(16, 16);
      this.mCornerPanel.TabIndex = 4;
      // 
      // Canvas
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.mCornerPanel);
      this.Controls.Add(this.m_hScrollBar);
      this.Controls.Add(this.m_vScrollBar);
      this.Name = "Canvas";
      this.Size = new System.Drawing.Size(545, 429);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.HScrollBar m_hScrollBar;
    private System.Windows.Forms.VScrollBar m_vScrollBar;
    private System.Windows.Forms.Panel mCornerPanel;
  }
}
