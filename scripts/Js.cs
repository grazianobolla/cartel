using Godot;

public static class Js
{
    public static int GetArraySize(this JavaScriptObject jsArray)
    {
        return (int)jsArray.Get("length");
    }

    public static object GetAt(this JavaScriptObject jsArray, int index)
    {
        return jsArray.Call("at", index);
    }

    public static JavaScriptObject GetAllButFirst(this JavaScriptObject jsArray)
    {
        return (JavaScriptObject)jsArray.Call("slice", 1);
    }

    public static Godot.Collections.Array ToArray(this JavaScriptObject jsArray)
    {
        int length = jsArray.GetArraySize();
        Godot.Collections.Array array = new Godot.Collections.Array();
        for (int i = 0; i < length; i++)
        {
            array.Add(jsArray.GetAt(i));
        }

        return array;
    }
}
