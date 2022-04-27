#if TOOLS
using Godot;
using System;

[Tool]
public class AirConsolePlugin : EditorPlugin
{
    public override void _EnterTree()
    {

        AddCustomType("AirConsole", "Node", GD.Load<Script>("AirConsole.cs"), GD.Load<Texture>("airconsole.png"));
    }

    public override void _ExitTree()
    {
        RemoveCustomType("AirConsole");
    }
}
#endif