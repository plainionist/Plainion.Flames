using System;
using System.Collections.Generic;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Etlx;
using Plainion.Collections;
using Plainion.Flames.Model;

namespace Plainion.Flames.Modules.ETW.Builders
{
    class CallstackBuilder
    {
        private TraceModelBuilder myBuilder;
        private Index<string, Index<string, Method>> myMethodIndex;

        public CallstackBuilder(TraceModelBuilder builder)
        {
            myBuilder = builder;
            myMethodIndex = new Index<string, Index<string, Method>>(mod => new Index<string, Method>(method => CreateMethod(mod, method)));
        }

        public IReadOnlyList<Method> GetCallstack(TraceEvent evt)
        {
            var traceLog = evt.Source as Microsoft.Diagnostics.Tracing.Etlx.TraceLog;

            var callStack = traceLog.CallStacks[traceLog.GetCallStackIndexForEvent(evt)];
            if (callStack == null)
            {
                return null;
            }

            // add 1 for the potential pseudo frame "broken". One more item hurds less than a potential automatic
            // growth of the entire list
            var frames = new List<Method>(callStack.Depth + 1);

            while (callStack != null)
            {
                if (callStack.CodeAddress != null)
                {
                    frames.Add(GetMethod(callStack.CodeAddress));
                }

                callStack = callStack.Caller;
            }

            if (frames.Count > 0 && IsBroken(frames[frames.Count - 1]))
            {
                frames.Add(myBuilder.BrokenCallstackMethod());
            }

            frames.Reverse();

            return frames;
        }

        public Method GetMethod(TraceCodeAddress codeAddress)
        {
            string module = string.IsNullOrEmpty(codeAddress.ModuleName) ? "Unknown" : codeAddress.ModuleName;

            if (codeAddress.ModuleFilePath.EndsWith(".sys", StringComparison.OrdinalIgnoreCase)
                && !module.EndsWith(".sys", StringComparison.OrdinalIgnoreCase))
            {
                module += ".sys";
            }

            return myMethodIndex[module][codeAddress.FullMethodName];
        }

        public Method CreateMethod(string module, string fullMethodName)
        {
            if (string.IsNullOrWhiteSpace(fullMethodName))
            {
                return myBuilder.CreateMethod(module, null, null, "?");
            }

            var tokens = fullMethodName.Split(new[] { '(' }, 2);
            var fullName = tokens[0];

            string nameSpace;
            string className;
            string method;
            Parse(fullName, out nameSpace, out className, out method);

            return myBuilder.CreateMethod(module, nameSpace, className, method);
        }

        private static void Parse(string fullName, out string nameSpace, out string className, out string method)
        {
            nameSpace = null;
            className = null;
            method = null;

            var pos = fullName.LastIndexOf('.');
            if (pos > 0)
            {
                // .Net
                method = fullName.Substring(pos + 1);
                var domain = fullName.Substring(0, pos);

                pos = domain.IndexOf('[');
                if (pos > 0)
                {
                    pos = domain.Substring(0, pos).LastIndexOf('.');
                }
                else
                {
                    pos = domain.LastIndexOf('.');
                }

                if (pos > 0)
                {
                    className = domain.Substring(pos + 1);
                    nameSpace = domain.Substring(0, pos);
                }
                else
                {
                    className = domain;
                }

                return;
            }

            pos = fullName.LastIndexOf("::");
            if (pos >= 0)
            {
                // c++
                method = fullName.Substring(pos + 2);

                // not sure what usecase that is but we had one example
                method = string.IsNullOrEmpty(method) ? "?" : method;

                var domain = fullName.Substring(0, pos);

                pos = domain.IndexOf('<');
                if (pos > 0)
                {
                    pos = domain.Substring(0, pos).LastIndexOf("::");
                }
                else
                {
                    pos = domain.LastIndexOf("::");
                }

                if (pos > 0)
                {
                    className = domain.Substring(pos + 2);
                    nameSpace = domain.Substring(0, pos);
                }
                else
                {
                    className = domain;
                }

                return;
            }

            // c
            method = fullName;
        }

        private static bool IsBroken(Method callStackRoot)
        {
            if (callStackRoot.Module == "ntoskrnl" || callStackRoot.Module.EndsWith(".sys"))
            {
                // no broken stack detection for kernel or kernel drivers
                return false;
            }

            if (callStackRoot.Module == "ntdll" && (callStackRoot.Name == "RtlUserThreadStart" || callStackRoot.Name == "_RtlUserThreadStart"))
            {
                return false;
            }

            return true;
        }
    }
}
