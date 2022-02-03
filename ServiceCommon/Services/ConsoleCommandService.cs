using System;
using System.Threading;
using NConsoleMenu;

namespace Monitor.ServiceCommon.Services;

public class ConsoleCommandService
{
    private readonly CMenu menu;

    public ConsoleCommandService()
    {
        menu = new CMenu();

        menu.OnQuit += OnMenuQuit;

        StartThread();
    }

    public void AddCommand(string command, Action<string> action, string help = null)
    {
        lock (menu)
        {
            menu.Add(command, action, help);
        }
    }

    private void StartThread()
    {
        Thread t = new(() => menu.Run())
        {
            Name = "Console command processor",
            IsBackground = true
        };
        t.Start();
    }

    private void OnMenuQuit(CMenu obj)
    {
        Environment.Exit(0);
    }
}