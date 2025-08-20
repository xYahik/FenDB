
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

public class PythonService
{
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
        await RunPythonProcess(pipPath, $"install --upgrade pip");
        await RunPythonProcess(pipPath, $"install -r {Path.Combine(pythonServicePath, "requirements.txt --no-deps --ignore-installed")}");

        await RunPythonProcess(pythonPath, Path.Combine(pythonServicePath, "app.py"));
    }

    private async Task RunPythonProcess(string command, string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        Console.CancelKeyPress += (sender, e) =>
        {
            if (!process.HasExited)
                process.Kill();
        };

        //string output = process.StandardOutput.ReadToEnd();
        //string error = process.StandardError.ReadToEnd();
        //process.WaitForExit();

        //if (!string.IsNullOrEmpty(output))
        //    Console.WriteLine(output);
        //if (!string.IsNullOrEmpty(error))
        //    Console.WriteLine(error);
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