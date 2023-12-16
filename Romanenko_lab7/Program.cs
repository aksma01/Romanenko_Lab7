using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Romanenko_lab7
{
    public class DistributedSystemNode
    {
        private string nodeName;
        private bool isActive;
     
        public event Func<string, bool, Task> NodeStatusChanged;

        public DistributedSystemNode(string name)
        {
            nodeName = name;
            isActive = true;
        }
     
        public async Task ReceiveMessageAsync(string message, DistributedSystemNode senderNode)
        {
            Console.WriteLine($"{nodeName} отримав повідомлення від {senderNode.nodeName}: {message}");
        
            await OnNodeStatusChanged();
        }

        public async Task SendMessageAsync(string message, DistributedSystemNode receiverNode)
        {
            Console.WriteLine($"{nodeName} відправляє повідомлення до {receiverNode.nodeName}: {message}");
            await Task.Delay(1000);       
            await OnNodeStatusChanged();
        }
      
        private async Task OnNodeStatusChanged()
        {         
            isActive = new Random().Next(2) == 0;
            if (NodeStatusChanged != null)
            {
                await NodeStatusChanged.Invoke(nodeName, isActive);
            }
        }
    }
    class Program
    {
        static async Task Main()
        {           
            DistributedSystemNode node1 = new DistributedSystemNode("Node 1");
            DistributedSystemNode node2 = new DistributedSystemNode("Node 2");
        
            node1.NodeStatusChanged += NodeStatusChangedHandler;
            node2.NodeStatusChanged += NodeStatusChangedHandler;

            await node1.SendMessageAsync("Привіт, Node 2!", node2);
        }
        static async Task NodeStatusChangedHandler(string nodeName, bool isActive)
        {
            Console.WriteLine($"{nodeName} має новий статус: {(isActive ? "активний" : "неактивний")}");
        }
    }
}