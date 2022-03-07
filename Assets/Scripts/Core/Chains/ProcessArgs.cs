using System;
using System.Text;

namespace Core.Chains
{
    /// <summary>
    /// Arguments for status of running process: success, failure, etc.
    /// </summary>
    [Serializable]
    public class ProcessArgs : EventArgs
    {
        public bool success = true;
        StringBuilder sb = new StringBuilder();
        public string Message => sb.ToString();

        public static ProcessArgs Success()
        {
            return new ProcessArgs();
        }

        public static ProcessArgs Failed(string message = null)
        {
            ProcessArgs args = new ProcessArgs();
            if (message == null)
            {
                args.success = false;
                return args;
            }

            return args.Fail(message);
        }

        public ProcessArgs Success(string message)
        {
            return AddMessage(message);
        }

        public ProcessArgs Fail(string message)
        {
            success = false;
            return AddMessage(message);
        }

        public ProcessArgs AddMessage(string message)
        {
            if (string.IsNullOrEmpty(message)) return this;
            
            if (sb.Length != 0)
                sb.Append("\n");
            
            sb.Append(message);
            return this;
        }

        public ProcessArgs Merge(ProcessArgs newArgs)
        {
            if (!newArgs.success)
            {
                success = false;
            }

            return AddMessage(newArgs.Message);
        }
    }
}