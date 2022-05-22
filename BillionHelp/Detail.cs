﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.ListBox;

namespace BillionHelp
{
    public partial class Detail : Form
    {
        public string gameInstance;
        public List<string> backUps = new List<string>();

        public Detail()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Fresh();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult d = MessageBox.Show("确定要回滚当前信息吗 ?", "确定");
            int index = listBox1.IndexFromPoint(e.Location);
            var record = backUps[index];
            if (d == DialogResult.OK)
            {
                DirectoryInfo currentBackupDir = new DirectoryInfo( Form1.GamePath + "/Saves/back/" + gameInstance + "/" + record);
                var fileInfos = currentBackupDir.GetFiles();
                foreach (var fileinfo in fileInfos)
                {
                    fileinfo.CopyTo(Form1.GamePath + "/Saves" + "/" + fileinfo.Name, true);
                }
            }
            MessageBox.Show("已经回滚到"+record+"该时间点!");
        }

        private void Detail_Load(object sender, EventArgs e)
        {
            Fresh();
            this.label1.Text = "当前为" + gameInstance + "记录档案";
        }

        private void Fresh()
        {
            Form1.GameBackups.TryGetValue(gameInstance, out backUps);
            if (backUps != null)
            {
                ObjectCollection objectCollection = new ObjectCollection(listBox1);
                objectCollection.Clear();
                objectCollection.AddRange(backUps.ToArray());
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
