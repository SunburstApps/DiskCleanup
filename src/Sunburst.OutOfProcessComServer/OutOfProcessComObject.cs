﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Sunburst.OutOfProcessComServer
{
    public abstract class OutOfProcessComObject
    {
        private static int ServerRefCount;

        public static void RunComServer(IEnumerable<Type> comObjectTypes)
        {
            if (comObjectTypes == null) throw new ArgumentNullException(nameof(comObjectTypes));

            RegistrationServices services = new RegistrationServices();
            List<int> registrationCookies = new List<int>();

            try
            {
                foreach (var type in comObjectTypes)
                {
                    if (!type.IsSubclassOf(typeof(OutOfProcessComObject)))
                        throw new ArgumentException($"Out-of-process COM type {type.FullName} must subclass {nameof(OutOfProcessComObject)}.");

                    int cookie = services.RegisterTypeForComClients(type, RegistrationClassContext.LocalServer, RegistrationConnectionType.MultipleUse);
                    registrationCookies.Add(cookie);
                }

                Application.Run();
            }
            finally
            {
                foreach (var cookie in registrationCookies)
                    services.UnregisterTypeForComClients(cookie);
            }
        }

        protected OutOfProcessComObject(OutOfProcessComObjectFlags flags)
        {
            Interlocked.Increment(ref ServerRefCount);
        }

        ~OutOfProcessComObject() {
            int newRefCount = Interlocked.Decrement(ref ServerRefCount);
            if (newRefCount == 0) Application.Exit();
        }
    }

    [Flags]
    public enum OutOfProcessComObjectFlags
    {
        Default = 0
    }
}
