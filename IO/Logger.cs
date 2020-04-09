using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using static DragonLib.IO.ConsoleSwatch;

namespace DragonLib.IO
{
    [PublicAPI]
    public static class Logger
    {
        public static bool ShowTime = true;
        public static bool ShowThread;
        public static bool ShowLevel;
        public static bool ShowCategory = true;

#if DEBUG
        public static bool ShowDebug = true;
#else
        public static bool ShowDebug;
#endif

        public static bool Enabled = true;
        public static bool UseColor = true;

        public static void Log4Bit(ConsoleColor color, bool newLine, TextWriter writer, string? category, string? level, string? message)
        {
            if (!Enabled) return;

            var (prefix, content) = FormatMessage(category, level, message);
            if (UseColor) Console.ForegroundColor = ConsoleColor.Gray;
            writer.Write(prefix ?? string.Empty);
            if (UseColor) Console.ForegroundColor = color;
            writer.Write(content);
            if (UseColor) Console.ForegroundColor = ConsoleColor.Gray;

            if (newLine) writer.WriteLine();
        }

        private static (string? prefix, string content) FormatMessage(string? category, string? level, string? message)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(category) && ShowTime) parts.Add(DateTimeOffset.UtcNow.ToString("s"));
            if (!string.IsNullOrWhiteSpace(category) && ShowCategory) parts.Add(category);
            if (!string.IsNullOrWhiteSpace(level) && ShowLevel) parts.Add(level);
            if (ShowThread) parts.Add(Thread.CurrentThread.ManagedThreadId.ToString("D"));
            var content = message ?? string.Empty;
            var prefix = default(string);
            if (parts.Count > 0) prefix = string.Join(string.Empty, parts.Select(x => $"[{x}]"));
            return (prefix, content);
        }

        public static void Log(XTermColor color, bool newline, TextWriter writer, string? category, string? level, string message)
        {
            if (!Enabled) return;

            if (!EnableVT())
            {
                Log4Bit(ConsoleColor.Gray, newline, writer, category, level, message);
                return;
            }

            Log(color.ToForeground(), default, newline, writer, category, level, message);
        }

        public static void Log(string? foreground, string? background, bool newLine, TextWriter writer, string? category, string? level, string message)
        {
            if (!Enabled) return;

            if (!EnableVT())
            {
                Log4Bit(ConsoleColor.Gray, newLine, writer, category, level, message);
                return;
            }

            var (prefix, content) = FormatMessage(category, level, message);
            if (!string.IsNullOrWhiteSpace(category) && UseColor && Console.CursorLeft == 0)
            {
                writer.Write(XTermColor.Grey24.ToForeground());
                writer.Write((prefix ?? string.Empty) + " ");
                writer.Write(COLOR_RESET);
                prefix = string.Empty;
            }

            if (UseColor && !string.IsNullOrWhiteSpace(foreground)) writer.Write(foreground);

            if (UseColor && !string.IsNullOrWhiteSpace(background)) writer.Write(background);

            var output = content;
            if (!string.IsNullOrWhiteSpace(prefix)) output = $"{prefix} {content}";
            writer.Write(output);

            if (UseColor && (!string.IsNullOrWhiteSpace(foreground) || !string.IsNullOrWhiteSpace(background))) writer.Write(COLOR_RESET);

            if (newLine) writer.WriteLine();
        }

        public static void Success(string? category, string message) =>
            Log(XTermColor.Green, true, Console.Out, category, "OK", message);

        public static void PrintVersion(string? category, string template = "{0} v{1}", Assembly? asm = null)
        {
            asm ??= Assembly.GetEntryAssembly();
            if (asm == null) return;
            Log(XTermColor.White, true, Console.Error, category, default, string.Format(template, asm.GetName().Name, asm.GetName().Version));
        }

        public static void Info(string? category, string message) =>
            Log(XTermColor.White, true, Console.Out, category, "INFO", message);

        public static void Debug(string? category, string message)
        {
            if (!ShowDebug) return;

            Log(XTermColor.Grey, true, Console.Out, category, "DEBUG", message);
        }

        public static void Warn(string? category, string message) =>
            Log(XTermColor.LightYellow, true, Console.Error, category, "WARN", message);

        public static void Error(string? category, string message) =>
            Log(XTermColor.Red, true, Console.Out, category, "ERROR", message);


        public static void Error(string? category, string message, Exception e)
        {
            Log(XTermColor.Red, true, Console.Out, category, "ERROR", message);
            Log(XTermColor.Red, true, Console.Out, category, "ERROR", e.ToString());
        }

        public static void Error(string? category, Exception e) => Error(category, e.ToString());

        public static void Fatal(string? category, string message) =>
            Log(XTermColor.Red, true, Console.Out, category, "FATAL", message);

        public static void Fatal(string? category, Exception e) => Error(category, e.ToString());

        public static void ResetColor(TextWriter writer)
        {
            if (!EnableVT())
                Console.ResetColor();
            else
                writer.Write(COLOR_RESET);
        }

        public static string ReadLine(TextWriter writer, bool @private)
        {
            var builder = new StringBuilder();
            ConsoleKeyInfo ch;
            while ((ch = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (ch.Key == ConsoleKey.Backspace) // backspace
                {
                    if (builder.Length > 0)
                    {
                        if (!@private)
                        {
                            writer.Write(ch.KeyChar);
                            writer.Write(" ");
                            writer.Write(ch.KeyChar);
                        }

                        builder.Remove(builder.Length - 1, 1);
                    }
                    else
                    {
                        Console.Beep();
                    }
                }
                else
                {
                    builder.Append(ch.KeyChar);

                    if (!@private) writer.Write(ch.KeyChar);
                }
            }

            writer.WriteLine();
            return builder.ToString();
        }

        private static string? LastMessage;

        public static void LogProgress(string message, string pre, string post, double value, XTermColor messageColor, XTermColor preColor, XTermColor postColor, XTermColor brickColor, XTermColor processColor, bool showProgressValue, XTermColor processValueColor)
        {
            if (Console.IsOutputRedirected) return;

            var width = Console.WindowWidth;
            var empty = new char[width];
            Array.Fill(empty, ' ');
            if (message != LastMessage)
            {
                Console.Out.Write(empty);
                Console.CursorLeft = 0;
                LastMessage = message;
                Log(messageColor, true, Console.Out, default, default, message);
            }

            Console.Out.Write(empty);
            Console.CursorLeft = 0;
            var remaining = width - pre.Length - post.Length - 4;
            Log(preColor, false, Console.Out, default, default, pre);
            if (remaining > 0)
            {
                Log(brickColor, false, Console.Out, default, default, " [");
                empty = new char[remaining];
                Array.Fill(empty, ' ');
                Array.Fill(empty, '=', 0, (int) Math.Round(remaining * Math.Min(value, 1)));
                Log(processColor, false, Console.Out, default, default, string.Join(string.Empty, empty));
                Log(brickColor, false, Console.Out, default, default, "] ");

                if (showProgressValue && remaining > 6)
                {
                    var valueText = (Math.Min(value, 1) * 100).ToString(CultureInfo.InvariantCulture).Split('.')[0] + "%";
                    Console.CursorLeft = pre.Length + 2 + (int) Math.Floor(remaining / 2.0d - valueText.Length / 2.0d);
                    Log(processValueColor, false, Console.Out, default, default, valueText);
                    Console.CursorLeft = width - post.Length;
                }
            }

            Log(postColor, false, Console.Out, default, default, post);
            Console.CursorLeft = Console.WindowWidth - 1;
            Console.CursorTop -= 1;
        }

        public static void FlushProgress()
        {
            Console.CursorTop += 1;
            Console.WriteLine();
        }

        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static bool Assert(bool condition, string? message = null, params string[] detail)
        {
            if (condition) return false;
            var trace = new StackTrace(1, true);
            var frame = trace.GetFrame(0);

            Log(XTermColor.Purple, true, Console.Error, default, "ASSERT", $"Assertion failed at {frame?.ToString().Trim() ?? "unknown location"}");

            if (message != null)
                Log(XTermColor.Purple, true, Console.Error, default, default, "\t -> " + message);

            if (!(detail?.Length > 0)) return true;
            foreach (var line in detail) Log(XTermColor.Purple, true, Console.Error, default, default, "\t -> " + line);

            return true;
        }

        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static void Trace()
        {
            if (!ShowDebug) return;
            var trace = new StackTrace(1, true);

            Log(XTermColor.HotPink3, true, Console.Error, default, "TRACE", trace.ToString().Trim());
        }
    }
}
