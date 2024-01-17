# Godot Grabber

Godot Grabber is a source generator for Godot C# scripts. It reduces boilerplate when getting nodes within a script:

```cs
partial class MyNode : Node
{
    // This will get a scene-unique node named "%CharacterSprite"
    [Grab]
    Sprite2D CharacterSprite;

    // This will get a node at path "Other/Sprite"
    [Grab("Other/Sprite")]
    Sprite2D EnemySprite;

    public override void _Ready()
    {
        // Call within _Ready() to actually get the above nodes
        GrabNodes();
    }
}
```

The generator makes a private method `GrabNodes` to set the values of each field marked with `[Grab]`. Because it's generated at compile time, no runtime reflection is performed. This keeps things lightweight.

## Scene unique nodes

The default and recommended behavior is to use [scene unique nodes](https://docs.godotengine.org/en/stable/tutorials/scripting/scene_unique_nodes.html) that exactly match the name of the C# field. These nodes are marked with a `%` in the Godot editor.

Because the nodes are scene unique, you can move them within the hierarchy of the scene without changing the binding in C#.

## Custom paths

If you'd like to specify a custom path instead, pass a path to the attribute:

```cs
[Grab("Other/Sprite")]
Sprite2D EnemySprite;
```

The path string is passed directly to `Node.GetNode()`, so it supports the usual [NodePath](https://docs.godotengine.org/en/stable/classes/class_nodepath.html#class-nodepath) syntax.