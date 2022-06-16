using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace InjectorTest
{
    internal class Program
    {
        public static Process Minecraft;

        public static Task Main(string[] args) => new Program().MainAsync(args);

        private async Task MainAsync(string[] args)
        {
            string arg; 
            start:
            // if you didn't provide a correct arg then this will always be true
            if (args.Length == 0 || args.Length > 1)
            {
                Console.WriteLine("Provide a path to a DLL file or a link to a DLL:");
                arg = Console.ReadLine();
            }
            else
            {
                arg = args[0];
            }

            if (arg != null && !arg.EndsWith(".dll"))
            {
                Console.WriteLine("That's not a DLL file!");
                goto start;
            }

            var minecraftIndex = Process.GetProcessesByName("Minecraft.Windows");

            if (minecraftIndex.Length != 0)
            {
                Console.WriteLine("Minecraft is already open, skipping module check");
                Minecraft = minecraftIndex[0];
                await Injector.Inject(arg);
            }
            else
            {
                Process.Start("minecraft://");
                Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
                await WaitForModules();
                await Injector.Inject(arg);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private async Task WaitForModules()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Waiting for Minecraft to load");
                while (true)
                {
                    Minecraft.Refresh();
                    if (Minecraft.Modules.Count > 160) break;
                    Thread.Sleep(4000);
                }
            });
            Console.WriteLine("Minecraft finished loading");
        }
    }
}
