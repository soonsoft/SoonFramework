using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Service
{
    public abstract class ServiceStarter
    {
        static bool setConsoleColor = true;

        public void Start(string[] args)
        {
            string exeArg = String.Empty;

            if (args == null || args.Length < 1)
            {
                Console.WriteLine("请选择启动方式...");
                Console.WriteLine("-[r]: 从控制台启动;");
                Console.WriteLine("-[i]: 安装到Windows服务;");
                Console.WriteLine("-[u]: 从Windows服务卸载;");

                while (true)
                {
                    exeArg = Console.ReadKey().KeyChar.ToString();
                    Console.WriteLine();

                    if (Run(exeArg, null))
                        break;
                }
            }
            else
            {
                exeArg = args[0];

                if (!string.IsNullOrEmpty(exeArg))
                    exeArg = exeArg.TrimStart('-');

                Run(exeArg, args);
            }
        }

        protected virtual bool Run(string exeArg, string[] startArgs)
        {
            switch (exeArg.ToLower())
            {
                case ("i"):
                    InstallWindowsService();
                    return true;

                case ("u"):
                    UninstallWindowsService();
                    return true;

                case ("r"):
                    RunAsConsole();
                    return true;

                default:
                    Console.WriteLine("无效的参数!");
                    return false;
            }
        }

        protected abstract void InstallWindowsService();

        protected abstract void UninstallWindowsService();

        protected virtual void RunAsConsole()
        {
            Console.WriteLine("欢迎使用控制台启动应用程序!");
            CheckCanSetConsoleColor();

            Console.ResetColor();
            Console.WriteLine("输入 'quit' 并回车停止服务");
        }

        void CheckCanSetConsoleColor()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.ResetColor();
                setConsoleColor = true;
            }
            catch
            {
                setConsoleColor = false;
            }
        }

        protected static void SetConsoleColor(ConsoleColor color)
        {
            if (setConsoleColor)
                Console.ForegroundColor = color;
        }
    }
}
