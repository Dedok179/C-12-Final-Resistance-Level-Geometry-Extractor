using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using C12_Lib;

namespace C_12_Extracting_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool mwdLoaded = false;
        private byte[] mwdBuffer;
        private byte[] exeBuffer;
        private string filepath = "";             

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openEXE = new OpenFileDialog() { Filter = "SCUS_946.66|SCUS_946.66" };
            OpenFileDialog openMWD = new OpenFileDialog() { Filter = "PROJFILE.MWD|PROJFILE.MWD" };
            
            if (mwdLoaded != true)
            {
                button4.Text = "Open";

                if (openEXE.ShowDialog() == DialogResult.OK)
                {
                    if (openMWD.ShowDialog() == DialogResult.OK)
                    {
                        exeBuffer = File.ReadAllBytes(openEXE.FileName);
                        mwdBuffer = File.ReadAllBytes(openMWD.FileName);
                        filepath = openMWD.FileName;

                        mwdLoaded = true;

                        button4.Text = "Export Data";
                    }
                }                               
            }
            else
            {
                
                try
                {
                    C12.ExtractLevelByID(exeBuffer, mwdBuffer, (int)numericLevelID.Value, (int)numericSectionID.Value, filepath);
                }
                catch
                {
                    MessageBox.Show("Error during data reading!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
