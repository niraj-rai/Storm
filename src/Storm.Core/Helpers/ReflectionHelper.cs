using System;
using System.IO;
using System.Reflection;

namespace Storm.Core.Helpers
{
    public static class ReflectionHelper
    {
        public static object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName).GetValue(obj, null);
        }

        public static object ExecuteMethod(object obj, string methodName, object[] param = null)
        {
            var type = obj.GetType();
            var methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var temp = methodInfo.Invoke(obj, param);
            return temp;
        }

        public static object ExecuteMethod(object obj, string methodName, Type[] paramTypes, object[] param = null)
        {
            var type = obj.GetType();
            var methodInfo = type.GetMethod(methodName, paramTypes);

            var temp = methodInfo.Invoke(obj, param);
            return temp;
        }

        public static object GetInstance(string libName, string className, Type[] paramTypes, object[] param, bool isWebApp = true, string libPath = null)
        {
            var asmFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, isWebApp ? "bin" : "", libName);
            if (isWebApp == false && !string.IsNullOrWhiteSpace(libPath))
                asmFilePath = Path.Combine(libPath, libName);

            var asm = Assembly.LoadFrom(asmFilePath);
            var type = asm.GetType(className);

            var methodInfo = type.GetConstructor(paramTypes);

            var temp = methodInfo.Invoke(param);
            return temp;
        }
    }
}