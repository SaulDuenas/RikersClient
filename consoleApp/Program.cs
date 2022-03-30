using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Monitor.Directory;
using RikersProxy;
using Service.Domian;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using NetLogger.Implementation;
using Service.Domian.Core;

namespace consoleApp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Form1());

            // Dependencie Injection
         //   var container = Bootstrap();
         //   Application.Run(container.GetInstance<Form1>());
            
        }


        private static Container Bootstrap()
        {
            // Create the container as usual.
            var container = new Container();

            // Register your types, for instance:
            container.Register<IClientProxy,ClientProxy>(Lifestyle.Singleton);
            container.Register<Winlogger>();
            container.Register<FileLogger>();
            container.Register<Logger>();
            
            AutoRegisterWindowsForms(container);

            container.Verify();

            return container;
        }

        private static void AutoRegisterWindowsForms(Container container)
        {
            var types = container.GetTypesToRegister<Form>(typeof(Program).Assembly);

            foreach (var type in types)
            {
                var registration =
                    Lifestyle.Transient.CreateRegistration(type, container);

                registration.SuppressDiagnosticWarning(
                    DiagnosticType.DisposableTransientComponent,
                    "Forms should be disposed by app code; not by the container.");

                container.AddRegistration(type, registration);
            }
        }

    }
}
