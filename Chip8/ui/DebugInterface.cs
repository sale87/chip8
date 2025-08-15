using System.Numerics;
using Chip8.components;
using ImGuiNET;
using Raylib_cs;

namespace Chip8.ui;

public class DebugInterface
{
    // Debug state
    private bool _showDebugWindow = true;
    private bool _showDisplayWindow = true;
    
    public void Render(Display display)
    {
        RenderDebugWindow(display);
        RenderCpuDebugWindow();
        RenderMenuBar();
    }
    
    private void RenderDebugWindow(Display display)
    {
        if (!_showDebugWindow) return;
        ImGui.SetNextWindowPos(new Vector2(600, 50), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(200, 200), ImGuiCond.FirstUseEver);
        ImGui.Begin("CHIP-8 Debug", ref _showDebugWindow);
            
        ImGui.Text($"FPS: {Raylib.GetFPS()}");
        ImGui.Text($"Display: {Display.Width}x{Display.Height}");
            
        ImGui.Separator();
            
        ImGui.ColorEdit3("Pixel Color", ref display.PixelColor);
        ImGui.ColorEdit3("Background Color", ref display.BackgroundColor);
        ImGui.ColorEdit3("Grid Color", ref display.GridColor);
            
        ImGui.Separator();
        ImGui.End();
    }
    
    private static void RenderCpuDebugWindow()
    {
        ImGui.SetNextWindowPos(new Vector2(600, 470), ImGuiCond.Appearing);
        ImGui.SetNextWindowSize(new Vector2(240, 300), ImGuiCond.Appearing);
        ImGui.Begin("CPU Debug");
        
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
        ImGui.End();
    }
    
    private void RenderMenuBar()
    {
        if (!ImGui.BeginMainMenuBar()) return;
        
        if (ImGui.BeginMenu("View"))
        {
            ImGui.MenuItem("Debug Window", "", ref _showDebugWindow);
            ImGui.MenuItem("Display Buffer", "", ref _showDisplayWindow);
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