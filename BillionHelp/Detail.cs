using System;
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
            DialogResult d = MessageBox.Show("确定要回滚当前信息吗 ?", "确定", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            int index = listBox1.IndexFromPoint(e.Location);
            if (index<0)
            {
                return;
            }
            var record = backUps[index];
            if (d == DialogResult.Yes)
            {
                DirectoryInfo currentBackupDir = new DirectoryInfo( Form1.GamePath + "/Saves/back/" + gameInstance + "/" + record);
                var fileInfos = currentBackupDir.GetFiles();
                foreach (var fileinfo in fileInfos)
                {
                    fileinfo.CopyTo(Form1.GamePath + "/Saves" + "/" + fileinfo.Name, true);
                }
                MessageBox.Show("已经回滚到" + record + "该时间点!");
            }
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
                //ObjectCollection objectCollection = new ObjectCollection(listBox1);
                //objectCollection.Clear();
                //objectCollection.AddRange(backUps.ToArray());
                listBox1.Items.Clear();
                foreach (var item in backUps)
                {
                    listBox1.Items.Add(item);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void removeRecord(string gameInstance, string time)
        {
            var path = BillionHelp.Form1.GamePath+"/Saves/back" + "/" + gameInstance + "/" + time;
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //this.listBox1.SelectedItems.Clear();

            var data = listBox1.SelectedItems;
            foreach (var item in listBox1.SelectedItems)
            {
                removeRecord(gameInstance,(string)item);
            }
            Form1.FreshInstanceGame(gameInstance);
            Fresh();
        }
    }
}
