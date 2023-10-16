using NodeGrabber;

namespace GrabberTest
{
    internal partial class Program
    {
        [Grab]
        int beena;

        [Grab]
        int fooba;

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
