using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class TerminalWindow : ITabWindow
    {
        public string Name => "Terminal";

        public string IconName => Icon.Get(Icon.Material_terminal);

        public bool Unsave => false;

        public bool ExitModal { get; set; }

        private string _rootPath = "";
        private string _userName;
        private string _userDomain;

        private string _cdPath = "";
        private string _command = "";

        private Queue<string> _history = new Queue<string>();
        public TerminalWindow()
        {
            _rootPath = Environment.GetEnvironmentVariable("USERPROFILE");
            if (string.IsNullOrEmpty(_rootPath))
            {
                _rootPath = "./";
            };
            _cdPath = _rootPath = Path.GetFullPath(_rootPath);
            _userName = Environment.GetEnvironmentVariable("USERNAME");
            _userDomain = Environment.GetEnvironmentVariable("USERDOMAIN");
        }

        public bool Deserialize(string data)
        {
            return false;
        }

        public void Dispose()
        {

        }

        public void OnDraw(bool canInput = true)
        {
            foreach (var item in _history)
            {
                if (item == null)
                    continue;

                ImGui.Text(item);
            }

            string cdPath = _cdPath.Equals(_rootPath) ? "~" : _cdPath;
            ImGui.Text($"{_userName}@{_userDomain} {cdPath}");
            if (ImGui.InputText("cmd", ref _command, 200))
            {

            }
            if (ImGui.Button("ook"))
            {
                if (!string.IsNullOrEmpty(_command))
                {
                    RunProcess(_command);
                    _command = "";
                }
            }
        }

        public void OnExitModalSure()
        {

        }

        public string Serialize()
        {
            return null;
        }

        public void Setup(params object[] parameters)
        {

        }


        private void RunProcess(string command)
        {
            try
            {
                if (!string.IsNullOrEmpty(command))
                {
                    _history.Enqueue(command);

                    int index = command.IndexOf(' ');
                    string cmd = command;
                    string arguments = "";
                    if (index > 0)
                    {
                        cmd = command.Substring(0, index);
                        arguments = command.Substring(index, command.Length - index).Trim();
                    }

                    Console.WriteLine("Console.WriteLine");

                    var cmdProcess = new Process();
                    var startInfo = new ProcessStartInfo();
                    //startInfo.WorkingDirectory = @"E:\source\temp\godot";
                    startInfo.FileName = cmd;
                    startInfo.Arguments = arguments;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardError = true;
                    cmdProcess.StartInfo = startInfo;
                    cmdProcess.OutputDataReceived += (sender, e) =>
                    {
                        _history.Enqueue(e.Data);
                        //Console.WriteLine($"OutputDataReceived: {e.Data}");
                    };
                    cmdProcess.ErrorDataReceived += (sender, e) =>
                    {
                        _history.Enqueue(e.Data);

                        //Console.WriteLine($"ErrorDataReceived: {e.Data}");
                    };
                    _history.Enqueue($"WorkingDirectory: {startInfo.WorkingDirectory}");
                    cmdProcess.Start();
                    cmdProcess.BeginOutputReadLine();
                    cmdProcess.BeginErrorReadLine();

                    cmdProcess.WaitForExit();
                    Console.WriteLine("Console.WriteLine#");
                    //cmdProcess.StandardInput.WriteLine("git --help");
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
