using System.IO;

namespace Transpaqer
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileToPaqName = Path.GetFileName(args[0]);
            var fileToPaqBytes = File.ReadAllBytes(args[0]);
            File.WriteAllText($"{args[1]}.paqed", System.Convert.ToBase64String(fileToPaqBytes));
        }
    }
}