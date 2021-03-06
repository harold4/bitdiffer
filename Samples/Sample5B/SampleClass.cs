using System;
using System.Collections.Generic;
using System.Text;

namespace BitDiffer.Samples.Sample5
{
    public class SampleClass
    {
        public void PublicMethod1()
        {
        }

        public void TestMethod(int x)
        {
        }

        internal void InternalMethod1(int x)
        {
        }

        public string SampleProperty1
        {
            get { return ""; }
        }

        public int SampleProperty2
        {
            get { return 0; }
            set { ; }
        }

        public event EventHandler PublicEvent;

        public enum ExampleEnum
        {
            ExampleValue1,
            ExampleValue2
        };
    }
}
