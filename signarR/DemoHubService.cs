using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using signarR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace signarR
{
    /// <summary>
    /// 处理Demo的类，这里收到不同程序的请求调用 js client
    /// </summary>
    public class DemoHubService
    {
        // 懒加载 singleton instance
        private static Lazy<DemoHubService> _instance = new Lazy<DemoHubService>(() => new DemoHubService(GlobalHost.ConnectionManager.GetHubContext<DemoHub>().Clients));

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="clients"></param>
        private DemoHubService(IHubConnectionContext<dynamic> clients)
        {
            Clients = clients;
        }

        /// <summary>
        /// 返回当前实例
        /// </summary>
        public static DemoHubService Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// hub client
        /// </summary>
        private IHubConnectionContext<dynamic> Clients
        {
            get;
            set;
        }

        /// <summary>
        /// 推送消息到客户端
        /// </summary>
        /// <param name="message"></param>
        public void PushClient(string message)
        {


        }


    }
}