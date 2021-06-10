using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace PageTitleChangeDetector
{
    public partial class Form1 : Form
    {
        string CURRENT_URI = "";
        string CURRENT_STATUS = "idle";
        int CHECK_COUNT = 0;

        public Form1()
        {
            InitializeComponent();
        }

        public bool UpdateInitialTitle()
        {
            var title = this.GetCurrentTitle();
            if (title == "")
            {
                label7.Text = "Error: the page title could not be retrieved";
                label7.ForeColor = Color.Red;
                return false;
            }
            label7.ForeColor = SystemColors.ControlText;
            label7.Text = title;
            return true;
        }

        public bool VerifyTitleChanged()
        {
            // redownload page, check title changed or not
            var newTitle = this.GetCurrentTitle();
            var oldTitle = label7.Text;
            if (newTitle == "")
            {
                label9.Text = "Error: the page title could not be retrieved";
                label9.ForeColor = Color.Red;
                label9.Font = new Font(label9.Font, FontStyle.Regular);
                label9.Refresh();
                return false;
            }
            else if (newTitle == oldTitle) { 
                label9.ForeColor = SystemColors.ControlText;
                label9.Font = new Font(label9.Font, FontStyle.Regular);
                label9.Text = newTitle;
                label9.Refresh();
                return false;
            }
            else {
                // Changed!
                label9.ForeColor = SystemColors.Highlight;
                label9.Text = newTitle;
                label9.Font = new Font(label9.Font, FontStyle.Bold);
                label9.Refresh();
                return true;
            }
        }

        public string GetCurrentTitle()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            WebClient x = new WebClient();
            x.Encoding = Encoding.UTF8;

            // Get source & parse text
            try
            {
                x.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36");
                string source = x.DownloadString(CURRENT_URI);
                string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                    RegexOptions.IgnoreCase).Groups["Title"].Value;
                return title;
            }
            catch (WebException ex)
            {
                var resp = (HttpWebResponse)ex.Response;
                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound) // HTTP 404
                {
                    //the page was not found, continue with next in the for loop
                    return "404 - Not Found";
                }
                button2_Click(null, null);
                MessageBox.Show("Error: "+ex.Message);
                return "";
            }
            catch (Exception ex)
            {
                button2_Click(null, null);
                MessageBox.Show("Error: " + ex.Message);
                return "";
            }
        }

        public void UpdateStatus()
        {
            if (this.CURRENT_STATUS == "initial")
            {
                label5.Text = "Getting initial title...";
            }
            else if (this.CURRENT_STATUS == "timer")
            {
                label5.Text = "Waiting for next verification (#"+this.CHECK_COUNT+")...";
            }
            else 
            {
                label5.Text = "Idle - press start to begin";
            }

            label5.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Validate URI
            this.CHECK_COUNT = 1;
            var uriName = textBox1.Text;
            Uri uriResult;
            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                MessageBox.Show("The specified URL does not seem to be valid.");
                return;
            }
            this.CURRENT_URI = uriName;

            // Update status
            CURRENT_STATUS = "initial";
            this.UpdateStatus();

            // Try updating the initial title
            label9.Text = "";
            if (!UpdateInitialTitle()) return;

            // Switch button state
            button1.Enabled = false;
            button2.Enabled = true;
            textBox1.Enabled = false;
            numericUpDown1.Enabled = false;

            // Start the timers
            timer1.Interval = Convert.ToInt32(numericUpDown1.Value) * 1000;
            timer1.Start(); // Start the timer that will perform the check

            // Update status
            CURRENT_STATUS = "timer";
            this.UpdateStatus();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Switch button state
            button1.Enabled = true;
            button2.Enabled = false;
            textBox1.Enabled = true;
            numericUpDown1.Enabled = true;

            // Stop timer
            timer1.Stop();

            // Update status
            CURRENT_STATUS = "idle";
            this.UpdateStatus();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.VerifyTitleChanged())
            {
                // Changed! press Stop, and then alert
                button2_Click(sender, e);

                // BeEp
                System.Media.SystemSounds.Beep.Play();
                System.Media.SystemSounds.Beep.Play();
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("The page title has changed ("+ DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")+")\n\nThe new title is:\n"+label9.Text);
            }
            else
            {
                // No change
                this.CHECK_COUNT++;
                this.UpdateStatus();
            }

        }
    }
}
