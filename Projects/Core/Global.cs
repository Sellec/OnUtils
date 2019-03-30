using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Microsoft.Deployment.WindowsInstaller;
using System.Reflection;

namespace OnUtils
{
    /// <summary>
    /// </summary>
    public static class Global
    {
        static Global()
        {
            CanUseAsync = false;

            bool load1 = true;
            try { Assembly.Load("System, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e, Retargetable=Yes"); }
            catch { load1 = false; }

            bool load2 = true;
            try { Assembly.Load("System.Core, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e, Retargetable=Yes"); }
            catch { load2 = false; }

            if (!load1 && !load2)
            {
                //if (IsPatchAlreadyInstalled("{F5B09CFD-F0B2-36AF-8DF4-1DF6B63FC7B4}", "KB2468871")) CanUseAsync = true;// .NET Framework 4 Client Profile 64-bit
                //else if (IsPatchAlreadyInstalled("{8E34682C-8118-31F1-BC4C-98CD9675E1C2}", "KB2468871")) CanUseAsync = true;// .NET Framework 4 Extended 64-bit
                //else if (IsPatchAlreadyInstalled("{3C3901C5-3455-3E0A-A214-0B093A5070A6}", "KB2468871")) CanUseAsync = true;// .NET Framework 4 Client Profile 32-bit
                //else if (IsPatchAlreadyInstalled("{0A0CADCF-78DA-33C4-A350-CD51849B9702}", "KB2468871")) CanUseAsync = true;// .NET Framework 4 Extended 32-bit
            }
            else CanUseAsync = true;
        }

        //private static bool IsPatchAlreadyInstalled(string productCode, string patchCode)
        //{
        //    var patches = PatchInstallation.GetPatches(null, productCode, null, UserContexts.Machine, PatchStates.Applied);
        //    return patches.Any(patch => patch.DisplayName == patchCode);
        //}

        /// <summary>
        /// Указывает, доступно ли текущему приложению использование async/await.
        /// </summary>
        public static bool CanUseAsync
        {
            get;
            private set;
        }

        /// <summary>
        /// Свойство используется в пуле объектов <see cref="Architecture.ObjectPool.ObjectPool{TPoolObject}"/> и при запуске приложения в <see cref="Startup.StartupFactory"/>.
        /// Присвоенный метод должен возвращать true для сборок, которые необходимо игнорировать при обходе всех типов в сборке.
        /// </summary>
        public static Func<Assembly, bool> CheckIfExcludeFromAssemblyWatching { get; set; }

        internal static bool CheckIfIgnoredAssembly(Assembly assembly)
        {
            if (CheckIfNetAssembly(assembly)) return true;

            if (CheckIfExcludeFromAssemblyWatching != null)
                if (CheckIfExcludeFromAssemblyWatching(assembly)) return true;

            return false;
        }

        internal static bool CheckIfNetAssembly(Assembly assembly)
        {
            if (assembly.FullName.ToLower().EndsWith(", publickeytoken=31bf3856ad364e35") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=b77a5c561934e089") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=b03f5f7f11d50a3a") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=69c3241e6f0468ca") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=71e9bce111e9429c") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=842cf8be1de50553") ||
                assembly.FullName.ToLower().EndsWith(", publickeytoken=89845dcd8080cc91"))
            {
                //Debug.WriteLineNoLog($"ObjectPool<{typeof(TProviderInterface).FullName}, {typeof(TFactoryType).FullName}>.CheckIfNetAssembly skipped '{assembly.FullName}'");
                return true;
            }
            return false;
        }

    }
}
