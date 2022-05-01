using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BTCAddressTask
{
    public partial class Form1 : Form
    {
        private int StopAt = 0;
        private bool Stopflag = false;
        private List<string> AllLines = new List<string>();
        private List<string> BTCAddress = new List<string>();
        private List<Output> outputs = new List<Output>();
        private List<Output> SortedOutputs = new List<Output>();
        private string FullPath = string.Empty;

        public Form1()
        {
            InitializeComponent();
            output.Enabled = false;
            stop.Enabled = false;
            restart.Enabled = false;
            label5.Text = "";
            progressBar1.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select file to be upload.";
            openFileDialog.Filter = "Select Text files(*.txt)| *.txt";
            openFileDialog.FilterIndex = 1;
            try
            {
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog.CheckFileExists)
                    {
                        FullPath = System.IO.Path.GetFullPath(openFileDialog.FileName);
                        path.Text = FullPath;
                        button1.Enabled = false;
                        output.Enabled = true;
                    }
                }
                else
                {
                    MessageBox.Show("Please Upload File.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void output_Click(object sender, EventArgs e)
        {
            button1.Enabled = true;
            output.Enabled = false;
            button1.Enabled = false;
            stop.Enabled = true;
            progressBar1.Enabled = true;
            label5.Text = "Processing Started";
            var result = await ReadFileAsync(FullPath);
            if (result == 1)
            {
                await RequestURLAsync();
                SortedOutputs = (from item in outputs
                                 orderby item.BTCAddress.Length descending
                                 select item).ToList();
                WriteOutputFile(SortedOutputs);
                button1.Enabled = true;
                stop.Enabled = false;
                restart.Enabled = false;
                label5.Text = "Processing Completed";
                MessageBox.Show("Processing is done, Output File Generated");
            }
        }
        private async void stop_ClickAsync(object sender, EventArgs e)
        {
            Stopflag = true;
            restart.Enabled = true;
            stop.Enabled = false;
            await ReadFileAsync(FullPath);
        }
        private async void restart_Click(object sender, EventArgs e)
        {
            stop.Enabled = true;
            restart.Enabled = false;
            Stopflag = false;
            StopAt = 0;
            progressBar1.Value = 0;
            label3.Text = "0";
            await ReadFileAsync(FullPath);
        }

        private async Task<int> ReadFileAsync(string path)
        {
            string BtcPattern = @"^[13][a-km-zA-HJ-NP-Z1-9]{25,34}";
            Regex regex = new Regex(BtcPattern);
            if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Invalid File Path.");
            }
            int reset = 0;
            using (StreamReader reader = new StreamReader(path))
            {
                while (true)
                {
                    if (Stopflag == true)
                    {
                        break;
                    }
                    if (reset++ == StopAt || StopAt == 0)
                    {
                        StopAt++;
                        label3.Text = StopAt.ToString();
                        string line = await reader.ReadLineAsync();
                        if (line != null)
                        {
                            foreach (string word in line.Split(' '))
                            {
                                if (regex.IsMatch(word))
                                {
                                    var match = regex.Match(word);
                                    if (!BTCAddress.Contains(match.Value))
                                    {
                                        BTCAddress.Add(match.Value);
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            if (Stopflag == false)
            {
                return 1;
            }
            return 0;
        }
        private async Task GetBalanceAsync(string btcAddress)
        {
            var page = "https://api.blockcypher.com/v1/btc/main/addrs/" + btcAddress + "";
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(page))
                if (response.IsSuccessStatusCode)
                {
                    using (HttpContent content = response.Content)
                    {
                        string result = await content.ReadAsStringAsync();
                        ResponseModel resp = JsonConvert.DeserializeObject<ResponseModel>(result);
                        if (resp.final_balance > 0)
                        {
                            outputs.Add(new Output() { BTCAddress = resp.address, FinalBalance = resp.final_balance.ToString() });
                        }
                    }
                }
        }

        public async Task RequestURLAsync()
        {
            BTCAddress = BTCAddress.Select(p => p).Distinct().ToList();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = BTCAddress.Count;
            foreach (var btcAddress in BTCAddress)
            {
                try
                {
                    await GetBalanceAsync(btcAddress);
                    progressBar1.Value = progressBar1.Value + 1;
                }
                catch (HttpRequestException)
                {
                    continue;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        public async void WriteOutputFile(List<Output> outputs)
        {
            TextWriter tw = new StreamWriter("OutPutList.txt");
            foreach (var item in outputs)
                await tw.WriteLineAsync(item.BTCAddress +" -> "+item.FinalBalance);
            tw.Close();
        }

        
    }
}
