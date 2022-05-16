namespace DragonLib.IO;

public interface ILogger {
    public void Log(LogLevel level, string message, params object?[] args);
    public void Log(Exception e, string message, params object?[] args);
}
