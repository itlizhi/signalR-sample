using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using Microsoft.AspNet.SignalR;
using StackExchange.Redis;
using Common;

[assembly: OwinStartup(typeof(signarR.Startup))]
namespace signarR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            app.Map("/socket", map =>
            {
                //允许跨域
                map.UseCors(CorsOptions.AllowAll);

                var hubConfiguration = new HubConfiguration
                {
                    // EnableJSONP = true
                };
                map.RunSignalR(hubConfiguration);
            });

            var redis = ConnectionMultiplexer.Connect(Config.GetConnectionString("redisConn")); //连接

            //订阅消息
            redis.PreserveAsyncOrder = false;//不按顺序执行（使用并行）
            var sub = redis.GetSubscriber();

            sub.Subscribe(SignalRChannel.Demo.ToString(), (channel, message) =>
            {
                //交给hub处理
                DemoHubService.Instance.PushClient(message);
            });

        }
    }
}
