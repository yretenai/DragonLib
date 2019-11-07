using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using static DragonLib.IO.ConsoleSwatch;

namespace DragonLib.IO
{
    // ReSharper disable once UnusedType.Global
    public static class Logger
    {
        public static bool ShowTime = false;

#if DEBUG
        public static bool ShowDebug = true;
#else
        public static bool ShowDebug = false;
#endif

        public static bool Enabled = true;
        public static bool UseColor = true;

        public static void Log4Bit(ConsoleColor color, bool newLine, TextWriter writer, string category, string message, params object[] arg)
        {
            if (!Enabled) return;

            if (UseColor) Console.ForegroundColor = color;

            var output = message;

            if (arg.Length > 0) output = string.Format(message, arg);

            if (!string.IsNullOrWhiteSpace(category)) output = $"[{category}] {output}";

            if (ShowTime) output = $"{DateTime.Now.ToLocalTime().ToLongTimeString()} {output}";

            writer.Write(output);

            if (UseColor) Console.ForegroundColor = ConsoleColor.Gray;

            if (newLine) writer.WriteLine();
        }

        public static void Log24Bit(ConsoleColor color, bool newline, TextWriter writer, string category, string message, params object[] arg)
        {
            if (!Enabled) return;

            if (!EnableVT())
            {
                Log4Bit(color, newline, writer, category, message, arg);
                return;
            }

            Log24Bit(color.AsDOSColor().AsXTermColor().ToForeground(), default, newline, writer, category, message, arg);
        }

        public static void Log24Bit(DOSColor color, bool newline, TextWriter writer, string category, string message, params object[] arg)
        {
            if (!Enabled) return;

            if (!EnableVT())
            {
                Log4Bit(color.AsConsoleColor(), newline, writer, category, message, arg);
                return;
            }

            Log24Bit(color.AsXTermColor().ToForeground(), default, newline, writer, category, message, arg);
        }

        public static void Log24Bit(XTermColor color, bool newline, TextWriter writer, string category, string message, params object[] arg)
        {
            if (!Enabled) return;

            if (!EnableVT())
            {
                Log4Bit(ConsoleColor.Gray, newline, writer, category, message, arg);
                return;
            }

            Log24Bit(color.ToForeground(), default, newline, writer, category, message, arg);
        }

        public static void Log24Bit(string foreground, string background, bool newLine, TextWriter writer, string category, string message, params object[] arg)
        {
            if (!Enabled) return;

            if (!EnableVT())
            {
                Log4Bit(ConsoleColor.Gray, newLine, writer, category, message, arg);
                return;
            }

            if (!string.IsNullOrWhiteSpace(category) && UseColor && Console.CursorLeft == 0)
            {
                writer.Write(XTermColor.Grey24.ToForeground());
                writer.Write($"[{category}] ");
                writer.Write(ColorReset);
            }

            if (UseColor && !string.IsNullOrWhiteSpace(foreground)) writer.Write(foreground);

            if (UseColor && !string.IsNullOrWhiteSpace(background)) writer.Write(background);

            var output = message;

            if (arg.Length > 0) output = string.Format(message, arg);

            if (!string.IsNullOrWhiteSpace(category) && !UseColor && Console.CursorLeft == 0) output = $"[{category}] {output}";

            if (ShowTime) output = $"{DateTime.Now.ToLocalTime().ToLongTimeString()} {output}";

            writer.Write(output);

            if (UseColor && (!string.IsNullOrWhiteSpace(foreground) || !string.IsNullOrWhiteSpace(background))) writer.Write(ColorReset);

            if (newLine) writer.WriteLine();
        }

        public static void Log(ConsoleColor color, bool newline, bool stderr, string category, string message, params object[] arg) => Log24Bit(color, newline, stderr ? Console.Error : Console.Out, category, message, arg);

        public static void Success(string category, string message, params object[] arg) => Log(ConsoleColor.Green, true, false, category, message, arg);

        public static void Info(string category, string message, params object[] arg) => Log(ConsoleColor.White, true, false, category, message, arg);

        public static void Debug(string category, string message, params object[] arg)
        {
            if (!ShowDebug) return;

            Log(ConsoleColor.DarkGray, true, false, category, message, arg);
        }

        public static void Warn(string category, string message, params object[] arg) => Log(ConsoleColor.DarkYellow, true, false, category, message, arg);

        public static void Error(string category, string message, params object[] arg) => Log(ConsoleColor.Red, true, true, category, message, arg);

        public static void ResetColor(TextWriter writer)
        {
            if (!EnableVT())
                Console.ResetColor();
            else
                writer.Write(ColorReset);
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

        private static string LastMessage;

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
                Log24Bit(messageColor, true, Console.Out, null, message);
            }

            Console.Out.Write(empty);
            Console.CursorLeft = 0;
            var remaining = width - pre.Length - post.Length - 4;
            Log24Bit(preColor, false, Console.Out, null, pre);
            if (remaining > 0)
            {
                Log24Bit(brickColor, false, Console.Out, null, " [");
                empty = new char[remaining];
                Array.Fill(empty, ' ');
                Array.Fill(empty, '=', 0, (int) Math.Round(remaining * Math.Min(value, 1)));
                Log24Bit(processColor, false, Console.Out, null, string.Join("", empty));
                Log24Bit(brickColor, false, Console.Out, null, "] ");

                if (showProgressValue && remaining > 6)
                {
                    var valueText = (Math.Min(value, 1) * 100).ToString(CultureInfo.InvariantCulture).Split('.')[0] + "%";
                    Console.CursorLeft = pre.Length + 2 + (int) Math.Floor(remaining / 2.0d - valueText.Length / 2.0d);
                    Log24Bit(processValueColor, false, Console.Out, null, valueText);
                    Console.CursorLeft = width - post.Length;
                }
            }

            Log24Bit(postColor, false, Console.Out, null, post);
            Console.CursorLeft = Console.WindowWidth - 1;
            Console.CursorTop -= 1;
        }

        public static void FlushProgress()
        {
            Console.CursorTop += 1;
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        [DebuggerHidden]
        [DebuggerNonUserCode]
        [DebuggerStepThrough]
        public static void Assert(bool condition, string message = null, string detail = null, params object[] args)
        {
            if (condition) return;
            var trace = new StackTrace(1, true);
            var frame = trace.GetFrame(0);

            Log24Bit(XTermColor.Purple, true, Console.Error, "ASSERT", "Assertion tripped on {0}", frame.ToString().Trim());
            
            if (message != null)
            {
                Log24Bit(XTermColor.Purple, true, Console.Error, null, "\t -> " + message, args);
            }

            if (detail != null)
            {
                Log24Bit(XTermColor.Purple, true, Console.Error, null, "\t -> " + detail, args);
            }
        }
    }
}
