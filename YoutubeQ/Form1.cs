using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeQ
{
    public partial class Form1 : Form
    {
        bool playing = false;
        public Form1()
        {
            InitializeComponent();
        }

        public void nowPlaying()
        {
            playing = true;
            chromiumWebBrowser1.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            button6.Visible = false;
            button7.Visible = false;
            listView1.Visible = false;
            button1.Visible = false;    
            button2.Visible = false;    
            button3.Visible = false;
            checkBox1.Visible = false;  
        }
        public void stopPlaying()
        {
            timer1.Stop();
            button6.Visible = true;
            button7.Visible = true;
            button4.Visible = false;
            button5.Visible = false;
            listView1.Visible = true;
            button1.Visible = true;
            button2.Visible = true;
            button3.Visible = true;
            checkBox1.Visible = true;
            playing = false;
            chromiumWebBrowser1.Load("about:blank");
            chromiumWebBrowser1.Visible = false;
        }
        

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string link = (string)e.Data.GetData(DataFormats.Text);                
                addToListView(link);
            }
            catch (Exception) { throw; }
           
                
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text) && (e.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            writeLinks();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                listView1.Items.Remove(listView1.SelectedItems[0]);
            }
            catch (Exception){}
            writeLinks();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            writeLinks();
            if (listView1.Items.Count > 0)
            {
                nowPlaying();
                YoutubeLink link = (YoutubeLink)listView1.Items[0].Tag;
                timer1.Interval = link.getDuration();
                timer1.Start();
                chromiumWebBrowser1.Load(link.getUrl());                
            }           
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.Size = new Size(this.Size.Width-1,this.Size.Height-30);
            if (playing)
            {
                button4.Visible = true;
                button5.Visible = true;
            }
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            this.Size = new Size(this.Size.Width+1, this.Size.Height+30);
            this.FormBorderStyle = FormBorderStyle.None;
            if (playing)
            {
                button4.Visible = false;
                button5.Visible = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                TopMost = true;
            }
            else
            {
                TopMost = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            writeLinks();
            stopPlaying();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            playNext();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            readAllLink();
        }

        private void writeLinks()
        {
            string links ="";
            foreach (ListViewItem item in listView1.Items)
            {
                YoutubeLink link = (YoutubeLink)item.Tag;
                links += link.getUrl() + "\n";  
            }
            System.IO.File.WriteAllText("videolist.txt", links);
        }
        private void readAllLink()
        {
            if (System.IO.File.Exists("videolist.txt"))
            {
                string[] list = System.IO.File.ReadAllLines("videolist.txt");
                foreach (string item in list)
                {
                    addToListView(item);
                }
            }
        }
        private void addToListView(string url)
        {
            YoutubeLink video = new YoutubeLink(url);
            ListViewItem list = new ListViewItem();
            list.Text = video.getTitle();
            list.Tag = video;
            listView1.Items.Add(list);
            listView1.Update();
            listView1.Refresh();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                ListViewItem select = listView1.SelectedItems[0];
                int index = listView1.Items.IndexOf(select);
                listView1.Items.Remove(select);
                listView1.Items.Insert(index-1, select);
                listView1.Update();
                listView1.Refresh();
                writeLinks();
            }
            catch (Exception) {}
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                ListViewItem select = listView1.SelectedItems[0];
                int index = listView1.Items.IndexOf(select);
                listView1.Items.Remove(select);
                listView1.Items.Insert(index + 1, select);
                listView1.Update();
                listView1.Refresh();
                writeLinks();
            }
            catch (Exception) { }
        }

        private void playNext()
        {
            if (listView1.Items.Count > 0)
            {
                listView1.Items.RemoveAt(0);
                if (listView1.Items.Count > 0)
                {
                    YoutubeLink link = (YoutubeLink)listView1.Items[0].Tag;
                    timer1.Interval = link.getDuration();
                    chromiumWebBrowser1.Load(link.getUrl());
                }
                else
                {
                    stopPlaying();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            playNext();
            
        }

        private void chromiumWebBrowser1_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            SendKeys.Send("{ENTER}");
        }
    }


    public class YoutubeLink
    {
        private string url;
        private string title;
        private int duration;
        public YoutubeLink(string url)
        {
            this.url = convertLink(url);
            title = generateTitle(this.url);
        }
        public string generateTitle(string link)
        {
            string data;
            Regex regx = new Regex("runs(.*?)defaultThumbnail", RegexOptions.IgnoreCase);
            using (WebClient web1 = new WebClient()) {
              
                data = web1.DownloadString(link);
            }
            //getDuration(data);
            data = regx.Match(data).Value;
            data = data.Split('\"')[4].Replace('\\',' ');
            return data;     
        }
        private void getDuration(string data )
        {           
            Regex regx = new Regex("videoDurationSeconds(.*?)webPlayerActionsPorting", RegexOptions.IgnoreCase);
            string temp = regx.Match(data).Value;
            temp = temp.Split('\"')[2].Replace('\\', ' ');
            int seconds;
            int.TryParse(temp, out seconds);
            duration = seconds*1000;
        }
        public string convertLink(string link)
        {
            return link.Replace("watch?v=", "embed/") + "?autoplay=1";
           //     link = link.Replace("watch?v=", "embed/");
        }
        public string getUrl()
        {
            return url;
        }
        public string getTitle()
        {
            return title;
        }
        public int  getDuration()
        {
            return duration;
        }
    }

}
