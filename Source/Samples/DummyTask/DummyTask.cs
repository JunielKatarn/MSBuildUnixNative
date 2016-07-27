#region Using directives

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

#endregion

namespace DummyTaskNameSpace
{
    public class DummyTask : Task
    {
        #region Private members

        private string _clangExecutable = "";
        private string _sourceFile = "";
        private string _outputFile = "";

        #endregion

        #region Public properties

        public string ClangExecutable
        {
            get { return _clangExecutable; }
            set { _clangExecutable = value; }
        }

        public string SourceFile
        {
            get { return _sourceFile; }
            set { _sourceFile = value; }
        }

        public string OutputFile
        {
            get { return _outputFile; }
            set { _outputFile = value; }
        }

        #endregion

        #region Public methods

        public override bool Execute()
        {
            VerifyPaths();

            string arguments = SourceFile + " -o " + OutputFile;
            
            string stdout = String.Empty;
            string stderr = String.Empty;
            
            int errorCode = RunProgram(ClangExecutable, arguments, out stdout, out stderr);

            if(String.Empty != stdout)
            {
                Console.WriteLine("stdout: " + stdout + "\n");
            }
            if(String.Empty != stderr)
            {
                Console.WriteLine("stderr: " + stderr + "\n");
            }
            if(0 != errorCode)
            {
                Console.WriteLine("Error code: " + errorCode);
            }

            if(0 != errorCode)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Private methods

        private void VerifyPaths()
        {
            string errorMessage = String.Empty;
            
            if(String.Empty == SourceFile)
            {
                errorMessage = "Source file cannot be empty.";
            }
            else if(String.Empty == ClangExecutable)
            {
                errorMessage = "Clang path cannot be empty.";
            }
            else if(!File.Exists(SourceFile))
            {
                errorMessage = "Source file does not exist.";
            }
            else if(!File.Exists(ClangExecutable))
            {
                errorMessage = "Clang path does not exist.";
            }

            if(String.Empty != errorMessage)
            {
                throw new Exception(errorMessage);
            }
        }         

        private int RunProgram(string pathToExecutable, string arguments, out string standardOutput, out string standardError)
        {
            standardOutput = String.Empty;
            standardError  = String.Empty;

            StringBuilder sbOutput = new StringBuilder();
            StringBuilder sbError  = new StringBuilder();

            int exitCode = -1;

            Process process = null;

            try
            {
                process = new Process();

                process.StartInfo.FileName = pathToExecutable;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += new DataReceivedEventHandler((sender, e) => {
                    if(!String.IsNullOrEmpty(e.Data))
                    {
                        sbOutput.Append("\n" + e.Data);
                    }
                });

                process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => {
                    if(!String.IsNullOrEmpty(e.Data))
                    {
                        sbOutput.Append("\n" + e.Data);
                    }
                });

                Console.WriteLine("Command to execute: " + process.StartInfo.FileName + " " + process.StartInfo.Arguments);

                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while(!process.HasExited)
                {
                    process.WaitForExit(1);
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }

                exitCode = process.ExitCode;

                standardOutput = sbOutput.ToString();
                standardError  = sbError.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                if(process != null)
                {
                    process.Dispose();
                }
            }

            return exitCode;
        }

        #endregion
    }
}
