using SyncLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Properties;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SyncWatcher sync = new SyncWatcher(Directory.GetCurrentDirectory(), "*.cwa", Settings1.Default.AWS_KEY, Settings1.Default.AWS_SECRET, Settings1.Default.AWS_BUCKET, Settings1.Default.AWS_REGION, Settings1.Default.API_URL,Settings1.Default.PRE_SHARED_KEY);
            sync.Start();
            sync.HaveProgrammedDevice(1, new Random().Next(0, 10000).ToString());
            Console.ReadLine();
        }
    }
}
