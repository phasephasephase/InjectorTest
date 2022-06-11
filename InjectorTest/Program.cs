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
            if (args.Length == 0 || args.Length > 1 || !args[0].EndsWith(".dll"))
            {
                Console.WriteLine("Provide a path to a DLL file (use \"quotes\" if you have to)");
                Environment.Exit(-1);
            }

            var minecraftIndex = Process.GetProcessesByName("Minecraft.Windows");

            if (minecraftIndex.Length != 0)
            {
                Console.WriteLine("Minecraft is already open, skipping module check");
                Minecraft = minecraftIndex[0];
                await Injector.Inject(args[0]);
            }
            else
            {
                Process.Start("minecraft://");
                Minecraft = Process.GetProcessesByName("Minecraft.Windows")[0];
                await WaitForModules();
                await Injector.Inject(args[0]);
            }

            Console.WriteLine("done");
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
