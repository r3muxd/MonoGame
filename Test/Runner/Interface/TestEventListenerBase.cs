using System;
using System.IO;
using NUnit.Framework.Interfaces;

namespace MonoGame.Tests
{
    public class TestEventListenerBase : ITestListener
    {
        private readonly TextWriter _stdoutStandin;
        protected TextWriter StdoutStandin { get { return _stdoutStandin; } }

        private readonly StreamWriter _stdout;

        public TestEventListenerBase ()
        {
            _stdoutStandin = new StringWriter ();
            Console.SetOut (_stdoutStandin);
            _stdout = new StreamWriter (Console.OpenStandardOutput ());
            _stdout.AutoFlush = true;
        }

        public virtual void RunStarted(string name, int testCount)
        {
            _stdout.WriteLine("Run Started: {0}; {1} tests", name, testCount);
        }

        public virtual void RunFinished(Exception exception)
        {
            _stdout.WriteLine();
        }

        public virtual void RunFinished(ITestResult result)
        {
            _stdout.WriteLine();
            StdoutStandin.WriteLine(string.Format("Total run time was {0} seconds\n", result.Duration));
        }

        public void TestStarted(ITest test)
        {
            _stdoutStandin.WriteLine(test.FullName);
        }

        public void TestFinished(ITestResult result)
        {
            char output;
            switch (result.ResultState.Status)
            {
                case TestStatus.Failed:
                    output = 'F';
                    break;
                case TestStatus.Inconclusive:
                    output = '?';
                    break;
                case TestStatus.Skipped:
                    output = 'S';
                    break;
                default:
                    output = '.';
                    break;
            }

            _stdout.Write (output);

            _stdoutStandin.WriteLine("Finished: " + result.FullName);
            _stdoutStandin.WriteLine();
        }

        public void TestOutput(TestOutput output)
        {
        }
    }
}
