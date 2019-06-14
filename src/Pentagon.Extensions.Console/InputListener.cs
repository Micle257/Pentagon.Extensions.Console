// -----------------------------------------------------------------------
//  <copyright file="InputListener.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class InputListener
    {
        public event EventHandler<ConsoleKeyInfo> KeyPressed;
        public event EventHandler KeyPressing;
        public static InputListener Current { get; } = new InputListener();

        public bool IsRunning { get; private set; }

        public Task StartListeningAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            if (IsRunning)
                throw new InvalidOperationException(message: "The listener is already running.");

            IsRunning = true;
            return Task.Run(() =>
                            {
                                ConsoleKeyInfo key;
                                do
                                {
                                    KeyPressing?.Invoke(this, EventArgs.Empty);
                                    key = Console.ReadKey(true);

                                    if (cancellationToken.IsCancellationRequested)
                                        return;

                                    KeyPressed?.Invoke(this, key);
                                } while (key.Key != ConsoleKey.Q || (key.Modifiers & ConsoleModifiers.Alt) != ConsoleModifiers.Alt);

                                IsRunning = false;
                            });
        }

        public void ErrorStop(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.CursorLeft = Console.CursorTop = 1;
            Console.WriteLine(value: "Error! Press any key to exit...");
        }
    }
}