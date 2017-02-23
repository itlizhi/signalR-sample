using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.Threading;
namespace Console1
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 0; i < 50; i++)
            {
                //模拟推送消息
                SignalRPub.PublishSubscriber(SignalRChannel.Demo, new SignalRMessage()
                {
                    RecipientId = "123",
                    Message = DateTime.Now.ToString()
                });
                Thread.Sleep(2000);
            }
            Console.ReadLine();
        }
    }
}
