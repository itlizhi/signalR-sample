using StackExchange.Redis;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Common
{
    /// <summary>
    /// StackExchange.Redis 的帮助类
    /// </summary>
    public class RedisManager
    {
        private static ConnectionMultiplexer _conn; //连接对象
        private static object _lockObj = new object();  //lock

        /// <summary>
        /// 得到连接对象 Singleton模式
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionMultiplexer GetConnInstance(string connectionString = null)
        {
            if (_conn == null)
            {
                lock (_lockObj)
                {
                    if (_conn != null) return _conn;

                    if (string.IsNullOrEmpty(connectionString))
                    {
                        connectionString = Config.GetConnectionString("redisConn"); //默认连接字符串
                    }
                    _conn = ConnectionMultiplexer.Connect(connectionString); //连接
                    return _conn;
                }
            }
            else
            {
                return _conn;
            }
        }

        #region 基本公共方法

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMinutes">过期时间，单位分钟，默认不过期</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool Set(string key, string value, int expireMinutes = -1, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            if (expireMinutes > 0)
            {
                return db.StringSet(key, value, TimeSpan.FromMinutes(expireMinutes));
            }
            else
            {
                return db.StringSet(key, value);
            }
        }

        /// <summary>
        /// 新增 hastset，同一个key可以存储多个数据
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">字段</param>
        /// <param name="value">值</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool HashSet(string key, string filed, string value, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            try
            {
                return db.HashSet(key, filed, value);
            }
            catch (Exception ex)
            {
                LogExtention.Instance().Error(ex, key, filed, value);
                return false;
            }
        }

        /// <summary>
        /// 查询key的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static string Get(string key, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.StringGet(key);
        }

        /// <summary>
        /// 根据key 和filed得到hash的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <param name="database">默认-1</param>
        /// <returns></returns>
        public static string HashGet(string key, string filed, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);

            return db.HashGet(key, filed);
        }

        /// <summary>
        /// 得到hashkey里面存储的所有数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static HashEntry[] HashGetAll(string key, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.HashGetAll(key);
        }

        /// <summary>
        /// 查询key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool HasKey(string key, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.KeyExists(key);
        }

        /// <summary>
        /// 是否存在hash key | filed
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool HasHashKey(string key, string filed, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.HashExists(key, filed);
        }

        /// <summary>
        /// 添加一个list集合
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static long ListSet(string key, string value, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            try
            {
                return db.ListLeftPush(key, value);
            }
            catch (Exception ex)
            {
                LogExtention.Instance().Error(ex, key, value);
                return 0;
            }
        }

        /// <summary>
        /// 获取集合列表
        /// </summary>
        /// <param name="key"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static RedisValue[] ListGet(string key, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.ListRange(key);
        }

        /// <summary>
        /// 根据key移除数据，包括hash
        /// </summary>
        /// <param name="key"></param>
        /// <param name="database">默认-1</param>
        /// <returns></returns>
        public static bool Remove(string key, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.KeyDelete(key);
        }

        /// <summary>
        /// 移除集合内容
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static long ListRemove(string key, string value, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.ListRemove(key, value);
        }

        /// <summary>
        /// 移除hashset里面的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <param name="database">默认-1</param>
        /// <returns></returns>
        public static bool HashRemove(string key, string filed, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            return db.HashDelete(key, filed);
        }

        #endregion 基本公共方法

        #region 添加序列化方法

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expireMinutes">过期时间，单位分钟，默认不过期</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool Set(string key, object value, int expireMinutes = 0, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            var valueStr = Json.ToJson(value);
            if (expireMinutes > 0)
            {
                return db.StringSet(key, valueStr, TimeSpan.FromMinutes(expireMinutes));
            }
            else
            {
                return db.StringSet(key, valueStr);
            }
        }

        /// <summary>
        /// 新增 hastset，同一个key可以存储多个数据
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="filed">字段</param>
        /// <param name="value">值</param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static bool HashSet(string key, string filed, object value, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            var valueStr = Json.ToJson(value);
            return db.HashSet(key, filed, valueStr);
        }

        /// <summary>
        /// 查询key的值
        /// </summary>
        /// <typeparam name="T">返回对象</typeparam>
        /// <param name="key"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static T Get<T>(string key, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            string value = db.StringGet(key);
            return Json.ToObject<T>(value);
        }

        /// <summary>
        /// 根据key 和filed得到hash的值
        /// </summary>
        /// <typeparam name="T">返回对象</typeparam>
        /// <param name="key"></param>
        /// <param name="filed"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        public static T HashGet<T>(string key, string filed, int database = 0)
        {
            var db = GetConnInstance().GetDatabase(database);
            string value = db.HashGet(key, filed);
            return Json.ToObject<T>(value);
        }

        #endregion 添加序列化方法

        /// <summary>
        /// 推送消息到订阅者
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void PublishMessage(string channel, string message)
        {
            try
            {
                ISubscriber sub = GetConnInstance().GetSubscriber();
                var received = sub.Publish(channel, message);
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
}