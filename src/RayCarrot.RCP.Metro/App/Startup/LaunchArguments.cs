using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace RayCarrot.RCP.Metro;

public class LaunchArguments
{
    public LaunchArguments(string[] args)
    {
        Args = args;

        if (args.Length > 0 && File.Exists(args[0]))
            FilePathArg = args[0];
    }

    public string[] Args { get; }
    public FileSystemPath? FilePathArg { get; }

    public bool HasArg(string arg) => Array.IndexOf(Args, arg) != -1;
    
    public bool HasArg(string arg, [NotNullWhen(true)]out string? value)
    {
        int index = Array.IndexOf(Args, arg);

        if (index != -1)
        {
            value = Args[index + 1];
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}