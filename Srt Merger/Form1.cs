using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace Srt_Merger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        List<SrtItem> list = new List<SrtItem>();

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            // if any file has inappropriate extension (not .srt), show red color and leave
            // else, show green color. 
            // Also change the hint message to alert user of in-appropriate file extension.
            string extension = string.Empty;
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            extension = Path.GetExtension(paths[0]).ToLower();
                
                if (extension == ".srt")
                {
                    e.Effect = DragDropEffects.Copy;
                }
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                // Get pathes of the dropped files.
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                string extension = Path.GetExtension(paths[0]).ToLower();
                if (extension == ".srt")
                {
                    listBox1.Items.Add(paths[0]);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count < 2)
            {
                MessageBox.Show("Add More Files");
                return;
            }
            //
            List<string> texts = new List<string>();
            List<SrtItem> srts = new List<SrtItem>();

            //load files
            foreach (string item in listBox1.Items)
            {
                
                string[] fileLines= File.ReadAllLines(item,Encoding.UTF8);

                //initialize states
                int state = 0;
                SrtItem srti = null;

                //going through states
                foreach (string line in fileLines)
                {
                    switch (state)
                    {
                        case 0:
                            if (line == "") //empty
                            {
                                //stay in state 0
                                
                            }
                            else if(Regex.IsMatch(line , "^\\d+$")) //Numebr
                            {
                                state = 1;
                                //new srt item
                                srti = new SrtItem();
                            }
                            break;
                        case 1:
                            if (line.Contains(" --> "))
                            {
                                //append to srt text 
                                //srti.textLines.Add(line); //now this line will be added by srtItem itself
                                //extract the time and assign it to the srt item
                                
                                string[] time_start = line.Substring(0, 12).Split(':', ',');
                                string[] time_end = line.Substring(17, 12).Split(':', ',');

                                srti.startTime = new DateTime( 2000, 1, 1,
                                    Convert.ToInt16(time_start[0]),
                                    Convert.ToInt16(time_start[1]),
                                    Convert.ToInt16(time_start[2]),
                                    Convert.ToInt16(time_start[3]) );

                                srti.endTime = new DateTime(2000, 1, 1,
                                    Convert.ToInt16(time_end[0]),
                                    Convert.ToInt16(time_end[1]),
                                    Convert.ToInt16(time_end[2]),
                                    Convert.ToInt16(time_end[3]));
                                //state update
                                state = 2;
                            }
                            break;
                        case 2:
                            //whatever you find, just append it to the srt text
                            //and move to state 3
                            srti.textLines.Add(line);
                            state = 3;
                            break;
                        case 3:
                            if (line == "") //empty
                            {
                                state = 0;
                                //add the srt object to the list
                                if (srti != null)
                                {
                                    srts.Add(srti);
                                    srti = null;
                                }
                            }
                            else
                            {
                                //append to the srt lines and stay in state 3
                                srti.textLines.Add(line);
                            }
                            break;
                        default:
                            state = 0;
                            break;
                    }
                } //end looping on a file lines
            } //end looping on files

           

            //sort
            srts.Sort();

            //process
            for (int u = 0; u < srts.Count-1; u++)
            {
                SrtItem current, next;
                current = srts.ElementAt(u);
                next = srts.ElementAt(u + 1);
                if (current.endTime > next.startTime) //if they overlap
                {
                    srts.Remove(current);
                    srts.Remove(next);
                    srts.Insert(u, current.merge(next));
                }
            }

            //
            //dump to file
            //
            
            //create file
            string fileName = Path.GetDirectoryName((string)listBox1.Items[0]) + string.Format("\\merged-{0}.srt", new Random(DateTime.Now.Millisecond).Next(999));
            File.WriteAllText(fileName, "", Encoding.UTF8);
            StreamWriter st= File.AppendText(fileName);
            
            int i = 1;
            foreach (SrtItem item in srts)
            {
                item.dumpWithNumber(i, st);
                i++;
            }
            st.Close();
        }
    }
}
