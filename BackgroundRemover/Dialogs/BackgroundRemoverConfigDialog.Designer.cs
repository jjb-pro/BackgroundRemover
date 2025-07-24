namespace Dialogs
{
    partial class BackgroundRemoverConfigDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        ///// <summary>
        ///// Clean up any resources being used.
        ///// </summary>
        ///// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackgroundRemoverConfigDialog));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            label3 = new System.Windows.Forms.Label();
            gpuCheckBox = new System.Windows.Forms.CheckBox();
            fp16CheckBox = new System.Windows.Forms.CheckBox();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Controls.Add(label2, 0, 1);
            tableLayoutPanel1.Controls.Add(button1, 0, 6);
            tableLayoutPanel1.Controls.Add(label3, 0, 3);
            tableLayoutPanel1.Controls.Add(gpuCheckBox, 1, 3);
            tableLayoutPanel1.Controls.Add(fp16CheckBox, 1, 1);
            tableLayoutPanel1.Controls.Add(label4, 0, 2);
            tableLayoutPanel1.Controls.Add(label5, 0, 4);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tableLayoutPanel1.Size = new System.Drawing.Size(384, 261);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label1, 2);
            label1.Font = new System.Drawing.Font("Segoe UI", 14F);
            label1.Location = new System.Drawing.Point(13, 20);
            label1.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(129, 25);
            label1.TabIndex = 0;
            label1.Text = "Configuration";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Dock = System.Windows.Forms.DockStyle.Left;
            label2.Location = new System.Drawing.Point(13, 55);
            label2.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(54, 17);
            label2.TabIndex = 1;
            label2.Text = "Use FP16";
            // 
            // button1
            // 
            tableLayoutPanel1.SetColumnSpan(button1, 2);
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.ForeColor = System.Drawing.Color.Black;
            button1.Location = new System.Drawing.Point(13, 225);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 5;
            button1.Text = "OK";
            button1.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Dock = System.Windows.Forms.DockStyle.Left;
            label3.Location = new System.Drawing.Point(13, 122);
            label3.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(52, 17);
            label3.TabIndex = 2;
            label3.Text = "Use GPU";
            // 
            // gpuCheckBox
            // 
            gpuCheckBox.AutoSize = true;
            gpuCheckBox.Checked = true;
            gpuCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            gpuCheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            gpuCheckBox.Location = new System.Drawing.Point(356, 122);
            gpuCheckBox.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            gpuCheckBox.Name = "gpuCheckBox";
            gpuCheckBox.Size = new System.Drawing.Size(15, 14);
            gpuCheckBox.TabIndex = 4;
            gpuCheckBox.UseVisualStyleBackColor = true;
            // 
            // fp16CheckBox
            // 
            fp16CheckBox.AutoSize = true;
            fp16CheckBox.Dock = System.Windows.Forms.DockStyle.Right;
            fp16CheckBox.Location = new System.Drawing.Point(356, 55);
            fp16CheckBox.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
            fp16CheckBox.Name = "fp16CheckBox";
            fp16CheckBox.Size = new System.Drawing.Size(15, 14);
            fp16CheckBox.TabIndex = 3;
            fp16CheckBox.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label4, 2);
            label4.Location = new System.Drawing.Point(13, 82);
            label4.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(357, 30);
            label4.TabIndex = 6;
            label4.Text = "Use half precision (FP16) for faster, more efficient performance on diverse hardware; otherwise, full precision (FP32) is used.";
            // 
            // label5
            // 
            label5.AutoSize = true;
            tableLayoutPanel1.SetColumnSpan(label5, 2);
            label5.Location = new System.Drawing.Point(13, 149);
            label5.Margin = new System.Windows.Forms.Padding(3, 10, 3, 0);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(356, 30);
            label5.TabIndex = 7;
            label5.Text = "Enable GPU acceleration via DirectML for near-instant background removal on most DirectX 12-compatible GPUs.";
            // 
            // BackgroundRemoverConfigDialog
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(384, 261);
            Controls.Add(tableLayoutPanel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BackgroundRemoverConfigDialog";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Background Remover";
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        protected System.Windows.Forms.CheckBox gpuCheckBox;
        protected System.Windows.Forms.CheckBox fp16CheckBox;
    }
}