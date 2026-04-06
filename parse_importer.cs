using UnityEditor;
using UnityEngine;
using System.Reflection;

public class ParseImporter {
    public static void Main() {
        var t = typeof(ModelImporterClipAnimation);
        foreach(var f in t.GetProperties()) {
            System.Console.WriteLine(f.Name);
        }
    }
}
