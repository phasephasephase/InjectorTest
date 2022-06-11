using System;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

using static InjectorTest.Windows;

namespace InjectorTest
{
    public static class Injector // yes i stole from my own launcher
    {
        public static async Task Inject(string path)
        {
            await ApplyAppPackages(path);

            await Task.Run(() =>
            {
                Console.WriteLine("Injecting " + path);
                try
                {
                    var targetProcess = Program.Minecraft;
                    var procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
                        PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                        false, targetProcess.Id);

                    var loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                    var allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero,
                        (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT
                        | MEM_RESERVE, PAGE_READWRITE);

                    WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(path),
                        (uint)((path.Length + 1) * Marshal.SizeOf(typeof(char))), out _);
                    CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddress,
                        allocMemAddress, 0, IntPtr.Zero);

                    Console.WriteLine("Finished injecting");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Injection failed. Exception: " + e);
                }
            });
        }

        private static async Task ApplyAppPackages(string path)
        {
            await Task.Run(() =>
            {
                var infoFile = new FileInfo(path);
                var fSecurity = infoFile.GetAccessControl();
                fSecurity.AddAccessRule(
                    new FileSystemAccessRule(new SecurityIdentifier("S-1-15-2-1"),
                    FileSystemRights.FullControl, InheritanceFlags.None,
                    PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

                infoFile.SetAccessControl(fSecurity);
            });
            
            Console.WriteLine("Applied ALL_APPLICATION_PACKAGES permission to " + path);
        }
    }
}
