using Godot;
using System;

public static class Utils
{
    public static String ReadFile(String path)
    {
        File file = new File();
        file.Open(path, File.ModeFlags.Read);
        String content = file.GetAsText();
        file.Close();

        return content;
    }

    public static void DebugPrint(String str)
    {
        GD.Print(str);
    }

    public static Spatial SpawnModel(Node parent, String path)
    {
        PackedScene scene = (PackedScene)GD.Load(path);
        Spatial model = (Spatial)scene.Instance();
        parent.AddChild(model);
        return model;
    }
}
