using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Transactions;

namespace ThreadPractice2
{
    public class CSVReader
    {
        private string path;
        public CSVReader(string path)
        {
            this.path = path;
        }

        public IEnumerable<string> Read()
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.SequentialScan);
            using var reader = new StreamReader(fs);
            string line = "";
            while ((line = reader.ReadLine()) != null)
            {
                yield return line.Split(',')[0];
            }
        }
    }

    public class ThreadTester
    {
        private string threadName;
        private List<string> temp;
        
        public ThreadTester(string threadName, List<string> temp)
        {
            this.threadName = threadName;
            this.temp = temp;
        }

        public void printall()
        {
            foreach (string x in temp)
            {
                if (x == "99999")
                {
                    Console.WriteLine("reached last number" );

                }
                
                Console.WriteLine(this.threadName + ": " + x );
            }
            
        }
        
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            CSVReader reader =
                new CSVReader("/Users/philip/RiderProjects/ThreadPractice2/ThreadPractice2/csvs/test.csv");

            int i = 0;
            int total = 0;

            List<string> list = new List<string>();

            LinkedList<Thread> threadQueue = new LinkedList<Thread>();

            foreach (string x in reader.Read())
            {
                //for each 1k lines, start a new thread that prints out all of the values
                //Console.WriteLine("{0}. {1}", i, x.Replace("\"", ""));

                list.Add(x);
                if (i == 10000)
                {
                    if (threadQueue.Count < 10)
                    {
                        threadQueue.AddLast(new Thread(new ThreadStart((new ThreadTester(String.Format("Thread {0}", total), list.ToList())).printall)));
                        threadQueue.Last.Value.Start();
                    }
                    else
                    {
                        while (threadQueue.First.Value.IsAlive)
                        {
                            Console.WriteLine("waiting...");
                            Thread.Sleep(4000);
                        }

                        threadQueue.RemoveFirst();
                        threadQueue.AddLast(new Thread(new ThreadStart((new ThreadTester(String.Format("Thread {0}", total), list.ToList())).printall)));
                        threadQueue.Last.Value.Start();
                    }
                    list = null;
                    //GC.Collect();
                    Console.WriteLine("Enqueued thread");

                    list = new List<string>();
                    i = 0;
                }
                i++;
                total++;
            }
            if(list.Count > 0)
            {
                threadQueue.AddLast(new Thread(new ThreadStart((new ThreadTester(String.Format("Thread {0}", total), list.ToList())).printall)));
                threadQueue.Last.Value.Start();
            }
        }
    }
}