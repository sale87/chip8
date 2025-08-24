using System.Numerics;
using Chip8.components;
using ImGuiNET;
using Raylib_cs;

namespace Chip8.ui;

public class DebugInterface
{
    // Debug state
    private bool _showCpuDebug = true;
    private bool _showColorConfig = true;
    private bool _showInstructionsDebug = true;
    
    public void Render(Display display, Cpu cpu, Action reboot)
    {
        RenderCpuDebugWindow(cpu);
        RenderColorConfigWindow(display, cpu, reboot);
        RenderInstructionsDebugWindow(cpu);
        RenderMenuBar();
    }
    
    private void RenderCpuDebugWindow(Cpu cpu)
    {
        if (!_showCpuDebug) return;
        
        // Left side, full height
        ImGui.SetNextWindowPos(new Vector2(10, 30), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(300, Raylib.GetScreenHeight() - 40), ImGuiCond.Always);
        ImGui.Begin("CPU Debug", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        
        ImGui.Text("CPU Registers:");
        if (cpu.IsCpuStateDebuggingEnabled)
        {
            var registers = cpu.GetRegisters();
            ImGui.Text($"V0: 0x{registers[0]:X2}  V1: 0x{registers[1]:X2}  V2: 0x{registers[2]:X2}  V3: 0x{registers[3]:X2}");
            ImGui.Text($"V4: 0x{registers[4]:X2}  V5: 0x{registers[5]:X2}  V6: 0x{registers[6]:X2}  V7: 0x{registers[7]:X2}");
            ImGui.Text($"V8: 0x{registers[8]:X2}  V9: 0x{registers[9]:X2}  VA: 0x{registers[10]:X2}  VB: 0x{registers[11]:X2}");
            ImGui.Text($"VC: 0x{registers[12]:X2}  VD: 0x{registers[13]:X2}  VE: 0x{registers[14]:X2}  VF: 0x{registers[15]:X2}");
        }
        else
        {
            ImGui.Text("V0: --    V1: --    V2: --    V3: --");
            ImGui.Text("V4: --    V5: --    V6: --    V7: --");
            ImGui.Text("V8: --    V9: --    VA: --    VB: --");
            ImGui.Text("VC: --    VD: --    VE: --    VF: --");
        }
        
        ImGui.Separator();
        
        ImGui.Text("Special Registers:");
        if (cpu.IsCpuStateDebuggingEnabled)
        {
            ImGui.Text($"PC: 0x{cpu.GetProgramCounter():X4}");
            ImGui.Text($"I:  0x{cpu.GetIndexRegister():X4}");
            ImGui.Text($"SP: 0x{cpu.GetStackPointer():X2}");
        }
        else
        {
            ImGui.Text("PC: ----");
            ImGui.Text("I:  ----");
            ImGui.Text("SP: --");
        }
        
        ImGui.Separator();
        
        ImGui.Text("Timers:");
        if (cpu.IsCpuStateDebuggingEnabled)
        {
            ImGui.Text($"Delay: {cpu.GetDelayTimer()}");
            ImGui.Text("Sound: 0"); // Sound timer not implemented in current CPU
        }
        else
        {
            ImGui.Text("Delay: --");
            ImGui.Text("Sound: --");
        }
        
        ImGui.Separator();
        
        ImGui.Text("Memory:");
        ImGui.Text("ROM Start: 0x0200");
        
        ImGui.Separator();
        
        ImGui.Text($"System Info:");
        ImGui.Text($"FPS: {Raylib.GetFPS()}");
        ImGui.Text($"Display: {Display.Width}x{Display.Height}");
        
        ImGui.End();
    }
    
    private void RenderColorConfigWindow(Display display, Cpu cpu, Action reboot)
    {
        if (!_showColorConfig) return;
        
        // Top middle - positioned between left and right panels
        float windowWidth = Raylib.GetScreenWidth() - 640; // Screen width minus left (300) and right (300) and margins (40)
        const float xPos = 320; // Left panel (300) + margin (20)
        ImGui.SetNextWindowPos(new Vector2(xPos, 30), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(windowWidth, 250), ImGuiCond.Always);
        ImGui.Begin("Display Colors", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        
        ImGui.Text("Display Color Configuration:");
        ImGui.Separator();
        
        ImGui.ColorEdit3("Pixel Color", ref display.PixelColor);
        ImGui.ColorEdit3("Background Color", ref display.BackgroundColor);
        ImGui.ColorEdit3("Grid Color", ref display.GridColor);
        
        ImGui.Separator();
        
        ImGui.Text("Emulator Controls:");
        if (ImGui.Button("Load ROM", new Vector2(100, 30)))
        {
            // TODO: Implement ROM loading
        }
        ImGui.SameLine();
        if (ImGui.Button("Reset", new Vector2(100, 30)))
        {
            reboot();
        }
        ImGui.SameLine();
        if (ImGui.Button("Exit", new Vector2(100, 30)))
        {
            // TODO: Implement exit
        }
        
        ImGui.Separator();
        
        ImGui.Text("Debug Controls:");
        var historyEnabled = cpu.IsInstructionHistoryEnabled;
        if (ImGui.Checkbox("Instruction History", ref historyEnabled))
        {
            cpu.SetInstructionHistoryEnabled(historyEnabled);
        }
        
        var cpuStateEnabled = cpu.IsCpuStateDebuggingEnabled;
        if (ImGui.Checkbox("CPU State Monitoring", ref cpuStateEnabled))
        {
            cpu.SetCpuStateDebuggingEnabled(cpuStateEnabled);
        }
        
        ImGui.End();
    }
    
    private static void RenderInstructionsDebugWindow(Cpu cpu)
    {
        // Right side, same width as left panel, full height
        const float windowWidth = 300;
        var xPos = Raylib.GetScreenWidth() - windowWidth - 10;
        ImGui.SetNextWindowPos(new Vector2(xPos, 30), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(windowWidth, Raylib.GetScreenHeight() - 40), ImGuiCond.Always);
        ImGui.Begin("Instructions Debug", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        
        ImGui.Text("Current Instruction:");
        ImGui.Text(cpu.GetCurrentInstruction());
        
        ImGui.Separator();
        
        ImGui.Text("Instruction History:");
        ImGui.BeginChild("InstructionHistory", new Vector2(0, 200), ImGuiChildFlags.None);
        
        if (cpu.IsInstructionHistoryEnabled)
        {
            var history = cpu.GetInstructionHistory();
            if (history.Count == 0)
            {
                ImGui.Text("No instructions executed yet...");
            }
            else
            {
                // Display instructions in reverse order (newest first)
                for (var i = history.Count - 1; i >= 0; i--)
                {
                    ImGui.Text(history[i]);
                }
            }
        }
        else
        {
            ImGui.Text("Instruction history is disabled.");
            ImGui.Text("Enable it using the checkbox in");
            ImGui.Text("the Display Colors window.");
        }
        
        ImGui.EndChild();
        
        ImGui.Separator();
        
        ImGui.Text("Stack:");
        ImGui.BeginChild("Stack", new Vector2(0, 150), ImGuiChildFlags.None);
        
        if (cpu.IsCpuStateDebuggingEnabled)
        {
            var stackContents = cpu.GetStackContents();
            if (stackContents.Length == 0)
            {
                ImGui.Text("Stack empty");
            }
            else
            {
                for (var i = 0; i < stackContents.Length; i++)
                {
                    ImGui.Text($"[{i}]: 0x{stackContents[i]:X4}");
                }
            }
        }
        else
        {
            ImGui.Text("CPU State Monitoring disabled.");
            ImGui.Text("Enable it using the checkbox in");
            ImGui.Text("the Display Colors window.");
        }
        
        ImGui.EndChild();
        
        ImGui.End();
    }
    
    private void RenderMenuBar()
    {
        if (!ImGui.BeginMainMenuBar()) return;
        
        if (ImGui.BeginMenu("View"))
        {
            ImGui.MenuItem("CPU Debug", "", ref _showCpuDebug);
            ImGui.MenuItem("Color Config", "", ref _showColorConfig);
            ImGui.MenuItem("Instructions Debug", "", ref _showInstructionsDebug);
            ImGui.EndMenu();
        }
            
        if (ImGui.BeginMenu("Emulator"))
        {
            ImGui.MenuItem("Load ROM...");
            ImGui.MenuItem("Reset");
            ImGui.Separator();
            ImGui.MenuItem("Exit");
            ImGui.EndMenu();
        }
            
        ImGui.EndMainMenuBar();
    }
}