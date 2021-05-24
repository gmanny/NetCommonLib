using System.IO;
using System.Reflection;

namespace MonitorCommon
{
    public static class FileUtil
    {
        public static string AppDir() => Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
    }
}