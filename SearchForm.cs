using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuestStudio
{
    public partial class SearchForm : Form
    {
        TreeForm parentWindow = null;

        public SearchForm(TreeForm parent)
        {
            InitializeComponent();
            parentWindow = parent;
        }

        public TreeNode foundNode = null;
        private void button1_Click(object sender, EventArgs e)
        {
            foundNode = parentWindow.FindNode(2, textBox1.Text, exact.Checked, expand.Checked);
            this.Close();
        }
    }
}
