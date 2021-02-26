using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;

namespace GetScore_GUI
{
    public partial class Form1 : Form
    {
        private string jsonText = string.Empty;
        private string csvText = string.Empty;
        private string plainText = string.Empty;
        private int startInt;
        private int endInt;
        private LeaderBoardResponse json;

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
            if (e.ColumnIndex <= 1) return;
            var series = new Series();
            for (var i = startInt; i < endInt; i++)
            {
                var idx = i - startInt;
                var user = json.Users[i.ToString()].ToObject<User>();
                var newPoint = new DataPoint();
                switch (e.ColumnIndex)
                {
                    case 2:
                        newPoint = new DataPoint(idx + 1, user.UID);
                        break;
                    case 3:
                        newPoint = new DataPoint(idx + 1, user.Credit);
                        break;
                    case 4:
                        newPoint = new DataPoint(idx + 1, user.LevelID);
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
            richTextBox1.Visible = true;
            chart1.Visible = false;
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    richTextBox1.Visible = false;
                    chart1.Visible = true;
                    break;
                case 1:
                    richTextBox1.Text = plainText;
                    break;
                case 2:
                    richTextBox1.Text = jsonText;
                    break;
                case 3:
                    richTextBox1.Visible = true;
                    var serializer = new JsonSerializer();
                    TextReader tr = new StringReader(jsonText);
                    var jtr = new JsonTextReader(tr);
                    var textWriter = new StringWriter();
                    var jsonWriter = new JsonTextWriter(textWriter) 
                    { 
                        Formatting = Formatting.Indented,
                        Indentation = 4,
                        IndentChar = ' '
                    };
                    serializer.Serialize(jsonWriter, serializer.Deserialize(jtr));
                    richTextBox1.Text = textWriter.ToString() == "null" ? string.Empty : textWriter.ToString();
                    Console.WriteLine(JsonConvert.SerializeObject(jsonText));
                    break;
                case 4:
                    richTextBox1.Text = csvText;
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs args)
        {
            
            button1.Text = "获取中";
            button1.Enabled = false;
            startInt = (int)numericUpDown1.Value;           
            endInt = (int)numericUpDown2.Value;
            //结束排名必须>=开始排名
            if(endInt < startInt)
            {
                numericUpDown2.Value = startInt;
                endInt = startInt;
            }
            endInt++;
            getLeaderBoard:
            try
            {
                jsonText = GetResponse($"http://www.heibbs.net/api/get_credits.php?start={startInt}&end={endInt}");
            }
            catch(Exception e)
            {
                var result = MessageBox.Show($@"{e.Message}", @"错误", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);
                switch (result)
                {
                    case DialogResult.Retry:
                        goto getLeaderBoard;
                    default:
                        goto End;
                }
            }
            json = JsonConvert.DeserializeObject<LeaderBoardResponse>(jsonText);
            //将排名添加到表格
            var series = new Series();
            chart1.Series.Clear();
            plainText = string.Empty;
            csvText = "排名,用户名,UID,积分,等级\n";
            dataGridView1.Rows.Clear();
            chart1.ChartAreas[0].RecalculateAxesScale();
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
            for (var i = startInt; i < endInt; i++)
            {
                var idx = i - startInt;
                var user = json.Users[i.ToString()].ToObject<User>();
                var name = user.Name;
                var level = user.Level;
                var credit = user.Credit;
                var uid = user.UID;
                plainText += $"第{i}名：{user}\n";
                csvText += $"{i},{name},{uid},{credit},{level}\n";
                dataGridView1.Rows.Add(1);
                dataGridView1.Rows[idx].Cells[0].Value = i;
                dataGridView1.Rows[idx].Cells[1].Value = name;
                dataGridView1.Rows[idx].Cells[2].Value = uid;
                dataGridView1.Rows[idx].Cells[3].Value = credit;
                dataGridView1.Rows[idx].Cells[4].Value = level;              
                var point = new DataPoint(idx + 1, credit); 
                series.Points.Add(point);
            }
            //手动更新文本框内容
            var comboidx = comboBox1.SelectedIndex;
            comboBox1.SelectedIndex = 0;
            comboBox1.SelectedIndex = comboidx;         
            chart1.Series.Add(series);
            End:
            button1.Enabled = true;
            button1.Text = "获取";
        }

        public string GetResponse(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";

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
