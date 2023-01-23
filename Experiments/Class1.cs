namespace Experiments
{
    internal class Class1
    {
        private string bolides = "bolides";

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
    }
}