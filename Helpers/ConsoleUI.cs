namespace HospitalApp.Helpers
{
  
    public static class ConsoleUI
    {
        // ── Theme Colors ────────────────────────────────────────────────────────
        public static readonly ConsoleColor Accent   = ConsoleColor.Cyan;
        public static readonly ConsoleColor Success  = ConsoleColor.Green;
        public static readonly ConsoleColor Warning  = ConsoleColor.Yellow;
        public static readonly ConsoleColor Error    = ConsoleColor.Red;
        public static readonly ConsoleColor Muted    = ConsoleColor.DarkGray;
        public static readonly ConsoleColor Normal   = ConsoleColor.White;
        public static readonly ConsoleColor Heading  = ConsoleColor.Magenta;

        private const int ConsoleWidth = 72;

        // ── Basic Output ────────────────────────────────────────────────────────
        public static void Write(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        public static void WriteLine(string text = "", ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void WriteSuccess(string msg)  => WriteLine($"  [OK]  {msg}", Success);
        public static void WriteError(string msg)    => WriteLine($"  [!!]  {msg}", Error);
        public static void WriteWarning(string msg)  => WriteLine($"  [**]  {msg}", Warning);
        public static void WriteInfo(string msg)     => WriteLine($"  [--]  {msg}", Accent);
        public static void WriteMuted(string msg)    => WriteLine($"        {msg}", Muted);

        // ── Dividers ────────────────────────────────────────────────────────────
        public static void Line(char c = '─')    => WriteLine(new string(c, ConsoleWidth), Muted);
        public static void DoubleLine()          => WriteLine(new string('═', ConsoleWidth), Accent);
        public static void BlankLine()           => Console.WriteLine();

        // ── Headers ─────────────────────────────────────────────────────────────
        public static void Banner()
        {
            Console.Clear();
            DoubleLine();
            PrintCentered("  ██╗  ██╗ ██████╗ ███████╗██████╗ ██╗████████╗ █████╗ ██╗     ", Accent);
            PrintCentered("  ██║  ██║██╔═══██╗██╔════╝██╔══██╗██║╚══██╔══╝██╔══██╗██║     ", Accent);
            PrintCentered("  ███████║██║   ██║███████╗██████╔╝██║   ██║   ███████║██║     ", Accent);
            PrintCentered("  ██╔══██║██║   ██║╚════██║██╔═══╝ ██║   ██║   ██╔══██║██║     ", Accent);
            PrintCentered("  ██║  ██║╚██████╔╝███████║██║     ██║   ██║   ██║  ██║███████╗", Accent);
            PrintCentered("  ╚═╝  ╚═╝ ╚═════╝ ╚══════╝╚═╝     ╚═╝   ╚═╝   ╚═╝  ╚═╝╚══════╝", Accent);
            BlankLine();
            PrintCentered("H O S P I T A L   M A N A G E M E N T   S Y S T E M", Normal);
            PrintCentered("Baku Clinical Hospital  ·  v2.0  ·  Senior Edition", Muted);
            DoubleLine();
            BlankLine();
        }

        public static void SectionHeader(string title, string subtitle = "")
        {
            BlankLine();
            DoubleLine();
            PrintCentered(title.ToUpper(), Heading);
            if (!string.IsNullOrEmpty(subtitle))
                PrintCentered(subtitle, Muted);
            DoubleLine();
            BlankLine();
        }

        public static void SubHeader(string title)
        {
            BlankLine();
            Line('─');
            WriteLine($"  >> {title}", Accent);
            Line('─');
        }

        // ── Centered Text ────────────────────────────────────────────────────────
        public static void PrintCentered(string text, ConsoleColor color = ConsoleColor.White)
        {
            int padding = Math.Max(0, (ConsoleWidth - text.Length) / 2);
            Console.ForegroundColor = color;
            Console.WriteLine(new string(' ', padding) + text);
            Console.ResetColor();
        }

        // ── Menu Renderer ────────────────────────────────────────────────────────
        public static void Menu(string title, params (string key, string label)[] items)
        {
            SubHeader(title);
            foreach (var (key, label) in items)
            {
                Write($"    [{key}]", Accent);
                WriteLine($"  {label}");
            }
            BlankLine();
            Write("  Your choice: ", Muted);
        }

        // ── Table Renderer ───────────────────────────────────────────────────────
        public static void Table<T>(IEnumerable<T> items, string emptyMsg = "No records found.")
        {
            var list = items.ToList();
            if (!list.Any())
            {
                WriteWarning(emptyMsg);
                return;
            }

            Line();
            int i = 1;
            foreach (var item in list)
            {
                Write($"  {i++,3}. ", Muted);
                WriteLine(item?.ToString() ?? "null");
            }
            Line();
            WriteMuted($"Total: {list.Count} record(s).");
        }

        // ── Status Badge ─────────────────────────────────────────────────────────
        public static void StatusBadge(string label, string value, ConsoleColor valueColor = ConsoleColor.White)
        {
            Write($"  {label,-20}: ", Muted);
            WriteLine(value, valueColor);
        }

        // ── Box / Panel ──────────────────────────────────────────────────────────
        public static void Box(string content, ConsoleColor borderColor = ConsoleColor.DarkGray)
        {
            var lines = content.Split('\n');
            int width = Math.Max(lines.Max(l => l.TrimEnd().Length) + 4, 40);
            WriteLine("  ╔" + new string('═', width) + "╗", borderColor);
            foreach (var line in lines)
                WriteLine("  ║  " + line.TrimEnd().PadRight(width - 2) + "  ║", borderColor);
            WriteLine("  ╚" + new string('═', width) + "╝", borderColor);
        }

        // ── Progress Bar ─────────────────────────────────────────────────────────
        public static string ProgressBar(int current, int total, int width = 20)
        {
            if (total == 0) return "[" + new string('-', width) + "] N/A";
            int filled = (int)((double)current / total * width);
            filled = Math.Clamp(filled, 0, width);
            double pct = (double)current / total * 100;
            return "[" + new string('█', filled) + new string('─', width - filled) + $"] {pct:F0}%";
        }

        // ── Prompts ──────────────────────────────────────────────────────────────
        public static string Prompt(string label)
        {
            Write($"  {label}: ", Accent);
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        public static int PromptInt(string label, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Write($"  {label}: ", Accent);
                if (int.TryParse(Console.ReadLine(), out int val) && val >= min && val <= max)
                    return val;
                WriteError($"Please enter a valid integer ({min}–{max}).");
            }
        }

        public static decimal PromptDecimal(string label)
        {
            while (true)
            {
                Write($"  {label}: ", Accent);
                if (decimal.TryParse(Console.ReadLine(), out decimal val) && val >= 0)
                    return val;
                WriteError("Please enter a valid positive number.");
            }
        }

        public static DateTime PromptDateTime(string label)
        {
            while (true)
            {
                Write($"  {label} (dd/MM/yyyy HH:mm): ", Accent);
                if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy HH:mm",
                    null, System.Globalization.DateTimeStyles.None, out DateTime dt))
                    return dt;
                WriteError("Invalid format. Use: dd/MM/yyyy HH:mm");
            }
        }

        public static bool Confirm(string question)
        {
            Write($"  {question} [y/N]: ", Warning);
            return (Console.ReadLine()?.Trim().ToLower()) == "y";
        }

        // ── Wait / Pause ─────────────────────────────────────────────────────────
        public static void PressAnyKey(string msg = "Press any key to continue...")
        {
            BlankLine();
            Write($"  {msg}", Muted);
            Console.ReadKey(true);
            BlankLine();
        }

        public static void Spinner(string message, int milliseconds = 800)
        {
            char[] frames = { '|', '/', '-', '\\' };
            int steps = milliseconds / 80;
            for (int i = 0; i < steps; i++)
            {
                Write($"\r  {frames[i % 4]}  {message}", Muted);
                System.Threading.Thread.Sleep(80);
            }
            Console.Write("\r" + new string(' ', 60) + "\r");
        }

        // ── Notification ─────────────────────────────────────────────────────────
        public static void Notify(string msg, ConsoleColor color)
        {
            BlankLine();
            Line('·');
            PrintCentered(msg, color);
            Line('·');
            BlankLine();
        }
    }
}
