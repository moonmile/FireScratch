using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireScratch.Proxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Firebase Scratch Proxy");
            FireScratch.Core.FireScratch.Server(5411);
            Console.ReadKey();
        }
    }
}
