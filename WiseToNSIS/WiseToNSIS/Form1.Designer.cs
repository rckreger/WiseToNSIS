namespace WiseToNSIS
{
   partial class Form1
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
         this.buttonInputFile = new System.Windows.Forms.Button();
         this.buttonOutputFile = new System.Windows.Forms.Button();
         this.textBoxWise = new System.Windows.Forms.TextBox();
         this.textBoxNSIS = new System.Windows.Forms.TextBox();
         this.buttonConvert = new System.Windows.Forms.Button();
         this.label1 = new System.Windows.Forms.Label();
         this.label2 = new System.Windows.Forms.Label();
         this.label3 = new System.Windows.Forms.Label();
         this.checkBoxEnableComments = new System.Windows.Forms.CheckBox();
         this.label4 = new System.Windows.Forms.Label();
         this.textBoxTemplate = new System.Windows.Forms.TextBox();
         this.buttonTemplate = new System.Windows.Forms.Button();
         this.checkBoxCodeGenerator = new System.Windows.Forms.CheckBox();
         this.SuspendLayout();
         // 
         // buttonInputFile
         // 
         this.buttonInputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonInputFile.Location = new System.Drawing.Point(691, 25);
         this.buttonInputFile.Name = "buttonInputFile";
         this.buttonInputFile.Size = new System.Drawing.Size(75, 23);
         this.buttonInputFile.TabIndex = 0;
         this.buttonInputFile.Text = "...";
         this.buttonInputFile.UseVisualStyleBackColor = true;
         this.buttonInputFile.Click += new System.EventHandler(this.buttonInputFile_Click);
         // 
         // buttonOutputFile
         // 
         this.buttonOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonOutputFile.Location = new System.Drawing.Point(691, 55);
         this.buttonOutputFile.Name = "buttonOutputFile";
         this.buttonOutputFile.Size = new System.Drawing.Size(75, 23);
         this.buttonOutputFile.TabIndex = 1;
         this.buttonOutputFile.Text = "...";
         this.buttonOutputFile.UseVisualStyleBackColor = true;
         this.buttonOutputFile.Click += new System.EventHandler(this.buttonOutputFile_Click);
         // 
         // textBoxWise
         // 
         this.textBoxWise.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxWise.Location = new System.Drawing.Point(64, 27);
         this.textBoxWise.Name = "textBoxWise";
         this.textBoxWise.Size = new System.Drawing.Size(621, 20);
         this.textBoxWise.TabIndex = 2;
         this.textBoxWise.TextChanged += new System.EventHandler(this.textBoxWise_TextChanged);
         // 
         // textBoxNSIS
         // 
         this.textBoxNSIS.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxNSIS.Location = new System.Drawing.Point(64, 57);
         this.textBoxNSIS.Name = "textBoxNSIS";
         this.textBoxNSIS.Size = new System.Drawing.Size(621, 20);
         this.textBoxNSIS.TabIndex = 3;
         this.textBoxNSIS.TextChanged += new System.EventHandler(this.textBoxNSIS_TextChanged);
         // 
         // buttonConvert
         // 
         this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonConvert.Location = new System.Drawing.Point(691, 128);
         this.buttonConvert.Name = "buttonConvert";
         this.buttonConvert.Size = new System.Drawing.Size(75, 23);
         this.buttonConvert.TabIndex = 4;
         this.buttonConvert.Text = "Convert";
         this.buttonConvert.UseVisualStyleBackColor = true;
         this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
         // 
         // label1
         // 
         this.label1.AutoSize = true;
         this.label1.Location = new System.Drawing.Point(-12, 33);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(16, 13);
         this.label1.TabIndex = 5;
         this.label1.Text = "In";
         // 
         // label2
         // 
         this.label2.AutoSize = true;
         this.label2.Location = new System.Drawing.Point(23, 32);
         this.label2.Name = "label2";
         this.label2.Size = new System.Drawing.Size(31, 13);
         this.label2.TabIndex = 6;
         this.label2.Text = "Wise";
         // 
         // label3
         // 
         this.label3.AutoSize = true;
         this.label3.Location = new System.Drawing.Point(22, 64);
         this.label3.Name = "label3";
         this.label3.Size = new System.Drawing.Size(32, 13);
         this.label3.TabIndex = 7;
         this.label3.Text = "NSIS";
         this.label3.Click += new System.EventHandler(this.label3_Click);
         // 
         // checkBoxEnableComments
         // 
         this.checkBoxEnableComments.AutoSize = true;
         this.checkBoxEnableComments.Location = new System.Drawing.Point(26, 126);
         this.checkBoxEnableComments.Name = "checkBoxEnableComments";
         this.checkBoxEnableComments.Size = new System.Drawing.Size(111, 17);
         this.checkBoxEnableComments.TabIndex = 8;
         this.checkBoxEnableComments.Text = "Enable Comments";
         this.checkBoxEnableComments.UseVisualStyleBackColor = true;
         this.checkBoxEnableComments.CheckedChanged += new System.EventHandler(this.checkBoxEnableComments_CheckedChanged);
         // 
         // label4
         // 
         this.label4.AutoSize = true;
         this.label4.Location = new System.Drawing.Point(6, 88);
         this.label4.Name = "label4";
         this.label4.Size = new System.Drawing.Size(51, 13);
         this.label4.TabIndex = 11;
         this.label4.Text = "Template";
         this.label4.Click += new System.EventHandler(this.label4_Click);
         // 
         // textBoxTemplate
         // 
         this.textBoxTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                     | System.Windows.Forms.AnchorStyles.Right)));
         this.textBoxTemplate.Location = new System.Drawing.Point(63, 85);
         this.textBoxTemplate.Name = "textBoxTemplate";
         this.textBoxTemplate.Size = new System.Drawing.Size(621, 20);
         this.textBoxTemplate.TabIndex = 10;
         this.textBoxTemplate.TextChanged += new System.EventHandler(this.textBoxTemplate_TextChanged);
         // 
         // buttonTemplate
         // 
         this.buttonTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
         this.buttonTemplate.Location = new System.Drawing.Point(690, 83);
         this.buttonTemplate.Name = "buttonTemplate";
         this.buttonTemplate.Size = new System.Drawing.Size(75, 23);
         this.buttonTemplate.TabIndex = 9;
         this.buttonTemplate.Text = "...";
         this.buttonTemplate.UseVisualStyleBackColor = true;
         this.buttonTemplate.Click += new System.EventHandler(this.buttonTemplate_Click);
         // 
         // checkBoxCodeGenerator
         // 
         this.checkBoxCodeGenerator.AutoSize = true;
         this.checkBoxCodeGenerator.Location = new System.Drawing.Point(25, 149);
         this.checkBoxCodeGenerator.Name = "checkBoxCodeGenerator";
         this.checkBoxCodeGenerator.Size = new System.Drawing.Size(101, 17);
         this.checkBoxCodeGenerator.TabIndex = 12;
         this.checkBoxCodeGenerator.Text = "Code Generator";
         this.checkBoxCodeGenerator.UseVisualStyleBackColor = true;
         this.checkBoxCodeGenerator.CheckedChanged += new System.EventHandler(this.checkBoxCodeGenerator_CheckedChanged);
         // 
         // Form1
         // 
         this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
         this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
         this.ClientSize = new System.Drawing.Size(785, 178);
         this.Controls.Add(this.checkBoxCodeGenerator);
         this.Controls.Add(this.label4);
         this.Controls.Add(this.textBoxTemplate);
         this.Controls.Add(this.buttonTemplate);
         this.Controls.Add(this.checkBoxEnableComments);
         this.Controls.Add(this.label3);
         this.Controls.Add(this.label2);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.buttonConvert);
         this.Controls.Add(this.textBoxNSIS);
         this.Controls.Add(this.textBoxWise);
         this.Controls.Add(this.buttonOutputFile);
         this.Controls.Add(this.buttonInputFile);
         this.Name = "Form1";
         this.Text = "Wise 9.x to NSIS Converter";
         this.ResumeLayout(false);
         this.PerformLayout();

      }

      #endregion

      private System.Windows.Forms.Button buttonInputFile;
      private System.Windows.Forms.Button buttonOutputFile;
      private System.Windows.Forms.TextBox textBoxWise;
      private System.Windows.Forms.TextBox textBoxNSIS;
      private System.Windows.Forms.Button buttonConvert;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.Label label2;
      private System.Windows.Forms.Label label3;
      private System.Windows.Forms.CheckBox checkBoxEnableComments;
      private System.Windows.Forms.Label label4;
      private System.Windows.Forms.TextBox textBoxTemplate;
      private System.Windows.Forms.Button buttonTemplate;
      private System.Windows.Forms.CheckBox checkBoxCodeGenerator;
   }
}

