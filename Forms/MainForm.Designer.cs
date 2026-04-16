
﻿namespace PSRevitAddin.Forms
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
            button1 = new Button();
            button2 = new Button();
            comboBox1 = new ComboBox();
            comboBox2 = new ComboBox();
            label1 = new Label();
            label2 = new Label();
            checkBox1 = new CheckBox();
            checkBox2 = new CheckBox();
            checkBox3 = new CheckBox();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            label12 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            checkBox9 = new CheckBox();
            label14 = new Label();
            textBox5 = new TextBox();
            label13 = new Label();
            comboBox7 = new ComboBox();
            checkBox7 = new CheckBox();
            checkBox6 = new CheckBox();
            label5 = new Label();
            textBox2 = new TextBox();
            label4 = new Label();
            textBox1 = new TextBox();
            label3 = new Label();
            comboBox3 = new ComboBox();
            tabPage2 = new TabPage();
            checkBox5 = new CheckBox();
            checkBox4 = new CheckBox();
            label11 = new Label();
            label8 = new Label();
            comboBox4 = new ComboBox();
            label9 = new Label();
            comboBox5 = new ComboBox();
            label10 = new Label();
            comboBox6 = new ComboBox();
            label6 = new Label();
            textBox3 = new TextBox();
            label7 = new Label();
            textBox4 = new TextBox();
            tabPage3 = new TabPage();
            dataGridView1 = new DataGridView();
            button3 = new Button();
            button4 = new Button();
            checkBox8 = new CheckBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(872, 161);
            button1.Name = "button1";
            button1.Size = new Size(93, 23);
            button1.TabIndex = 0;
            button1.Text = "Finish";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Location = new Point(872, 47);
            button2.Name = "button2";
            button2.Size = new Size(93, 23);
            button2.TabIndex = 1;
            button2.Text = "Create WIn";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click_1;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(10, 384);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(157, 23);
            comboBox1.TabIndex = 3;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(10, 442);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(157, 23);
            comboBox2.TabIndex = 4;
            comboBox2.SelectedIndexChanged += comboBox2_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 363);
            label1.Name = "label1";
            label1.Size = new Size(67, 15);
            label1.TabIndex = 5;
            label1.Text = "창호프레임";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(10, 421);
            label2.Name = "label2";
            label2.Size = new Size(55, 15);
            label2.TabIndex = 6;
            label2.Text = "유리종류";
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(90, 272);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(50, 19);
            checkBox1.TabIndex = 9;
            checkBox1.Text = "방화";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // checkBox2
            // 
            checkBox2.AutoSize = true;
            checkBox2.Location = new Point(90, 297);
            checkBox2.Name = "checkBox2";
            checkBox2.Size = new Size(50, 19);
            checkBox2.TabIndex = 10;
            checkBox2.Text = "단열";
            checkBox2.UseVisualStyleBackColor = true;
            checkBox2.CheckedChanged += checkBox2_CheckedChanged;
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Location = new Point(90, 322);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new Size(74, 19);
            checkBox3.TabIndex = 11;
            checkBox3.Text = "전동개폐";
            checkBox3.UseVisualStyleBackColor = true;
            checkBox3.CheckedChanged += checkBox3_CheckedChanged;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
            tabControl1.Dock = DockStyle.Left;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(866, 582);
            tabControl1.TabIndex = 14;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(label12);
            tabPage1.Controls.Add(flowLayoutPanel1);
            tabPage1.Controls.Add(checkBox9);
            tabPage1.Controls.Add(label14);
            tabPage1.Controls.Add(textBox5);
            tabPage1.Controls.Add(label13);
            tabPage1.Controls.Add(comboBox7);
            tabPage1.Controls.Add(checkBox7);
            tabPage1.Controls.Add(checkBox6);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(textBox2);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(checkBox1);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(comboBox3);
            tabPage1.Controls.Add(checkBox2);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(comboBox2);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(checkBox3);
            tabPage1.Controls.Add(comboBox1);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(858, 554);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Window";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(10, 482);
            label12.Name = "label12";
            label12.Size = new Size(55, 15);
            label12.TabIndex = 35;
            label12.Text = "개폐방식";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Dock = DockStyle.Right;
            flowLayoutPanel1.Location = new Point(181, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(674, 548);
            flowLayoutPanel1.TabIndex = 15;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.Paint += flowLayoutPanel1_Paint;
            // 
            // checkBox9
            // 
            checkBox9.AutoSize = true;
            checkBox9.Location = new Point(13, 322);
            checkBox9.Name = "checkBox9";
            checkBox9.Size = new Size(74, 19);
            checkBox9.TabIndex = 33;
            checkBox9.Text = "Jinheung";
            checkBox9.UseVisualStyleBackColor = true;
            checkBox9.CheckedChanged += checkBox9_CheckedChanged;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(10, 128);
            label14.Name = "label14";
            label14.Size = new Size(14, 15);
            label14.TabIndex = 32;
            label14.Text = "b";
            // 
            // textBox5
            // 
            textBox5.Location = new Point(10, 146);
            textBox5.Name = "textBox5";
            textBox5.Size = new Size(100, 23);
            textBox5.TabIndex = 31;
            textBox5.Text = "0";
            textBox5.TextAlign = HorizontalAlignment.Center;
            textBox5.TextChanged += textBox5_TextChanged;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(10, 23);
            label13.Name = "label13";
            label13.Size = new Size(55, 15);
            label13.TabIndex = 30;
            label13.Text = "창호유형";
            // 
            // comboBox7
            // 
            comboBox7.FormattingEnabled = true;
            comboBox7.Location = new Point(10, 45);
            comboBox7.Name = "comboBox7";
            comboBox7.Size = new Size(154, 23);
            comboBox7.TabIndex = 25;
            comboBox7.SelectedIndexChanged += comboBox7_SelectedIndexChanged;
            // 
            // checkBox7
            // 
            checkBox7.AutoSize = true;
            checkBox7.Location = new Point(13, 297);
            checkBox7.Name = "checkBox7";
            checkBox7.Size = new Size(65, 19);
            checkBox7.TabIndex = 20;
            checkBox7.Text = "LX Z:IN";
            checkBox7.UseVisualStyleBackColor = true;
            checkBox7.CheckedChanged += checkBox7_CheckedChanged;
            // 
            // checkBox6
            // 
            checkBox6.AutoSize = true;
            checkBox6.Location = new Point(13, 272);
            checkBox6.Name = "checkBox6";
            checkBox6.Size = new Size(59, 19);
            checkBox6.TabIndex = 19;
            checkBox6.Text = "Eagon";
            checkBox6.UseVisualStyleBackColor = true;
            checkBox6.CheckedChanged += checkBox6_CheckedChanged;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(10, 172);
            label5.Name = "label5";
            label5.Size = new Size(14, 15);
            label5.TabIndex = 17;
            label5.Text = "h";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(10, 190);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(100, 23);
            textBox2.TabIndex = 16;
            textBox2.Text = "0";
            textBox2.TextAlign = HorizontalAlignment.Center;
            textBox2.TextChanged += textBox2_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(10, 81);
            label4.Name = "label4";
            label4.Size = new Size(16, 15);
            label4.TabIndex = 15;
            label4.Text = "w";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(10, 102);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(100, 23);
            textBox1.TabIndex = 14;
            textBox1.Text = "0";
            textBox1.TextAlign = HorizontalAlignment.Center;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // label3
            // 
            label3.Location = new Point(0, 0);
            label3.Name = "label3";
            label3.Size = new Size(100, 23);
            label3.TabIndex = 34;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(10, 503);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(157, 23);
            comboBox3.TabIndex = 7;
            comboBox3.SelectedIndexChanged += comboBox3_SelectedIndexChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(checkBox5);
            tabPage2.Controls.Add(checkBox4);
            tabPage2.Controls.Add(label11);
            tabPage2.Controls.Add(label8);
            tabPage2.Controls.Add(comboBox4);
            tabPage2.Controls.Add(label9);
            tabPage2.Controls.Add(comboBox5);
            tabPage2.Controls.Add(label10);
            tabPage2.Controls.Add(comboBox6);
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(textBox3);
            tabPage2.Controls.Add(label7);
            tabPage2.Controls.Add(textBox4);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(858, 554);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Door";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkBox5
            // 
            checkBox5.AutoSize = true;
            checkBox5.Location = new Point(423, 27);
            checkBox5.Name = "checkBox5";
            checkBox5.Size = new Size(50, 19);
            checkBox5.TabIndex = 30;
            checkBox5.Text = "실내";
            checkBox5.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            checkBox4.AutoSize = true;
            checkBox4.Location = new Point(358, 27);
            checkBox4.Name = "checkBox4";
            checkBox4.Size = new Size(50, 19);
            checkBox4.TabIndex = 29;
            checkBox4.Text = "실외";
            checkBox4.UseVisualStyleBackColor = true;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(192, 270);
            label11.Name = "label11";
            label11.Size = new Size(87, 15);
            label11.TabIndex = 28;
            label11.Text = "기능 및 용도별";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(358, 270);
            label8.Name = "label8";
            label8.Size = new Size(55, 15);
            label8.TabIndex = 27;
            label8.Text = "개폐방식";
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(358, 292);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(153, 23);
            comboBox4.TabIndex = 26;
            comboBox4.SelectedIndexChanged += comboBox4_SelectedIndexChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(207, 165);
            label9.Name = "label9";
            label9.Size = new Size(0, 15);
            label9.TabIndex = 25;
            // 
            // comboBox5
            // 
            comboBox5.FormattingEnabled = true;
            comboBox5.Location = new Point(192, 292);
            comboBox5.Name = "comboBox5";
            comboBox5.Size = new Size(153, 23);
            comboBox5.TabIndex = 23;
            comboBox5.SelectedIndexChanged += comboBox5_SelectedIndexChanged;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(25, 270);
            label10.Name = "label10";
            label10.Size = new Size(55, 15);
            label10.TabIndex = 24;
            label10.Text = "문프레임";
            // 
            // comboBox6
            // 
            comboBox6.FormattingEnabled = true;
            comboBox6.Location = new Point(22, 292);
            comboBox6.Name = "comboBox6";
            comboBox6.Size = new Size(153, 23);
            comboBox6.TabIndex = 22;
            comboBox6.SelectedIndexChanged += comboBox6_SelectedIndexChanged;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(148, 64);
            label6.Name = "label6";
            label6.Size = new Size(16, 15);
            label6.TabIndex = 21;
            label6.Text = "H";
            // 
            // textBox3
            // 
            textBox3.Location = new Point(148, 82);
            textBox3.Name = "textBox3";
            textBox3.Size = new Size(100, 23);
            textBox3.TabIndex = 20;
            textBox3.Text = "0";
            textBox3.TextAlign = HorizontalAlignment.Center;
            textBox3.TextChanged += textBox3_TextChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(25, 64);
            label7.Name = "label7";
            label7.Size = new Size(18, 15);
            label7.TabIndex = 19;
            label7.Text = "W";
            // 
            // textBox4
            // 
            textBox4.Location = new Point(25, 82);
            textBox4.Name = "textBox4";
            textBox4.Size = new Size(100, 23);
            textBox4.TabIndex = 18;
            textBox4.Text = "0";
            textBox4.TextAlign = HorizontalAlignment.Center;
            textBox4.TextChanged += textBox4_TextChanged;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(dataGridView1);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(858, 554);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Utility";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Dock = DockStyle.Fill;
            dataGridView1.Location = new Point(0, 0);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.Size = new Size(858, 554);
            dataGridView1.TabIndex = 15;
            // 
            // button3
            // 
            button3.Location = new Point(872, 76);
            button3.Name = "button3";
            button3.Size = new Size(93, 23);
            button3.TabIndex = 31;
            button3.Text = "Create Door";
            button3.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Location = new Point(872, 132);
            button4.Name = "button4";
            button4.Size = new Size(93, 23);
            button4.TabIndex = 15;
            button4.Text = "Import DB";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // checkBox8
            // 
            checkBox8.AutoSize = true;
            checkBox8.Location = new Point(872, 23);
            checkBox8.Name = "checkBox8";
            checkBox8.Size = new Size(74, 19);
            checkBox8.TabIndex = 32;
            checkBox8.Text = "벽체생성";
            checkBox8.UseVisualStyleBackColor = true;
            //checkBox8.CheckedChanged += checkBox8_CheckedChanged;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(975, 582);
            Controls.Add(checkBox8);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(tabControl1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(button2);
            Name = "MainForm";
            Text = "PS 창호설계";
            FormClosing += MainForm_FormClosing;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private Button button2;
        private ComboBox comboBox1;
        private ComboBox comboBox2;
        private Label label1;
        private Label label2;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TabPage tabPage3;
        private Label label4;
        private TextBox textBox1;
        private Label label5;
        private TextBox textBox2;
        private Label label6;
        private TextBox textBox3;
        private Label label7;
        private TextBox textBox4;
        private Label label8;
        private ComboBox comboBox4;
        private Label label9;
        private ComboBox comboBox5;
        private Label label10;
        private ComboBox comboBox6;
        private DataGridView dataGridView1;
        private Label label11;
        private CheckBox checkBox5;
        private CheckBox checkBox4;
        private CheckBox checkBox6;
        private Label label13;
        private ComboBox comboBox7;
        private CheckBox checkBox7;
        private Label label3;
        private ComboBox comboBox3;
        private Label label14;
        private TextBox textBox5;
        private CheckBox checkBox9;
        private Button button3;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button button4;
        private Label label12;
        private CheckBox checkBox8;
    }
}