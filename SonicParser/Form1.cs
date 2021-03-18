using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SonicParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

            OD1.ShowDialog();
            string path=OD1.FileName;

            string[] lines = File.ReadAllLines(path);

            SonicWall sonicWall = new SonicWall(lines,LB);



        }
    }
}
