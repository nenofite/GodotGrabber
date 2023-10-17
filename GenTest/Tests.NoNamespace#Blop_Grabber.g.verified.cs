//HintName: Blop_Grabber.g.cs

partial class Blop
{
    void GrabNodes()
    {
ShouldInclude = GetNode<int>("%ShouldInclude");
ShouldIncludeQualType = GetNode<System.Text.StringBuilder>("%ShouldIncludeQualType");
withPath = GetNode<bool>("special path");
    }
}
