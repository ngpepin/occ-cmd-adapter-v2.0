using System;
using System.Diagnostics;

namespace OccCmdAdapter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: occ-cmd-adapter <full file system path>");
                return;
            }

            string fileSystemPath = args[0].TrimEnd('/');
            string baseFileSystemPath = "/mnt/myJBOD/";

            // Transform filesystem path into Nextcloud path
            string nextcloudPath = fileSystemPath.StartsWith(baseFileSystemPath) ?
                                   fileSystemPath.Substring(baseFileSystemPath.Length).TrimEnd('/') :
                                   throw new InvalidOperationException("Filesystem path does not match expected base path.");

            // Escape and quote the Nextcloud path for the command
            string escapedNextcloudPath = EscapeForBash(nextcloudPath);

            // Construct the command
            string occCommand = $"sudo -u www-data php /var/www/nextcloud/occ files:scan --path='{escapedNextcloudPath}'";

            // Execute the command
            string result = ExecuteBashCommand(occCommand);

            Console.WriteLine(result);
            Console.WriteLine($"Nextcloud Path: {nextcloudPath}");
        }

        static string ExecuteBashCommand(string command)
        {
            try
            {
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c {command}",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return result;
            }
            catch (Exception ex)
            {
                return $"Error executing command: {ex.Message}";
            }
        }

        static string EscapeForBash(string input)
        {
            return input.Replace("'", "'\\''");
        }
    }
}
