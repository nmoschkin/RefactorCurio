namespace Experiments
{
    internal class Class1
    {
        public string Test4
        {
            get
            {
                return "Bonny";
            }
        }

        private string bolides = "bolides";
        private string bolides2 = "Bunny";
        public string Test1 => "Bonkers";

        public int Test2() => 1;

        public string Test3
        {
            get => bolides;
            set
            {
                bolides = value;
            }
        }

        public string Test5
        {
            get
            {
                return bolides2;
            }
            protected set
            {
                bolides2 = value;
            }
        }
    }
}