using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace task2
{
    class Program
    {
        public class Resource
        {
            public string Name { get; }

            public Resource(string name)
            {
                Name = name;
            }
        }

        public class ResourcePool
        {
            private readonly Dictionary<Resource, SemaphoreSlim> resources;
            private readonly object lockObject = new object();

            public ResourcePool(IEnumerable<Resource> availableResources)
            {
                resources = new Dictionary<Resource, SemaphoreSlim>();

                foreach (var resource in availableResources)
                {
                    resources.Add(resource, new SemaphoreSlim(1, 1));
                }
            }

            public void AcquireResource(Resource resource, bool isHighPriority)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} намагається отримати {resource.Name}...");

                if (isHighPriority)
                {
                    Monitor.Enter(lockObject);
                }

                resources[resource].Wait();

                Console.WriteLine($"{Thread.CurrentThread.Name} отримав {resource.Name}.");
            }

            public void ReleaseResource(Resource resource, bool isHighPriority)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} вивільняє {resource.Name}.");

                resources[resource].Release();

                if (isHighPriority)
                {                   
                    Monitor.Exit(lockObject);
                }
            }
        }

        public class Simulator
        {
            private static readonly Resource CPU = new Resource("CPU");
            private static readonly Resource RAM = new Resource("RAM");
            private static readonly Resource Disk = new Resource("Disk");

            private static readonly ResourcePool resourcePool = new ResourcePool(new List<Resource> { CPU, RAM, Disk });

            static void Main()
            {
                Thread lowPriorityThread1 = new Thread(() => AccessResources("Low Priority Thread 1", RAM, false));
                Thread lowPriorityThread2 = new Thread(() => AccessResources("Low Priority Thread 2", Disk, false));
                Thread highPriorityThread = new Thread(() => AccessResources("High Priority Thread", CPU, true));

                lowPriorityThread1.Start();
                lowPriorityThread2.Start();
                highPriorityThread.Start();

                lowPriorityThread1.Join();
                lowPriorityThread2.Join();
                highPriorityThread.Join();

                Console.WriteLine("Симуляція завершена.");
            }
            static void AccessResources(string threadName, Resource resource, bool isHighPriority)
            {
                Console.WriteLine($"{threadName} почав свою роботу.");

                resourcePool.AcquireResource(resource, isHighPriority);
                Thread.Sleep(2000);  

                resourcePool.ReleaseResource(resource, isHighPriority);

                Console.WriteLine($"{threadName} завершив свою роботу.");
            }
        }
    }
}
