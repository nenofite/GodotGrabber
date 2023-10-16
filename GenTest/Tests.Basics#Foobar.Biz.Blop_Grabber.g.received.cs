//HintName: Foobar.Biz.Blop_Grabber.g.cs

namespace Foobar.Biz
{
    partial class Blop
    {
        void GrabNodes()
        {
            ShouldInclude = GetNode<int>("%ShouldInclude");
ShouldSkipNoAttribute = GetNode<string>("%ShouldSkipNoAttribute");
ShouldIncludeQualType = GetNode<System.Text.StringBuilder>("%ShouldIncludeQualType");
        }
    }
}
