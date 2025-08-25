using System.Numerics;
using Chip8.components;
using ImGuiNET;
using Raylib_cs;

namespace Chip8.ui;

public class DebugInterface
{
    // ROM metadata structure
    private record RomInfo(string Name, string Path, string Description);
    
    // Debug state
    private bool _showCpuDebug = true;
    private bool _showColorConfig = true;
    private bool _showInstructionsDebug = true;
    private bool _showRomBrowser;
    
    // Memory viewer state
    private int _memoryViewAddress = 0x200; // Start at ROM area
    private const int MemoryViewLines = 32; // Number of lines to show
    private const int BytesPerLine = 16; // Bytes per line
    
    // ROM browser state
    private readonly List<RomInfo> _availableRoms = [];
    private bool _romsScanned;
    
    public void Render(Display display, Cpu cpu, Action reboot, Action exit, Action<string> loadRom)
    {
        RenderCpuDebugWindow(cpu);
        RenderColorConfigWindow(display, reboot, exit);
        RenderInstructionsDebugWindow(cpu);
        RenderDebugControlsWidget(cpu);
        RenderRomBrowserWindow(loadRom);
        RenderMenuBar();
    }
    
    private void RenderCpuDebugWindow(Cpu cpu)
    {
        if (!_showCpuDebug) return;
        
        // Left side, full height
        ImGui.SetNextWindowPos(new Vector2(10, 30), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(300, Raylib.GetScreenHeight() - 40), ImGuiCond.Always);
        ImGui.Begin("CPU Debug", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        
        ImGui.Text($"System Info:");
        ImGui.Text($"FPS: {Raylib.GetFPS()}");
        ImGui.Text($"IPS: {cpu.GetInstructionsPerSecond()}"); // Instructions Per Second
        ImGui.Text($"Display: {Display.Width}x{Display.Height}");
        
        ImGui.Separator();
        
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
        
        // Memory navigation controls
        if (ImGui.Button("ROM", new Vector2(40, 20)))
        {
            _memoryViewAddress = 0x200; // Jump to ROM start
        }
        ImGui.SameLine();
        if (ImGui.Button("PC", new Vector2(30, 20)))
        {
            _memoryViewAddress = cpu.GetProgramCounter() & 0xFFF0; // Jump to PC (aligned)
        }
        ImGui.SameLine();
        if (ImGui.Button("I", new Vector2(25, 20)))
        {
            _memoryViewAddress = cpu.GetIndexRegister() & 0xFFF0; // Jump to I register
        }
        
        // Address input
        var addressInput = _memoryViewAddress;
        if (ImGui.InputInt("Addr", ref addressInput, 16, 256))
        {
            _memoryViewAddress = Math.Max(0, Math.Min(0x1000 - (MemoryViewLines * BytesPerLine), addressInput));
        }
        
        // Memory display - expand to fill remaining space
        ImGui.BeginChild("Memory", new Vector2(0, -1), ImGuiChildFlags.None, ImGuiWindowFlags.None);
        
        if (cpu.IsCpuStateDebuggingEnabled)
        {
            var memoryData = cpu.GetMemoryRange((short)_memoryViewAddress, MemoryViewLines * BytesPerLine);
            
            for (var line = 0; line < MemoryViewLines; line++)
            {
                var baseAddr = _memoryViewAddress + (line * BytesPerLine);
                var hexBytes = new List<string>();
                var asciiChars = new List<char>();
                
                for (var col = 0; col < BytesPerLine; col++)
                {
                    var byteIndex = line * BytesPerLine + col;
                    if (byteIndex < memoryData.Length)
                    {
                        var byteValue = memoryData[byteIndex];
                        hexBytes.Add($"{byteValue:X2}");
                        asciiChars.Add(byteValue is >= 32 and <= 126 ? (char)byteValue : '.');
                    }
                    else
                    {
                        hexBytes.Add("--");
                        asciiChars.Add(' ');
                    }
                }
                
                var hexString = string.Join(" ", hexBytes);
                var asciiString = new string(asciiChars.ToArray());
                
                // Highlight current PC line
                var currentPc = cpu.GetProgramCounter();
                if (currentPc >= baseAddr && currentPc < baseAddr + BytesPerLine)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 1.0f, 0.0f, 1.0f)); // Yellow
                }
                
                ImGui.Text($"{baseAddr:X4}: {hexString} |{asciiString}|");
                
                if (currentPc >= baseAddr && currentPc < baseAddr + BytesPerLine)
                {
                    ImGui.PopStyleColor();
                }
            }
        }
        else
        {
            ImGui.Text("CPU State Monitoring disabled.");
        }
        
        ImGui.EndChild();
        
        ImGui.End();
    }
    
    private void RenderColorConfigWindow(Display display, Action reboot, Action exit)
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
            _showRomBrowser = true;
        }
        ImGui.SameLine();
        if (ImGui.Button("Restart", new Vector2(100, 30)))
        {
            reboot(); // This calls Boot() which reloads the ROM and resets CPU state
        }
        ImGui.SameLine();
        if (ImGui.Button("Exit", new Vector2(100, 30)))
        {
            exit(); // This will close the emulator
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
        
        ImGui.Separator();
        
        ImGui.Text("Instruction History:");
        ImGui.BeginChild("InstructionHistory", new Vector2(0, -1), ImGuiChildFlags.None);
        
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
        
        ImGui.End();
    }
    
    private void RenderDebugControlsWidget(Cpu cpu)
    {
        // Position below the main CHIP-8 display
        const int displayPixelWidth = Display.Width * 10; // DisplayScale = 10
        const int displayPixelHeight = Display.Height * 10;
        var displayX = (Raylib.GetScreenWidth() - displayPixelWidth) / 2;
        var displayY = (Raylib.GetScreenHeight() - displayPixelHeight) / 2;
        
        // Position debug controls below the display
        var debugY = displayY + displayPixelHeight + 20; // 20px gap
        var debugWidth = displayPixelWidth;
        var debugHeight = Raylib.GetScreenHeight() - debugY - 10; // Full height minus margins
        
        ImGui.SetNextWindowPos(new Vector2(displayX, debugY), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(debugWidth, debugHeight), ImGuiCond.Always);
        ImGui.Begin("Debug Controls", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar);
        
        // Debug Toggles
        ImGui.Text("Debug Options:");
        var historyEnabled = cpu.IsInstructionHistoryEnabled;
        if (ImGui.Checkbox("Instruction History##widget", ref historyEnabled))
        {
            cpu.SetInstructionHistoryEnabled(historyEnabled);
        }
        ImGui.SameLine();
        var cpuStateEnabled = cpu.IsCpuStateDebuggingEnabled;
        if (ImGui.Checkbox("CPU State Monitoring##widget", ref cpuStateEnabled))
        {
            cpu.SetCpuStateDebuggingEnabled(cpuStateEnabled);
        }
        
        ImGui.Separator();
        
        // Execution Speed Control
        ImGui.Text("Execution Speed:");
        var currentSpeed = cpu.GetExecutionSpeed();
        var speedSlider = currentSpeed;
        if (ImGui.SliderInt("Hz##speed", ref speedSlider, 1, 1000, $"{currentSpeed} Hz"))
        {
            cpu.SetExecutionSpeed(speedSlider);
        }
        
        // Preset buttons for common speeds
        if (ImGui.Button("60Hz##preset", new Vector2(50, 20)))
        {
            cpu.SetExecutionSpeed(60);
        }
        ImGui.SameLine();
        if (ImGui.Button("500Hz##preset", new Vector2(55, 20)))
        {
            cpu.SetExecutionSpeed(500);
        }
        ImGui.SameLine();
        if (ImGui.Button("1000Hz##preset", new Vector2(60, 20)))
        {
            cpu.SetExecutionSpeed(1000);
        }
        
        ImGui.Separator();
        
        // Execution Controls
        ImGui.Text("Execution:");
        if (cpu.IsRunning)
        {
            if (ImGui.Button("Pause##widget", new Vector2(80, 25)))
            {
                cpu.Pause();
            }
        }
        else
        {
            if (ImGui.Button("Resume##widget", new Vector2(80, 25)))
            {
                cpu.Resume();
            }
        }
        ImGui.SameLine();
        
        // Step button
        if (!cpu.IsRunning)
        {
            if (ImGui.Button("Step##widget", new Vector2(80, 25)))
            {
                cpu.StepOneInstruction();
            }
        }
        else
        {
            ImGui.BeginDisabled();
            ImGui.Button("Step##widget", new Vector2(80, 25));
            ImGui.EndDisabled();
        }
        
        ImGui.End();
    }
    
    private void RenderRomBrowserWindow(Action<string> loadRom)
    {
        if (!_showRomBrowser) return;
        
        // Scan for ROMs if not done yet
        ScanAvailableRoms();
        
        // Position ROM browser in center-right area
        const float windowWidth = 400;
        const float windowHeight = 500;
        var xPos = Raylib.GetScreenWidth() - windowWidth - 320; // Leave space for right panel
        var yPos = (Raylib.GetScreenHeight() - windowHeight) / 2;
        
        ImGui.SetNextWindowPos(new Vector2(xPos, yPos), ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowSize(new Vector2(windowWidth, windowHeight), ImGuiCond.FirstUseEver);
        ImGui.Begin("ROM Browser", ref _showRomBrowser, ImGuiWindowFlags.None);
        
        ImGui.Text("Available ROMs:");
        ImGui.Separator();
        
        ImGui.BeginChild("ROMList", new Vector2(0, -30), ImGuiChildFlags.None);
        
        foreach (var rom in _availableRoms)
        {
            // ROM name as selectable button
            var selected = ImGui.Selectable($"{rom.Name}##rom_{rom.Path}");
            
            // Show description on the same line or next line if it's too long
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.7f, 0.7f, 1.0f)); // Gray color
            ImGui.TextWrapped($"  {rom.Description}");
            ImGui.PopStyleColor();
            
            ImGui.Separator();
            
            if (selected)
            {
                loadRom(rom.Path);
                _showRomBrowser = false; // Close browser after loading
            }
        }
        
        ImGui.EndChild();
        
        ImGui.Separator();
        if (ImGui.Button("Refresh", new Vector2(80, 25)))
        {
            _romsScanned = false; // Force rescan
            ScanAvailableRoms();
        }
        ImGui.SameLine();
        if (ImGui.Button("Close", new Vector2(80, 25)))
        {
            _showRomBrowser = false;
        }
        
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
            ImGui.MenuItem("ROM Browser", "", ref _showRomBrowser);
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
    
    private void ScanAvailableRoms()
    {
        if (_romsScanned) return;
        
        _availableRoms.Clear();
        
        // Scan roms folder for .ch8 files
        const string romsPath = "roms";
        if (Directory.Exists(romsPath))
        {
            ScanRomsDirectory(romsPath);
        }
        
        // Sort ROMs alphabetically by name
        _availableRoms.Sort((rom1, rom2) => string.Compare(rom1.Name, rom2.Name, StringComparison.OrdinalIgnoreCase));
        
        _romsScanned = true;
    }
    
    private void ScanRomsDirectory(string basePath)
    {
        try
        {
            var romFiles = Directory.GetFiles(basePath, "*.ch8", SearchOption.AllDirectories);
            
            foreach (var romFile in romFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(romFile);
                var description = GetRomDescription(fileName);
                _availableRoms.Add(new RomInfo(fileName, romFile, description));
            }
        }
        catch (Exception)
        {
            // Ignore errors when scanning directories
        }
    }
    
    private static string GetRomDescription(string romName)
    {
        return romName.ToLower() switch
        {
            "ibm logo" => "Classic IBM logo demonstration",
            "1-chip8-logo" => "CHIP-8 logo test",
            "2-ibm-logo" => "IBM logo test ROM",
            "3-corax+" => "Corax+ test ROM",
            "4-flags" => "Flag test ROM", 
            "5-quirks" => "Quirks test ROM",
            "6-keypad" => "Keypad test ROM",
            var name when name.Contains("tetris") => "Classic Tetris game",
            var name when name.Contains("pong") => "Pong arcade game",
            var name when name.Contains("snake") => "Snake game",
            var name when name.Contains("breakout") => "Breakout arcade game",
            var name when name.Contains("invaders") => "Space Invaders style game",
            _ => "CHIP-8 ROM"
        };
    }
}