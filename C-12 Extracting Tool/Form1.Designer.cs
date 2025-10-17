namespace C_12_Extracting_Tool
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.numericLevelID = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericSectionID = new System.Windows.Forms.NumericUpDown();
            this.button4 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericLevelID)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSectionID)).BeginInit();
            this.SuspendLayout();
            // 
            // numericLevelID
            // 
            this.numericLevelID.Location = new System.Drawing.Point(65, 63);
            this.numericLevelID.Maximum = new decimal(new int[] {
            17,
            0,
            0,
            0});
            this.numericLevelID.Name = "numericLevelID";
            this.numericLevelID.Size = new System.Drawing.Size(47, 20);
            this.numericLevelID.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Level ID";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(138, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Section ID";
            // 
            // numericSectionID
            // 
            this.numericSectionID.Location = new System.Drawing.Point(201, 63);
            this.numericSectionID.Maximum = new decimal(new int[] {
            7,
            0,
            0,
            0});
            this.numericSectionID.Name = "numericSectionID";
            this.numericSectionID.Size = new System.Drawing.Size(47, 20);
            this.numericSectionID.TabIndex = 6;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(75, 12);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(100, 40);
            this.button4.TabIndex = 7;
            this.button4.Text = "Open";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 95);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.numericSectionID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numericLevelID);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "C-12 Extracting Tool";
            ((System.ComponentModel.ISupportInitialize)(this.numericLevelID)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSectionID)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.NumericUpDown numericLevelID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericSectionID;
        private System.Windows.Forms.Button button4;
    }
}

