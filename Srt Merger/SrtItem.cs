using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Srt_Merger
{
    class SrtItem : IComparable<SrtItem>
    {

        public List<string> textLines=new List<string>();
        public DateTime startTime, endTime;

        public SrtItem()
        {

        }

        //public SrtItem(string[] text, DateTime startTime)
        //{
        //    this.textLines.AddRange(text);
        //    this.startTime = startTime;
        //}

        public int CompareTo(SrtItem other)
        {
            return this.startTime.CompareTo(other.startTime);
        }

        public void dumpWithNumber(int number, StreamWriter st)
        {
            st.WriteLine(number);
            st.WriteLine(string.Format("{0} --> {1}", startTime.ToString("HH:mm:ss,fff"), endTime.ToString("HH:mm:ss,fff")));
            foreach (string item in textLines)
            {
                st.WriteLine(item);
            }
            st.WriteLine();
           
        }

        /// <summary>
        /// merges two srts, assuming sorted
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public SrtItem merge(SrtItem next)
        {
            SrtItem tmp = new SrtItem();
            tmp.startTime = this.startTime < next.startTime ? this.startTime : next.startTime;
            tmp.endTime = this.endTime > next.endTime ? this.endTime : next.endTime;
            tmp.textLines.AddRange(this.textLines);
            
            tmp.textLines.AddRange(next.textLines);
            return tmp;
        }

    }
}
