using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YoutubeQ
{
    public partial class Form1 : Form
    {
        bool playing = false;
        bool autoPlay = true;
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
            listView1.Visible = false;
            button1.Visible = false;    
            button2.Visible = false;    
            button3.Visible = false;
            checkBox1.Visible = false;  
        }
        public void stopPlaying()
        {
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

        public string convertLink(string link)
        {
            if (autoPlay)
            {
                link = link.Replace("watch?v=", "embed/") + "?autoplay=1";
            }
            else
            {
                link = link.Replace("watch?v=", "embed/");
                Clipboard.SetText(link);
            }
            return link;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                listView1.Items.Add(convertLink((string)e.Data.GetData(DataFormats.Text)));
            }
            catch (Exception){}
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text) && (e.AllowedEffect & DragDropEffects.Copy) != 0)
            {
                // Allow this.
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                // Don't allow any other drop.
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
                chromiumWebBrowser1.Load(listView1.Items[0].Text);                
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
            if (listView1.Items.Count > 0)
            {
                listView1.Items.RemoveAt(0);
                if (listView1.Items.Count > 0)
                {
                    chromiumWebBrowser1.Load(listView1.Items[0].Text);
                }
                else
                {
                    stopPlaying();
                }
            }
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
                links += item.Text + "\n";  
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
                    listView1.Items.Add(item);
                }
            }
        }
    }
}
