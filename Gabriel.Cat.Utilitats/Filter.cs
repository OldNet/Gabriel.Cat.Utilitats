using Gabriel.Cat.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gabriel.Cat
{
    public class Filter:ObjectAutoId
    {
        static char[] splitChar;
        public static char[] SplitChar
        {
           get
            {
                if (splitChar == null) splitChar = new char[] { ';','|', ',' };
                return splitChar;
            }
            set
            {
                splitChar = value;
            }
        }
        string filter;

        public Filter(string filter=null)
        {
            FilterString = filter;
        }

        public string FilterString
        {
            get
            {
                string filter;
                if (this.filter == null)
                    filter = "*.*";
                else filter = this.filter;
                return filter;
            }
            set
            {
                this.filter = value;
            }
        }
        public string[] FilterParts
        {
            get
            {
                string[] parts;
                string filter = FilterString;
                if (filter.ContainsAny(SplitChar)){
                    parts = filter.Split(SplitChar); }
                else parts =new string[]{ filter};
                return parts;
            }
            set
            {
                StringBuilder str = new StringBuilder();
                if (value != null)
                {
                    for (int i = 0; i < value.Length; i++)
                        str.Append(value[i] + SplitChar[0]);
                    filter = str.ToString();
                }
                else filter = null;

            }
        }
        public static implicit operator string(Filter filter)
        {
            return filter.FilterString;
        }
        public static implicit operator Filter(string filter)
        {
            return new Filter(filter);
        }
    }
}
