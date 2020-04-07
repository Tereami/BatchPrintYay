using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchPrintYay
{
    public class MyDimension
    {
        private int _mms;

        public int Millimeters
        {
            get { return _mms; }
            set
            {
                _mms = value;
                _feets = value / 304.8;
            }
        }

        private double _feets;
        public double Feets
        {
            get { return _feets; }
            set
            {
                _feets = value;
                _mms = (int)Math.Round((value * 304.8), 0);
            }
        }
    }
}
