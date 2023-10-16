using NodeGrabber;

namespace GrabberTest
{
    internal partial class Program
    {
        [Grab]
        int baoana;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Woorld!");
        }

        void Foo()
        {
            GrabNodes();
        }
    }
}
