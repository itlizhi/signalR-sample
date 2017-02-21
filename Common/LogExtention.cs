using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace Common
{
    /// <summary>
    /// 自定义日志类，多线程列队，单线程写
    /// </summary>
    public class LogExtention
    {
        /// <summary>
        /// 文件前缀
        /// </summary>
        private string fileprefix { get; set; }

        /// <summary>
        /// 日志等级
        /// </summary>
        private string level { get; set; }

        /// <summary>
        /// 类名称
        /// </summary>
        private string className { get; set; }

        private string url { get; set; }
        private string url_form { get; set; }
        private string url_ip { get; set; }

        private Exception exception { get; set; }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <typeparam name="T">类名称</typeparam>
        /// <param name="fileprefix">文件前缀</param>
        /// <returns></returns>
        public static LogExtention Instance<T>(string fileprefix = "")
        {
            var log = new LogExtention();
            log.className = typeof(T).FullName;
            log.fileprefix = fileprefix;
            return log;
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="fileprefix">文件前缀</param>
        /// <returns></returns>
        public static LogExtention Instance(string fileprefix = "")
        {
            var log = new LogExtention();
            log.fileprefix = fileprefix;
            return log;
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <param name="type">文件</param>
        /// <param name="fileprefix">文件前缀</param>
        /// <returns></returns>
        public static LogExtention Instance(Type type, string fileprefix = "")
        {
            var log = new LogExtention();
            log.className = type.FullName;
            log.fileprefix = fileprefix;
            return log;
        }

        /// <summary>
        /// 调试数据，包括debug，info
        /// </summary>
        /// <param name="content"></param>
        public void Debug(params object[] content)
        {
            //是否启用日志debug
            if (ConfigurationManager.AppSettings["log.debug"] != "true")
            {
                return;
            }
            //获取调用的类 +方法
            var st = new StackTrace();
            if (st.GetFrames().Length > 1)
            {
                var mb = st.GetFrames()[1].GetMethod();
                className = $"{mb.DeclaringType.FullName} - {mb.Name}";
            }

            level = "debug";
            Write(content);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="content"></param>
        public void Error(Exception ex, params object[] content)
        {
            exception = ex;
            if (className == null) { className = ex.Source; }
            level = "error";
            Write(content);
        }

        private void Write(params object[] content)
        {
            var sbText = new StringBuilder();
            //基本信息
            sbText.AppendLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} | {className} | {Thread.CurrentThread.ManagedThreadId.ToString()} | {url_ip} | {url} | {url_form}");
            //内容
            foreach (var item in content)
            {
                //系列化位 json字符串
                sbText.Append($"{ JsonConvert.SerializeObject(item, Formatting.None, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() })}  |  ");
            }
            sbText.AppendLine();

            //错误信息
            if (exception != null)
            {
                //基本信息
                sbText.AppendLine($"{exception.Message} | {exception.Source} | {exception.TargetSite.ToString()} | {exception.GetType().FullName} ");
                sbText.AppendLine(exception.StackTrace); //堆栈
            }
            //插入列队
            logQueue.Enqueue(new Tuple<string, string>(GetLogPath(), sbText.ToString()));
            pause.Set(); //激活
        }

        /// <summary>
        /// 获取日志的路径
        /// </summary>
        /// <returns></returns>
        private string GetLogPath()
        {
            //配置的日志文件夹
            var customDirectory = string.Empty;
            if (ConfigurationManager.AppSettings["log.path"] != null)
            {
                customDirectory = ConfigurationManager.AppSettings["log.path"];
            }

            if (!string.IsNullOrWhiteSpace(fileprefix))
            {
                fileprefix = fileprefix + "_";// 分隔符
            }
            string newFilePath = string.Empty; //返回的文件
            string extension = ".log";

            //存储地址
            string logDir = string.IsNullOrEmpty(customDirectory) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs") : customDirectory;
            logDir = Path.Combine(logDir, level, DateTime.Now.ToString("yyyy-MM")); //添加年-月

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            //文件名称
            string fileNameNotExt = string.Concat(fileprefix, DateTime.Now.ToString("yyyyMMdd"));
            string fileName = string.Concat(fileNameNotExt, extension);

            //判断文件大小
            string fileNamePattern = string.Concat(fileNameNotExt, "_*", extension);
            var filePaths = Directory.GetFiles(logDir, fileNamePattern, SearchOption.TopDirectoryOnly).ToList();

            //文件夹不止一个文件
            if (filePaths.Count > 0)
            {
                int fileMaxLen = filePaths.Max(d => d.Length);
                string lastFilePath = filePaths.Where(d => d.Length == fileMaxLen).OrderByDescending(d => d).FirstOrDefault();
                if (new FileInfo(lastFilePath).Length > 10 * 1024 * 1024)
                {
                    var match_file = new Regex(@"_(\d).log$").Match(Path.GetFileName(lastFilePath));
                    if (match_file.Success) { newFilePath = lastFilePath; }
                    //当前的值
                    var number = match_file.Groups[1].Value;
                    int temp = 0;
                    bool parse = int.TryParse(number, out temp);
                    string formatno = string.Format("_{0}", parse ? (temp + 1) : temp);
                    string newFileName = string.Concat(fileNameNotExt, formatno, extension);
                    newFilePath = Path.Combine(logDir, newFileName);
                }
                else
                {
                    newFilePath = lastFilePath;
                }
            }
            else
            {
                string newFileName = string.Concat(fileNameNotExt, "_0", extension); //新文件
                newFilePath = Path.Combine(logDir, newFileName);
            }
            return newFilePath;
        }

        static ConcurrentQueue<Tuple<string, string>> logQueue = new ConcurrentQueue<Tuple<string, string>>();
        static Task writeTask = default(Task);
        static ManualResetEvent pause = new ManualResetEvent(false);

        static LogExtention()
        {
            //循环判断
            writeTask = new Task((object obj) =>
            {
                while (true)
                {
                    pause.WaitOne();
                    pause.Reset();
                    var temp = new List<string[]>();
                    //循环列队里面的数据
                    foreach (var logItem in logQueue)
                    {
                        string logPath = logItem.Item1; //地址
                        //内容
                        string logMergeContent = string.Concat(logItem.Item2, Environment.NewLine, Environment.NewLine);
                        //合并内容
                        string[] logArr = temp.FirstOrDefault(d => d[0].Equals(logPath));
                        if (logArr != null)
                        {
                            logArr[1] = string.Concat(logArr[1], logMergeContent);
                        }
                        else
                        {
                            logArr = new string[] { logPath, logMergeContent };
                            temp.Add(logArr);
                        }
                        Tuple<string, string> val = default(Tuple<string, string>);
                        logQueue.TryDequeue(out val);
                    }
                    //写入
                    foreach (string[] item in temp)
                    {
                        WriteText(item[0], item[1]);
                    }
                }
            }
            , null
            , TaskCreationOptions.LongRunning);
            writeTask.Start();
        }
        //写文件
        private static void WriteText(string logPath, string logContent)
        {
            try
            {
                if (!File.Exists(logPath))
                {
                    File.CreateText(logPath).Close();
                }
                StreamWriter sw = File.AppendText(logPath);
                sw.Write(logContent);
                sw.Close();
            }
            catch (Exception) { }

        }
    }
}
