using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace task4
{
    public class Event
    {
        public string Name { get; }
        public int Timestamp { get; }

        public Event(string name, int timestamp)
        {
            Name = name;
            Timestamp = timestamp;
        }
    }

    public class LamportClock
    {
        private int value = 0;
        private readonly object lockObject = new object();

        public int Tick()
        {
            lock (lockObject)
            {
                value++;
                return value;
            }
        }

        public void Synchronize(int receivedValue)
        {
            lock (lockObject)
            {
                value = Math.Max(value, receivedValue) + 1;
            }
        }
    }

    public class Node
    {
        public string Name { get; }
        private readonly LamportClock clock;
        private readonly ConcurrentQueue<Event> eventQueue;

        public Node(string name, LamportClock clock)
        {
            Name = name;
            this.clock = clock;
            eventQueue = new ConcurrentQueue<Event>();
        }

        public void PublishEvent(string eventName)
        {
            int timestamp = clock.Tick();
            var newEvent = new Event(eventName, timestamp);
            eventQueue.Enqueue(newEvent);

            Console.WriteLine($"{Name} опублікував подію {eventName} з відміткою часу {timestamp}.");
            BroadcastEvent(newEvent);
        }

        public void Subscribe(Node otherNode)
        {
            otherNode.OnEventPublished += HandleEventPublished;
        }

        public void Unsubscribe(Node otherNode)
        {
            otherNode.OnEventPublished -= HandleEventPublished;
        }

        public event Action<Event> OnEventPublished;

        private void BroadcastEvent(Event newEvent)
        {
            OnEventPublished?.Invoke(newEvent);
        }

        private void HandleEventPublished(Event publishedEvent)
        {
            clock.Synchronize(publishedEvent.Timestamp);
            Console.WriteLine($"{Name} отримав подію {publishedEvent.Name} від {publishedEvent.Timestamp}.");
        }
    }

    public class DistributedSystem
    {
        private readonly List<Node> nodes = new List<Node>();
        private readonly LamportClock globalClock = new LamportClock();

        public void AddNode(Node newNode)
        {
            foreach (var node in nodes)
            {
                node.Subscribe(newNode);
                newNode.Subscribe(node);
            }

            nodes.Add(newNode);
            newNode.OnEventPublished += HandleEventPublished;
        }

        public void RemoveNode(Node nodeToRemove)
        {
            nodes.Remove(nodeToRemove);

            foreach (var node in nodes)
            {
                node.Unsubscribe(nodeToRemove);
                nodeToRemove.Unsubscribe(node);
            }

            nodeToRemove.OnEventPublished -= HandleEventPublished;
        }

        private void HandleEventPublished(Event publishedEvent)
        {
            globalClock.Synchronize(publishedEvent.Timestamp);
            Console.WriteLine($"Глобальний годинник: {globalClock.Tick()}");
        }
    }

    public class Program
    {
        static void Main()
        {
            DistributedSystem system = new DistributedSystem();

            Node node1 = new Node("Node 1", new LamportClock());
            Node node2 = new Node("Node 2", new LamportClock());
            Node node3 = new Node("Node 3", new LamportClock());

            system.AddNode(node1);
            system.AddNode(node2);
            system.AddNode(node3);

            Task.Run(() => node1.PublishEvent("Event A"));
            Task.Run(() => node2.PublishEvent("Event B"));

            Thread.Sleep(1000);

            system.RemoveNode(node2);

            Task.Run(() => node1.PublishEvent("Event C"));

            Console.ReadLine();
        }
    }
}
