using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.ListBox;

namespace BillionHelp
{
    public partial class Form1 : Form
    {
        List<string> gameInstances = new List<string>();
        public static Dictionary<string, List<string>> GameBackups = new Dictionary<string, List<string>>();
        public static string GamePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/My Games/They Are Billions";
        DirectoryInfo configDir = new DirectoryInfo(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/yiwanfuzhu");
        FileInfo configFile = new FileInfo(System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/yiwanfuzhu" + "/config.json");
        List<string> backupFileExtensions = new List<string> { ".zxcampaign", ".zxcheck", ".zxsav", "_Backup.zxcheck", "_Backup.zxsav", "~.zxcampaign", "~.zxcheck" };
        int IntervalTime = 2;
        Config config;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "开始定时备份")
            {
                timer1.Start();
                button1.Text = "停止定时备份";
            }
            else
            {
                timer1.Stop();
                button1.Text = "开始定时备份";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 加载配置文件
           
            if (!configDir.Exists)
            {
                configDir.Create();
            }
            if (!configFile.Exists)
            {
                config = new Config() { GamePath = GamePath, IntervalTime = IntervalTime };
                File.WriteAllText(configFile.FullName, JsonConvert.SerializeObject(config));
            }
            else
            {
                var configStr = File.ReadAllText(configFile.FullName);
                config = JsonConvert.DeserializeObject<Config>(configStr);
                // 结构被破坏
                if (config == null || config.GamePath == null || config.IntervalTime == 0)
                {
                    // 重新覆盖
                    config = new Config() { GamePath = GamePath, IntervalTime = IntervalTime };
                    File.WriteAllText(configFile.FullName, JsonConvert.SerializeObject(config));
                }
                GamePath = config.GamePath;
                IntervalTime = config.IntervalTime;
            }
            var valid = true;
            DirectoryInfo dir = new DirectoryInfo(GamePath);
            if (!dir.Exists)
            {
                MessageBox.Show("未找到亿万僵尸游戏目录，请手动指定。");

                valid = false;
            }

            DirectoryInfo dir2 = new DirectoryInfo(GamePath + "/Saves");
            if (!dir2.Exists)
            {
                MessageBox.Show("请检查目录是否正确，如果确认正确,请更新辅助程序版本。");

                valid = false;
            }
            if (valid)
            {
                loadBackup(dir2);
            }
            button1.Text = "开始定时备份";
            // 设置定时
            timer1.Interval = 60 * IntervalTime * 1000;
            timer1.Stop();
            // 显示定时时间
            textBox1.Text = IntervalTime.ToString();
            label1.Text = GamePath;
        }

        private void loadBackup(DirectoryInfo dir2)
        {
            gameInstances = new List<string>();
            var fileInfos = dir2.GetFiles();
            foreach (var item in fileInfos)
            {
                if (item.Extension == ".zxsav")
                {
                    if (!gameInstances.Contains(item.Name) && !item.Name.Contains("_Backup"))
                    {
                        gameInstances.Add(item.Name.Replace(".zxsav", ""));
                    }
                }
            }

            ObjectCollection objectCollection = new ObjectCollection(listBox1);
            objectCollection.Clear();
            objectCollection.AddRange(gameInstances.ToArray());

            foreach (var item in gameInstances)
            {
                DirectoryInfo currenGameInstanceBackupDir = new DirectoryInfo(GamePath + "/Saves/back/" + item);
                if (!currenGameInstanceBackupDir.Exists)
                {
                    currenGameInstanceBackupDir.Create();
                }

                var currenGameInstanceBackupDirChildDirs = currenGameInstanceBackupDir.GetDirectories();
                var records = new List<string>();
                foreach (var currenGameInstanceBackupDirChildDir in currenGameInstanceBackupDirChildDirs)
                {
                    records.Add(currenGameInstanceBackupDirChildDir.Name);
                }
                GameBackups[item] = records;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBox1.IndexFromPoint(e.Location);
            var item = gameInstances[index];
            Detail detail = new Detail();
            detail.gameInstance = item;
            detail.ShowDialog();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            backUp();
        }

        private void backUp()
        {
            DirectoryInfo dir = new DirectoryInfo(GamePath + "/Saves");

            if (!dir.Exists)
            {
                MessageBox.Show("请检查目录是否正确，如果确认正确,请更新辅助程序版本。");
            }
            else
            {
                var files = dir.GetFiles();

                DirectoryInfo backupDir = new DirectoryInfo(GamePath + "/Saves/back");
                if (!backupDir.Exists)
                {
                    backupDir.Create();
                }
                foreach (var item in gameInstances)
                {
                    if (!GameBackups.TryGetValue(item, out List<string> backupRecord))
                    {
                        backupRecord = new List<string>();
                    }

                    DirectoryInfo currenGameInstanceBackupDir = new DirectoryInfo(GamePath + "/Saves/back/" + item);
                    if (!currenGameInstanceBackupDir.Exists)
                    {
                        currenGameInstanceBackupDir.Create();
                    }
                    List<FileInfo> backupFiles = new List<FileInfo>();
                    var maxTime = DateTime.MinValue;
                    foreach (var file in files)
                    {
                        foreach (var backupFileExtension in backupFileExtensions)
                        {
                            if (file.Name == item + backupFileExtension)
                            {
                                backupFiles.Add(file);
                                if (file.LastWriteTime > maxTime)
                                {
                                    maxTime = file.LastWriteTime;
                                }
                            }
                        }
                    }
                    var record = maxTime.ToString("yyyy-MM-dd HH_mm_ss");

                    if (backupRecord.Contains(record))
                    {
                        return;
                    }

                    DirectoryInfo currentBackupDir = new DirectoryInfo(GamePath + "/Saves/back/" + item + "/" + record);
                    if (!currentBackupDir.Exists)
                    {
                        currentBackupDir.Create();
                    }
                    foreach (var backupFile in backupFiles)
                    {
                        backupFile.CopyTo(currentBackupDir.FullName + "/" + backupFile.Name, true);
                    }

                    // 更新这条记录
                    backupRecord.Add(record);
                    GameBackups[item] = backupRecord;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backUp();
            MessageBox.Show("备份成功!");
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8)
                e.Handled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dir = GetSelectDirectory();
            DirectoryInfo saveDir = new DirectoryInfo(dir + "/Saves");
            if (saveDir.Exists)
            {
                config.GamePath = dir;
                File.WriteAllText(configFile.FullName, JsonConvert.SerializeObject(config));
                GamePath = dir;
                label1.Text = dir;
                // 重新加载备份文件
                loadBackup(saveDir);
                MessageBox.Show("更新游戏路径成功。");
            }
            else
            {
                MessageBox.Show("你似乎选择了错误的目录。");
            }
        }

        /// <summary>
        /// 获取选择的目录
        /// </summary>
        /// <returns>返回选择的目录</returns>
        private string GetSelectDirectory()
        {
            FolderBrowserDialog dir = new FolderBrowserDialog();
            dir.ShowDialog();
            return dir.SelectedPath;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            var interval = Convert.ToInt32(textBox1.Text);
            config.IntervalTime = interval;
            File.WriteAllText(configFile.FullName, JsonConvert.SerializeObject(config));
            IntervalTime = interval;
            timer1.Interval = IntervalTime;
            if (timer1.Enabled)
            {
                timer1.Stop();
                timer1.Start();
            }
            MessageBox.Show("修改定时备份成功");
        }
    }
}
