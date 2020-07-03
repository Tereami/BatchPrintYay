using System;
using System.Diagnostics;


namespace BatchPrintYay
{
    public class Logger
    {
        public Logger()
        {
            Debug.Listeners.Clear();
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string folder = System.IO.Path.GetDirectoryName(assemblyLocation);
            folder = System.IO.Path.Combine(folder, "logs");
            if (!System.IO.Directory.Exists(folder))
                System.IO.Directory.CreateDirectory(folder);

            string logFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";
            string logFilePath = System.IO.Path.Combine(folder, logFileName);
            TextWriterTraceListener tr =
                new TextWriterTraceListener(System.IO.File.CreateText(logFilePath));
            Debug.Listeners.Add(tr);
        }

        public void Write(string message)
        {
            Debug.WriteLine(message);
            Debug.Flush();
        }
    }
}
