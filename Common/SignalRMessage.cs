using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SignalRPub
    {
        /// <summary>
        /// 推送消息到订阅者
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void PublishSubscriber(SignalRChannel channel, SignalRMessage message)
        {
            try
            {
                var sub = RedisManager.GetConnInstance().GetSubscriber();
                var received = sub.Publish(channel.ToString(), Json.ToJson(message));
                if (received <= 0)
                {
                    //如果没有订阅者，记录错误日志
                    LogExtention.Instance().Error(null, channel, message);
                }
            }
            catch (Exception ex)
            {
                LogExtention.Instance().Error(ex, channel, message);
            }
        }
    }

    /// <summary>
    /// 订阅管道
    /// </summary>
    public enum SignalRChannel
    {
        Demo
    }

    public class SignalRMessage
    {
        /// <summary>
        /// 接收者ID
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// 发送内容
        /// </summary>
        public object Message { get; set; }

    }
}
