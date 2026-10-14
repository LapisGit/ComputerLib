using System.Text;
using GorillaNetworking;

namespace ComputerLib.Types;

public abstract class Screen
{
    // make sure we dont override a normal screen
    internal const int StateBase = 1000;

    // text that actual shows the content
    private readonly StringBuilder text = new StringBuilder();

    // unfiltered name that is filtered into MenuName
    public abstract string ScreenName { get; }

    // set taht content text
    public StringBuilder Text => text;

    // what actually lands in the left menu, since we need to clean it up on render
    internal string MenuName { get; set; }

    // the page this screen is bound to, assigned by ComputerUtils on registration
    public GorillaComputer.ComputerState State { get; internal set; }

    // gets called when player switches on/off page
    public abstract void OnView(bool activated);

    // gets called on update when player is active on page
    public abstract void UpdateScreen();

    // gets called whenever a keyboard button event is detected
    public abstract void OnInput(GorillaKeyboardBindings button);
}
