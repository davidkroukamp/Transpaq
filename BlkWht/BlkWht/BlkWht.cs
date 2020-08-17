using System.IO;
using System.Threading;

namespace BlkWht
{
    public class BlkWht
    {
        public static void Execute()
        {
            File.WriteAllText(@"C:\WriteText.txt", "Im newerrr");
            Thread.Sleep(1200000);
        }
    }
}
