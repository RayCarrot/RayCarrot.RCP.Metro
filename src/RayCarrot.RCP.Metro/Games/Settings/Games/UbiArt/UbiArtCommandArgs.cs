using System.Text;

namespace RayCarrot.RCP.Metro.Games.Settings;

public class UbiArtCommandArgs
{
    // This is the default key if none is provided
    private const string DefaultKey = "map";

    private Dictionary<string, string> ParsedCommands { get; } = new();

    private string ToString(string separator)
    {
        StringBuilder sb = new();

        int count = 0;
        foreach (KeyValuePair<string, string> cmd in ParsedCommands)
        {
            sb.Append(cmd.Key);
            sb.Append('=');
            sb.Append(cmd.Value);
            
            if (count < ParsedCommands.Count - 1)
                sb.Append(separator);

            count++;
        }

        return sb.ToString();
    }

    // Re-implemented from game's function ITF::ApplicationFramework::useCommandLineFile
    public static string ReadArgsFromCommandLineFile(string fileText)
    {
        fileText = fileText.Replace("\n", ";");
        fileText = fileText.Replace("\r", "");

        string outputText = String.Empty;
        int i = 0;
        while (i < fileText.Length)
        {
            int nextPos = fileText.Length;
            int endIndex = fileText.Length;
            int commentIndex = fileText.IndexOf("##", i, StringComparison.InvariantCulture);

            if (commentIndex != -1)
            {
                endIndex = commentIndex;

                int separatorIndex = fileText.IndexOf(';', commentIndex);
                if (separatorIndex != -1)
                    nextPos = separatorIndex;
            }

            outputText += fileText.Substring(i, endIndex - i);
            i = nextPos;
        }

        return outputText;
    }

    // Re-implemented from game's function ITF::CommandArgs::process
    public void Process(string args)
    {
        ParsedCommands.Clear();
        
        foreach (string cmd in args.Split(';'))
        {
            if (cmd.Length < 2)
                continue;

            int charIndex = -1;
            bool hasParsedKey = false;
            string keyString = String.Empty;
            string valueString = String.Empty;

            while (true)
            {
                charIndex++;

                if (charIndex >= cmd.Length)
                    break;

                char currentChar = cmd[charIndex];

                if (currentChar == ';')
                    break;

                if (currentChar == '=')
                {
                    hasParsedKey = true;
                }
                else if (currentChar != '\"')
                {
                    if (hasParsedKey)
                    {
                        valueString += currentChar;
                    }
                    else
                    {
                        keyString += currentChar;
                    }
                }
            }

            keyString = keyString.Trim();
            valueString = valueString.Trim();

            if (hasParsedKey)
            {
                keyString = keyString.ToLowerInvariant();
            }
            else
            {
                valueString = keyString;
                keyString = DefaultKey;
            }

            ParsedCommands.Add(keyString, valueString);
        }
    }

    public string ToCommandLineFileString() => ToString(Environment.NewLine);
    public string ToArgString() => ToString(";");

    public int? GetInt(string name)
    {
        return ParsedCommands.TryGetValue(name.ToLowerInvariant(), out string valueString) && 
               Int32.TryParse(valueString, out int valueInt) ? valueInt : null;
    }
    public void SetInt(string name, int? value)
    {
        if (value != null)
            ParsedCommands[name.ToLowerInvariant()] = $"{value}";
        else
            ParsedCommands.Remove(name.ToLowerInvariant());
    }

    public bool GetBool(string name)
    {
        return ParsedCommands.TryGetValue(name.ToLowerInvariant(), out string valueString) &&
               Int32.TryParse(valueString, out int valueInt) &&
               valueInt != 0;
    }
    public void SetBool(string name, bool value)
    {
        if (value)
            ParsedCommands[name.ToLowerInvariant()] = "1";
        else
            ParsedCommands.Remove(name.ToLowerInvariant());
    }

    public string? GetString(string name)
    {
        return ParsedCommands.TryGetValue(name.ToLowerInvariant(), out string valueString) ? valueString : null;
    }
    public void SetString(string name, string? value)
    {
        if (!value.IsNullOrEmpty())
            ParsedCommands[name.ToLowerInvariant()] = value;
        else
            ParsedCommands.Remove(name.ToLowerInvariant());
    }
}