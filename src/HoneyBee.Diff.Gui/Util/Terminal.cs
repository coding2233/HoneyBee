using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyBee.Diff.Gui
{
    public class Terminal
    {
        private Queue<string> _history = new Queue<string>();
        private static Terminal s_terminal;
        private static Terminal self
        {
            get
            {
                if (s_terminal == null)
                {
                    s_terminal = new Terminal();
                }
                return s_terminal;
            }
        }

        private static bool Show=false;
        private static string m_workDirectory = null;

        private Process m_cmdProcess;
        private string m_command="";
        private TextEditor _terminalShowText = new TextEditor();


        public static void SetShow(string workDirectory = null)
        {
            m_workDirectory = workDirectory;
            Show = true;
        }

        public static void Pull(string gitPath,Action onComplete=null)
        {
            self.RunProcess("git pull",gitPath);
            Show = true;
        }

        public static void Fetch(string gitPath, string remote, string remoteBranch, string localBranch)
        {
            self.RunProcess($"git fetch {remote} {remoteBranch}:{localBranch}", gitPath);
            Show = true;
        }

        public static void Push(string gitPath,string remote,  string localBranch, string remoteBranch, bool force=false)
        {
            string cmd = force ? "git push -f":"git push";
            self.RunProcess($"{cmd} {remote} {localBranch}:{remoteBranch}", gitPath);
            Show = true;
        }

        internal static void Draw()
        {
            if (Show)
            {
                self.DrawTerminal();
            }
            else
            {
                m_workDirectory = null;
            }
        }

        private void DrawTerminal()
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(600, 400));
            ImGui.SetNextWindowFocus();
            ImGui.PushID("GlobalTerminal");
            bool showContent = ImGui.Begin("Terminal",ref Show);
            ImGui.PopID();
            if (showContent)
            {
                if (m_cmdProcess == null)
                {
                    ImGui.InputText("", ref m_command, 200);
                    ImGui.SameLine();
                    //Enter
                    if (ImGui.IsKeyDown((int)ImGuiKey.Enter) || ImGui.Button("Exec"))
                    {
                        if (!string.IsNullOrEmpty(m_command))
                        {
                            RunProcess(m_command);
                            m_command = "";
                        }
                    }
                }

                ImGui.BeginChild("Terminal Text Window");
                _terminalShowText.Render("Terminal Text",ImGui.GetWindowSize());
                ImGui.EndChild();

                //foreach (var item in _history)
                //{
                //    ImGui.Text(item);
                //}
                
            }
            else
            {
                //Console.WriteLine("Terminalxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
            }
            ImGui.End();
        }

        private void RunProcess(string command,string workDirectory=null)
        {
            if (m_cmdProcess != null)
            {
                AddRenderText("Other commands are being executed");
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(command))
                {
                    int index = command.IndexOf(' ');
                    string cmd = command;
                    string arguments = "";
                    if (index > 0)
                    {
                        cmd = command.Substring(0, index);
                        arguments = command.Substring(index, command.Length - index).Trim();
                    }

                    Console.WriteLine("Console.WriteLine");

                    m_cmdProcess = new Process();
                    var startInfo = new ProcessStartInfo();
                    string wd = string.IsNullOrEmpty(workDirectory) ? m_workDirectory : workDirectory;
                    if (!string.IsNullOrEmpty(wd))
                    {
                        m_workDirectory = wd;
                        startInfo.WorkingDirectory = wd;
                    }
                    startInfo.FileName = cmd;
                    startInfo.Arguments = arguments;
                    startInfo.UseShellExecute = false;
                    startInfo.CreateNoWindow = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardError = true;
                    m_cmdProcess.StartInfo = startInfo;
                    m_cmdProcess.OutputDataReceived += (sender, e) =>
                    {
                        AddRenderText(e.Data);
                    };
                    m_cmdProcess.ErrorDataReceived += (sender, e) =>
                    {
                        AddRenderText(e.Data);
                    };
                    AddRenderText($"WorkingDirectory: {startInfo.WorkingDirectory}");
                    AddRenderText(command);
                    Task.Run(()=> {
                        m_cmdProcess.Start();
                        m_cmdProcess.BeginOutputReadLine();
                        m_cmdProcess.BeginErrorReadLine();
                        m_cmdProcess.WaitForExit();
                        m_cmdProcess.CancelErrorRead();
                        m_cmdProcess.CancelOutputRead();
                        m_cmdProcess.Dispose();
                        m_cmdProcess = null;
                        RunProcessComplete(null);
                        Console.WriteLine("Console.WriteLine#");
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                m_cmdProcess.Dispose();
                m_cmdProcess = null;
                RunProcessComplete(e.Message);
            }

        }

        private void AddRenderText(string line)
        {
            if (line == null)
                return;
            _history.Enqueue(line);
            _terminalShowText.text += $"{line}\n";
        }

        private void RunProcessComplete(string error)
        {
            if(!string.IsNullOrEmpty(error))
                AddRenderText(error);
        }

    }
}
