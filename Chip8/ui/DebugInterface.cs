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
    
    public void Render(Display display, Action reboot)
    {
        RenderCpuDebugWindow();
        RenderColorConfigWindow(display, reboot);
        RenderInstructionsDebugWindow();
        RenderMenuBar();
    }
    
    private void RenderCpuDebugWindow()
    {
        if (!_showCpuDebug) return;
        
        // Left side, full height
        ImGui.SetNextWindowPos(new Vector2(10, 30), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(300, Raylib.GetScreenHeight() - 40), ImGuiCond.Always);
        ImGui.Begin("CPU Debug", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        
        ImGui.Text("CPU Registers:");
        ImGui.Text("V0: 0x00  V1: 0x00  V2: 0x00  V3: 0x00");
        ImGui.Text("V4: 0x00  V5: 0x00  V6: 0x00  V7: 0x00");
        ImGui.Text("V8: 0x00  V9: 0x00  VA: 0x00  VB: 0x00");
        ImGui.Text("VC: 0x00  VD: 0x00  VE: 0x00  VF: 0x00");
        
        ImGui.Separator();
        
        ImGui.Text("Special Registers:");
        ImGui.Text("PC: 0x0200");
        ImGui.Text("I:  0x0000");
        ImGui.Text("SP: 0x00");
        
        ImGui.Separator();
        
        ImGui.Text("Timers:");
        ImGui.Text("Delay: 0");
        ImGui.Text("Sound: 0");
        
        ImGui.Separator();
        
        ImGui.Text("Memory:");
        ImGui.Text("ROM Start: 0x0200");
        
        ImGui.Separator();
        
        ImGui.Text($"System Info:");
        ImGui.Text($"FPS: {Raylib.GetFPS()}");
        ImGui.Text($"Display: {Display.Width}x{Display.Height}");
        
        ImGui.End();
    }
    
    private void RenderColorConfigWindow(Display display, Action reboot)
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
        
        ImGui.End();
    }
    
    private static void RenderInstructionsDebugWindow()
    {
        // Right side, same width as left panel, full height
        const float windowWidth = 300;
        var xPos = Raylib.GetScreenWidth() - windowWidth - 10;
        ImGui.SetNextWindowPos(new Vector2(xPos, 30), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(windowWidth, Raylib.GetScreenHeight() - 40), ImGuiCond.Always);
        ImGui.Begin("Instructions Debug", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        
        ImGui.Text("Current Instruction:");
        ImGui.Text("0x0000: NOP");
        
        ImGui.Separator();
        
        ImGui.Text("Instruction History:");
        ImGui.BeginChild("InstructionHistory", new Vector2(0, 200), ImGuiChildFlags.None);
        
        // Placeholder instruction history
        for (var i = 0; i < 20; i++)
        {
            ImGui.Text($"0x{(0x200 + i * 2):X4}: SAMPLE INSTRUCTION {i}");
        }
        
        ImGui.EndChild();
        
        ImGui.Separator();
        
        ImGui.Text("Stack:");
        ImGui.BeginChild("Stack", new Vector2(0, 150), ImGuiChildFlags.None);
        
        ImGui.Text("Stack empty");
        
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