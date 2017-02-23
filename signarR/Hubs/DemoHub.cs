using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace signarR.Hubs
{
    public class DemoHub : Hub
    {
        #region  override 连接方法 只能重写一次

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            return Clients.Client(Context.ConnectionId).Callback("connected success： " + Context.ConnectionId);
        }

        /// <summary>
        /// 断开
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// 重新连接
        /// </summary>
        /// <returns></returns>
        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }
        #endregion

        #region 定义方法js client 调用

        public string Test(string str)
        {
            return str + DateTime.Now.ToString();
        }

        #endregion
    }
}