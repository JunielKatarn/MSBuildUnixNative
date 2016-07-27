#region Using directives

using DummyTaskNameSpace;
using System;

#endregion

namespace Testeame
{
    public class Program
    {
        public static void Main()
        {
            DummyTask task = new DummyTask();

            task.ClangExecutable = "/usr/bin/clang";
            task.SourceFile = "../application/main.cpp";
            task.OutputFile = "../application/a.out";
            
            task.Execute();
        }
    }
}