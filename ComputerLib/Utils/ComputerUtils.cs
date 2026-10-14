using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ComputerLib.Types;
using GorillaNetworking;
using HarmonyLib;

namespace ComputerLib.Utils;

public class ComputerUtils
{
    public static IEnumerable<Type> screens;

    private static readonly Dictionary<GorillaComputer.ComputerState, Screen> screensByState = new Dictionary<GorillaComputer.ComputerState, Screen>();

    private static readonly MethodInfo refreshFunctionNames = AccessTools.Method(typeof(GorillaComputer), "RefreshFunctionNames");

    private static Screen activeScreen;
    private static bool initialized;
    private static int menuWidth;
    internal readonly static int MaxVisibleRows = 14;

    internal static void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;

        screens = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => !assembly.IsDynamic)
            .SelectMany(GetLoadableTypes)
            .Where(type => type.IsSubclassOf(typeof(Screen)) && !type.IsAbstract)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();

        foreach (Type screen in screens)
        {
            if (Activator.CreateInstance(screen) is Screen screenInstance)
            {
                RegisterScreen(screenInstance);
            }
        }
        
        GorillaComputer computer = GorillaComputer.instance;
        RenderMenu(computer);
    }

    internal static void RegisterScreen(Screen screen)
    {
        GorillaComputer computer = GorillaComputer.instance;
        GorillaComputer.ComputerState state = (GorillaComputer.ComputerState)(Screen.StateBase + screensByState.Count);
        if (screensByState.ContainsKey(state))
        {
            Plugin.Logger.LogError($"failed to register '{screen.ScreenName}' as state {state} is already taken");
            return;
        }

        screen.MenuName = BuildMenuName(screen, computer);
        screen.State = state;
        screensByState.Add(state, screen);
        SyncOrderList(computer);
        Plugin.Logger.LogInfo($"registered screen '{screen.ScreenName}' as {state}.");
    }
    
    private static string BuildMenuName(Screen screen, GorillaComputer computer)
    {
        if (menuWidth <= 0)
        {
            menuWidth = computer.OrderList
                .Where(item => !screensByState.ContainsKey(item.State))
                .Select(item => item.GetName().Length)
                .DefaultIfEmpty(0)
                .Max();
        }

        string menuName = (screen.ScreenName ?? string.Empty).ToUpperInvariant();
        if (menuName.Length == 0)
        {
            return "N/A";
        }

        if (menuName.Length <= menuWidth)
        {
            return menuName;
        }

        Plugin.Logger.LogWarning($"'{screen.ScreenName}' truncated to {menuWidth} chars ('{menuName.Substring(0, menuWidth)}') so the computer menu pointer does not wrap.");
        return menuName.Substring(0, menuWidth);
    }

    internal static bool OwnsState(GorillaComputer.ComputerState state)
    {
        return screensByState.ContainsKey(state);
    }

    public static Screen ActiveScreen => activeScreen;

    internal static void Tick(GorillaComputer computer)
    {
        if (screensByState.Count == 0)
        {
            return;
        }

        SyncOrderList(computer);
        RenderMenu(computer);

        if (!screensByState.TryGetValue(computer.currentState, out Screen screen))
        {
            if (activeScreen != null)
            {
                activeScreen.OnView(false);
                activeScreen = null;
            }
            return;
        }

        screen.Text.Clear();
        if (activeScreen != screen)
        {
            activeScreen?.OnView(false);
            activeScreen = screen;
            screen.OnView(true);
        }

        screen.UpdateScreen();

        computer.screenText.Set(screen.Text.ToString());
    }

    internal static void OnInput(GorillaComputer computer, GorillaKeyboardBindings button)
    {
        if (screensByState.TryGetValue(computer.currentState, out Screen screen))
        {
            screen.OnInput(button);
        }
    }

    // pages lefthand side of gc
    // should be removed if aa ever adds pages themselves lol
    internal static void RenderMenu(GorillaComputer computer)
    {
        List<string> names = computer.FunctionNames;
        int count = names.Count;
        int selected = computer.GetStateIndex(computer.currentState);

        if (selected < 0 || count <= MaxVisibleRows)
        {
            computer.functionSelectText.Set(computer.GetOrderListForScreen(computer.currentState));
            return;
        }

        int start = selected - MaxVisibleRows / 2;
        if (start > count - MaxVisibleRows)
        {
            start = count - MaxVisibleRows;
        }

        if (start < 0)
        {
            start = 0;
        }

        int end = start + MaxVisibleRows;
        StringBuilder menu = new StringBuilder();
        for (int i = start; i < end; i++)
        {
            menu.Append(names[i]);
            if (i == selected)
            {
                menu.Append(computer.Pointer);
            }

            if (i < end - 1)
            {
                menu.Append("\n");
            }
        }

        computer.functionSelectText.Set(menu.ToString());
    }

    internal static void SyncOrderList(GorillaComputer computer)
    {
        bool changed = false;
        foreach (KeyValuePair<GorillaComputer.ComputerState, Screen> entry in screensByState)
        {
            if (computer.OrderList.Exists(item => item.State == entry.Key))
            {
                continue;
            }

            computer.OrderList.Add(new GorillaComputer.StateOrderItem(entry.Key, entry.Value.MenuName));
            changed = true;
        }

        if (!changed || refreshFunctionNames == null)
        {
            return;
        }

        refreshFunctionNames.Invoke(computer, null);
    }

    internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(type => type != null);
        }
    }
}
