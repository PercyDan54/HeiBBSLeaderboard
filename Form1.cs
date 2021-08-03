using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;

namespace HeiBBSLeaderboard
{
    public partial class Form1 : Form
    {
        private string jsonText = string.Empty;
        private string csvText = string.Empty;
        private string plainText = string.Empty;
        private int startInt;
        private int endInt;
        private LeaderBoardResponse leaderBoard;

        public Form1()
        {
            InitializeComponent();

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }

            startInt = (int)numericUpDown1.Value;
            endInt = (int)numericUpDown2.Value;
            button1.Click += button1_Click;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox1.SelectedIndex = 0;
            richTextBox1.Visible = false;
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = true;
            chart1.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            dataGridView1.ColumnHeaderMouseClick += dataGridView1_ColumnSortModeChanged;
        }

        private void dataGridView1_ColumnSortModeChanged(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex <= 1 || leaderBoard == null) return;

            var series = new Series();

            for (var i = startInt; i < endInt; i++)
            {
                var index = i - startInt;
                var user = leaderBoard.Users[i.ToString()].ToObject<User>();
                var newPoint = new DataPoint();
                ++index;

                switch (e.ColumnIndex)
                {
                    case 2:
                        newPoint = new DataPoint(index, user.UID);
                        break;

                    case 3:
                        newPoint = new DataPoint(index, user.Credit);
                        break;

                    case 4:
                        newPoint = new DataPoint(index, user.LevelID);
                        break;

                    case 5:
                        newPoint = new DataPoint(index, int.Parse(dataGridView1.Rows[index - 1].Cells[5].Value.ToString()));
                        break;
                }

                series.Points.Add(newPoint);
                chart1.Series.Clear();
                chart1.Series.Add(series);
                chart1.ChartAreas[0].RecalculateAxesScale();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var index = comboBox1.SelectedIndex;

            switch (index)
            {
                case 1:
                    richTextBox1.Text = plainText;
                    break;

                case 2:
                    richTextBox1.Text = jsonText;
                    break;

                case 3:
                    var text = JsonConvert.SerializeObject(leaderBoard, Formatting.Indented);
                    richTextBox1.Text = text == "null" ? string.Empty : text;
                    break;

                case 4:
                    richTextBox1.Text = csvText;
                    break;
            }

            richTextBox1.Visible = index > 0;
            chart1.Visible = !richTextBox1.Visible;
        }

        private void updateData()
        {
            var series = new Series();
            chart1.Series.Clear();
            dataGridView1.Rows.Clear();
            chart1.ChartAreas[0].RecalculateAxesScale();

            plainText = string.Empty;
            csvText = "排名,用户名,UID,积分,等级,平均每日发贴数\n";

            for (var i = startInt; i < endInt; i++)
            {
                var user = leaderBoard.Users[i.ToString()].ToObject<User>();
                var name = user.Name;
                var level = user.Level;
                var credit = user.Credit;
                var uid = user.UID;
                plainText += $"第{i}名：{user}\n";

                var index = i - startInt;
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[index].Cells[0].Value = i;
                dataGridView1.Rows[index].Cells[1].Value = name;
                dataGridView1.Rows[index].Cells[2].Value = uid;
                dataGridView1.Rows[index].Cells[3].Value = credit;
                dataGridView1.Rows[index].Cells[4].Value = level;
                var point = new DataPoint(index + 1, credit);
                series.Points.Add(point);

                if (checkBox1.Checked)
                {
                    var dailyPost = getDailyPost(uid);
                    dataGridView1.Rows[index].Cells[5].Value = dailyPost;
                    csvText += $"{i},{name},{uid},{credit},{level},{dailyPost}\n";
                }
                else
                    csvText += $"{i},{name},{uid},{credit},{level}\n";
            }

            chart1.Series.Add(series);
        }

        private int getDailyPost(int uid)
        {
            var document = SoftCircuits.HtmlMonkey.HtmlDocument.FromHtml(GetResponse($"http://www.heibbs.net/?{uid}"));

            var node = document.Find($"a[href=\"https://www.heibbs.net/home.php?mod=space&uid={uid}&do=thread&view=me&type=reply&from=space\"]");
            //发帖数
            double count = int.Parse(node.Single().Text.Remove(0, 4));
            node = document.Find($"a[href=\"https://www.heibbs.net/home.php?mod=space&uid={uid}&do=thread&view=me&type=thread&from=space\"]");
            //主题数
            count += int.Parse(node.Single().Text.Remove(0, 4));
            var regDate = DateTime.Parse(document.Find(n => n.Text == "注册时间").First().NextNode?.Text);
            var timespan = DateTime.Now.Date.Subtract(regDate).Days;

            return (int)Math.Round(count / timespan);
        }

        private void button1_Click(object sender, EventArgs args)
        {
            button1.Text = "获取中";
            button1.Enabled = false;
            startInt = (int)numericUpDown1.Value;
            endInt = (int)numericUpDown2.Value;

            numericUpDown2.Value = endInt = Math.Max(startInt, endInt);

            ++endInt;
            getLeaderBoard:

            try
            {
                jsonText = GetResponse($"http://www.heibbs.net/api/get_credits.php?start={startInt}&end={endInt}");
                leaderBoard = JsonConvert.DeserializeObject<LeaderBoardResponse>(jsonText);

                updateData();

                //手动更新文本框内容
                var selectedIndex = comboBox1.SelectedIndex;

                comboBox1.SelectedIndex = 0;
                comboBox1.SelectedIndex = selectedIndex;
            }
            catch (Exception e)
            {
                var result = MessageBox.Show($"{e.Message}", "错误", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                switch (result)
                {
                    case DialogResult.Retry:
                        goto getLeaderBoard;

                    default:
                        goto End;
                }
            }

            End:
            button1.Enabled = true;
            button1.Text = "获取";
        }

        public string GetResponse(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);

            var task = req.GetResponseAsync();
            var response = (HttpWebResponse)task.Result;
            var responseStream = new StreamReader(response.GetResponseStream() ?? throw new NullReferenceException(), Encoding.UTF8);
            var resultString = responseStream.ReadToEnd();

            responseStream.Close();
            response.Close();
            return resultString;
        }
    }
}
