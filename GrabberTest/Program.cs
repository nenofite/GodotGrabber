using Nenofite.GodotGrabber;

namespace GrabberTest
{
    internal partial class Program
    {
        [Grab]
        int beena;

        [Grab("aaa")]
        int fooba;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, Woorld!");
        }

        void Foo()
        {
            GrabNodes();
        }

        static T GetNode<T>(string path) => default;
    }
}
