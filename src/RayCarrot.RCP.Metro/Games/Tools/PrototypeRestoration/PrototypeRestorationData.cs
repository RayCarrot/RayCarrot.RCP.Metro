using System.Windows.Input;

namespace RayCarrot.RCP.Metro.Games.Tools.PrototypeRestoration;

public class PrototypeRestorationData
{
    public PrototypeRestorationData() : this(null, null) { }
    public PrototypeRestorationData(Dictionary<string, ToggleState>? toggleStates, Dictionary<int, Key>? keyboardButtonMapping)
    {
        ToggleStates = toggleStates ?? new Dictionary<string, ToggleState>();
        KeyboardButtonMapping = keyboardButtonMapping ?? new Dictionary<int, Key>();
    }

    public Dictionary<string, ToggleState> ToggleStates { get; }
    public Dictionary<int, Key> KeyboardButtonMapping { get; }
}