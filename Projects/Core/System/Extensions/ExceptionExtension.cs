using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Security;
using System.Diagnostics;

namespace System
{
    /// <summary>
    /// </summary>
    public static class ExceptionExtension
    {
        private static MethodInfo _methodClassName = typeof(Exception).GetMethod("GetClassName", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo _methodEnvironment_GetResourceString1 = typeof(Environment).GetMethod("GetResourceString", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(string) }, null);
        private static MethodInfo _methodStackFrame_GetIsLastFrameFromForeignExceptionStackTrace = typeof(StackFrame).GetMethod("GetIsLastFrameFromForeignExceptionStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Создает и возвращает строковое представление текущего исключения, при этом выводится только указанное количество фреймов.
        /// </summary>
        /// <param name="exception">Текущее исключение.</param>
        /// <param name="stackLimit">Количество фреймов из стека вызовов, которое необходимо вставить в вывод.</param>
        /// <returns>Строковое представление текущего исключения.</returns>
        public static string ToString(this Exception exception, int stackLimit)
        {
            string text = exception.Message;
            string text2;
            if (text == null || text.Length <= 0)
            {
                text2 = exception.GetType().ToString();
            }
            else
            {
                text2 = exception.GetType().ToString() + ": " + text;
            }
            if (exception.InnerException != null)
            {
                text2 = string.Concat(new string[]
                {
                    text2,
                    " ---> ",
                    exception.InnerException.ToString(stackLimit),
                    Environment.NewLine,
                    "   ",
                    Environment_GetResourceString("Exception_EndOfInnerExceptionStack")
                });
            }

            var trace = new System.Diagnostics.StackTrace(exception, true);

            string stackTrace = StackTrace_ToString(trace, stackLimit);
            if (stackTrace != null)
            {
                text2 = text2 + Environment.NewLine + stackTrace;
            }
            return text2;


        }

        private static string Environment_GetResourceString(string key)
        {
            return _methodEnvironment_GetResourceString1.Invoke(null, new object[] { key }) as string;
        }

        private static string StackTrace_ToString(System.Diagnostics.StackTrace stack, int stackLimit)
        {
            bool flag = true;
            string arg = Environment_GetResourceString("Word_At");
            string format = Environment_GetResourceString("StackTrace_InFileLineNumber");

            bool flag2 = true;
            var stringBuilder = new StringBuilder(255);
            int i = 0;
            for (; i < stack.FrameCount && i < stackLimit; i++)
            {
                var frame = stack.GetFrame(i);
                var method = frame.GetMethod();
                if (method != null)
                {
                    if (flag2)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        stringBuilder.Append(Environment.NewLine);
                    }
                    stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "   {0} ", arg);
                    var declaringType = method.DeclaringType;
                    if (declaringType != null)
                    {
                        stringBuilder.Append(declaringType.FullName.Replace('+', '.'));
                        stringBuilder.Append(".");
                    }
                    stringBuilder.Append(method.Name);
                    if (method is MethodInfo && ((MethodInfo)method).IsGenericMethod)
                    {
                        Type[] genericArguments = ((MethodInfo)method).GetGenericArguments();
                        stringBuilder.Append("[");
                        int j = 0;
                        bool flag3 = true;
                        while (j < genericArguments.Length)
                        {
                            if (!flag3)
                            {
                                stringBuilder.Append(",");
                            }
                            else
                            {
                                flag3 = false;
                            }
                            stringBuilder.Append(genericArguments[j].Name);
                            j++;
                        }
                        stringBuilder.Append("]");
                    }
                    stringBuilder.Append("(");
                    var parameters = method.GetParameters();
                    bool flag4 = true;
                    for (int k = 0; k < parameters.Length; k++)
                    {
                        if (!flag4)
                        {
                            stringBuilder.Append(", ");
                        }
                        else
                        {
                            flag4 = false;
                        }
                        string str = "<UnknownType>";
                        if (parameters[k].ParameterType != null)
                        {
                            str = parameters[k].ParameterType.Name;
                        }
                        stringBuilder.Append(str + " " + parameters[k].Name);
                    }
                    stringBuilder.Append(")");
                    if (flag && frame.GetILOffset() != -1)
                    {
                        string text = null;
                        try
                        {
                            text = frame.GetFileName();
                        }
                        catch (NotSupportedException)
                        {
                            flag = false;
                        }
                        catch (SecurityException)
                        {
                            flag = false;
                        }
                        if (text != null)
                        {
                            stringBuilder.Append(' ');
                            stringBuilder.AppendFormat(CultureInfo.InvariantCulture, format, text, frame.GetFileLineNumber());
                        }
                    }
                    if ((bool)_methodStackFrame_GetIsLastFrameFromForeignExceptionStackTrace.Invoke(frame, new object[] { }))
                    {
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(Environment_GetResourceString("Exception_EndStackTraceFromPreviousThrow"));
                    }
                }
            }
            if (stackLimit < stack.FrameCount)
            {
                if (!flag2) stringBuilder.Append(Environment.NewLine);
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "   еще {0} вызовов скрыто", stack.FrameCount - i);
            }
            return stringBuilder.ToString();
        }
    }


}
