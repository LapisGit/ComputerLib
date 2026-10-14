# ComputerLib
a tiny gorilla tag library that allows mod developers to add custom pages to the gorillacomputer


## Example

```csharp
public class TestScreen : Screen
{
    public override string ScreenName => "TestScreen";
    public static string LastPressedButton;
    
    public override void OnView(bool activated)
    {
        Plugin.Logger.LogInfo($"test screen {activated}");
    }
    
    public override void UpdateScreen()
    {
        Text.AppendLine("HELLO! :3");
        Text.AppendLine("");
        Text.AppendLine("Last pressed button: " + LastPressedButton);
    }

    public override void OnInput(GorillaKeyboardBindings button)
    {
        Plugin.Logger.LogInfo($"test keyboard binding: {button}");
        LastPressedButton = button.ToString();
    }
}
```
