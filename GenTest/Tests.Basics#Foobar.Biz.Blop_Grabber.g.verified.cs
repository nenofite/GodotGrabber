//HintName: Foobar.Biz.Blop_Grabber.g.cs

namespace Foobar.Biz
{

partial class Blop
{
    /// <summary>
    /// Set the values of all fields marked with <c>[Grab]</c>. You should usually call this in <c>_Ready()</c>.
    /// 
    /// The current value of each field will be overwritten.
    /// </summary>
    void GrabNodes()
    {
        ShouldInclude = GetNode<int>("%ShouldInclude");
        ShouldIncludeQualType = GetNode<System.Text.StringBuilder>("%ShouldIncludeQualType");
        withPath = GetNode<bool>("special path");
    }
}

}
