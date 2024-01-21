//HintName: Fruit_Grabber.g.cs

partial class Fruit
{
    /// <summary>
    /// Set the values of all fields marked with <c>[Grab]</c>. You should usually call this in <c>_Ready()</c>.
    /// 
    /// The current value of each field will be overwritten.
    /// </summary>
    void GrabNodes()
    {
        Apple = GetNode<string>("%Apple");
        Banana = GetNode<int>("../Banana");
    }
}
