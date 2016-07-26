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
            Console.WriteLine("Hello\n");

            DummyTask task = new DummyTask();

            task.ClangExecutable = "/usr/bin/clang";
            task.SourceFile = "/home/calope/Desktop/devmain/Source/Samples/application/main.cpp";
            task.OutputFile = "/home/calope/Desktop/devmain/Source/Samples/application/a.out";
            
            task.Execute();

            Console.WriteLine("Goodbye\n");
        }
    }
}