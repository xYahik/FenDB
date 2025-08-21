
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

public class PythonService
{
    private bool inTraceback = false;
    public async Task Initialize()
    {
        await PreparePythonEnv();
        await PreparePythonServer();
    }
    public async Task PreparePythonEnv()
    {
        string pythonServicePath = "Python";
        string venvPath = Path.Combine(pythonServicePath, "venv");
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (!Directory.Exists(venvPath))
        {
            Console.WriteLine("Python enviroment can't be find, creating new one...");
            await RunPythonProcess("python3", $"-m venv {venvPath}");
        }

        string pipPath = isWindows ? Path.Combine(venvPath, "Scripts", "pip.exe") : Path.Combine(venvPath, "bin", "pip");
        string pythonPath = isWindows ? Path.Combine(venvPath, "Scripts", "python.exe") : Path.Combine(venvPath, "bin", "python");

        Console.WriteLine("Installing requirements...");
        await RunPythonProcess(pipPath, $"install --upgrade pip -q", false);
        await RunPythonProcess(pipPath, $"install -r {Path.Combine(pythonServicePath, "requirements.txt -q")}", false);

        await RunPythonProcess(pythonPath, Path.Combine(pythonServicePath, "app.py"));
    }

    private async Task RunPythonProcess(string command, string args, bool redirectOutput = true)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = redirectOutput,
                RedirectStandardError = redirectOutput,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        //Currently don't want log everything from python
        process.OutputDataReceived += (s, e) =>
        {
            //if (!string.IsNullOrEmpty(e.Data))
            //Console.WriteLine("[PYTHON] " + e.Data);
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            //Check if error is traceback
            if (e.Data.StartsWith("Traceback") || e.Data.StartsWith("ERROR:"))
            {
                inTraceback = true;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine("[PYTHON ERROR] " + e.Data);
            }
            else if (inTraceback)
            {
                Console.Error.WriteLine("[PYTHON ERROR] " + e.Data);

                //Way to determine if Python error print ended // Thats always end with line which start with ValueError,NameError,TypeError etc. so we just need check if first part contains Error word.
                if (e.Data.Contains(":") && e.Data.Split(':')[0].EndsWith("Error"))
                {
                    inTraceback = false;
                    Console.ResetColor();
                }
            }
        };

        //Currently don't want log everything from python
        process.Exited += (s, e) =>
        {
            //Console.WriteLine($"Python exited with code {process.ExitCode}");
        };

        process.Start();
        if (redirectOutput)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
        else
        {
            await process.WaitForExitAsync();
        }

        Console.CancelKeyPress += (sender, e) =>
        {
            if (!process.HasExited)
            {
                //Console.WriteLine(process.StandardError.ReadToEnd());
                process.Kill();
            }
        };

    }

    public async Task PreparePythonServer()
    {
        using var client = new HttpClient();

        Console.WriteLine("Waiting for Python Service Server...");
        bool serverReady = false;
        while (!serverReady)
        {
            try
            {
                var res = await client.GetStringAsync("http://127.0.0.1:5000/ready");
                serverReady = res.Contains("true");
            }
            catch
            {
                await Task.Delay(1000);
            }
        }
        Console.WriteLine("Python Service Server is ready!");
    }
}