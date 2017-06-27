using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace HttpRequestListener
{
    class Program
    {
        static int a = 1;
        static System.Timers.Timer timer2;  //计时器
        static List<Param> objList = new List<Param>();
        static void Main(string[] args)
        {
            Object o = new object();
            HttpListener listerner = new HttpListener();
            timer2 = new System.Timers.Timer();
            timer2.Interval = 1000;  //设置计时器事件间隔执行时间
            timer2.Elapsed += new System.Timers.ElapsedEventHandler(timer2_Elapsed);
            timer2.Enabled = true;
            while (true)
            {
                try
                {
                    listerner.AuthenticationSchemes = AuthenticationSchemes.Anonymous;//指定身份验证 Anonymous匿名访问
                    listerner.Prefixes.Add("http://localhost:1500/TimeService/");
                    listerner.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("服务启动失败...");
                    break;
                }
                Console.WriteLine("服务器启动成功.......");

                //线程池
                int minThreadNum;
                int portThreadNum;
                int maxThreadNum;
                ThreadPool.GetMaxThreads(out maxThreadNum, out portThreadNum);
                ThreadPool.GetMinThreads(out minThreadNum, out portThreadNum);
                Console.WriteLine("最大线程数：{0}", maxThreadNum);
                Console.WriteLine("最小空闲线程数：{0}", minThreadNum);
                //ThreadPool.QueueUserWorkItem(new WaitCallback(TaskProc1), x);

                Console.WriteLine("\n\n等待客户连接中。。。。");
                while (true)
                {
                    //等待请求连接
                    //没有请求则GetContext处于阻塞状态
                    HttpListenerContext ctx = listerner.GetContext();

                    ThreadPool.QueueUserWorkItem(new WaitCallback(TaskProc), ctx);
                }
                //listerner.Stop();
            }

            Console.ReadKey();
        }
        static void TaskProc(object o)
        {
            HttpListenerContext ctx = (HttpListenerContext)o;

            ctx.Response.StatusCode = 200;//设置返回给客服端http状态代码

            //接收POST参数
            Stream stream = ctx.Request.InputStream;
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8);
            String body = reader.ReadToEnd();
            Console.WriteLine("收到POST数据:" + HttpUtility.UrlDecode(body));


            Param objParam = new Param();
            objParam.Hour = HttpUtility.ParseQueryString(body).Get("Hour");
            objParam.Minute = HttpUtility.ParseQueryString(body).Get("Minute");
            objParam.Url = HttpUtility.ParseQueryString(body).Get("Url");
            objParam.AppId = HttpUtility.ParseQueryString(body).Get("AppId");
            objList.Add(objParam);

            using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
            {
                writer.Write("{\"type\"=\"1\"}");
                writer.Close();
                ctx.Response.Close();
            }

            /*
            //接收Get参数
            string type = ctx.Request.QueryString["type"];
            string userId = ctx.Request.QueryString["userId"];
            string password = ctx.Request.QueryString["password"];
            string filename = Path.GetFileName(ctx.Request.RawUrl);
            string userName = HttpUtility.ParseQueryString(filename).Get("userName");//避免中文乱码
            //进行处理
            Console.WriteLine("收到数据:" + userName);

            //接收POST参数
            Stream stream = ctx.Request.InputStream;
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.UTF8);
            String body = reader.ReadToEnd();
            Console.WriteLine("收到POST数据:" + HttpUtility.UrlDecode(body));
            Console.WriteLine("解析:" + HttpUtility.ParseQueryString(body).Get("userName"));

            //使用Writer输出http响应代码,UTF8格式
            using (StreamWriter writer = new StreamWriter(ctx.Response.OutputStream, Encoding.UTF8))
            {
                writer.Write("处理结果,Hello world<br/>");
                writer.Write("数据是userId={0},userName={1}", userId, userName);
                writer.Close();
                ctx.Response.Close();
            }
            */
        }

        static void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int intHour = e.SignalTime.Hour;
                int intMinute = e.SignalTime.Minute;
                int intSecond = e.SignalTime.Second;
                string result = "";
                if (objList != null && objList.Count > 0)
                {
                    for (int i = 0; i < objList.Count; i++)
                    {
                        Param p = objList[i];
                        if (intHour == int.Parse(p.Hour) && intMinute == int.Parse(p.Minute))
                        {
                            objList.Remove(p);
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            dic.Add("AppId", p.AppId);
                            InterfaceHelper.RequestService(p.Url, dic, ref result);
                            dic = new Dictionary<string, string>();
                            Console.WriteLine("调用: " + p.Url);
                            Console.WriteLine("返回:" + result);
                           
                            
                            Console.WriteLine(a++);
                        }
                    }
                    //清楚集合中废数据
                    for (int i = 0; i < objList.Count; i++)
                    {
                        Param p = objList[i];
                        if (intHour > int.Parse(p.Hour))
                        {
                            objList.RemoveAt(i);
                            Console.WriteLine("作废:" + p.Hour + ":" + p.Minute + " " + p.Url);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }


        }
    }

    public class Param
    {

        public string Hour { get; set; }
        public string Minute { get; set; }
        public string Url { get; set; }
        public string AppId { get; set; }
    }

    public class InterfaceHelper
    {
        /// <summary>
        /// 调用接口通用方法
        /// </summary>
        /// <param name="strUri">接口地址</param>
        /// <param name="strPara">传入参数</param>
        /// <param name="strResult">返回的json串</param>
        /// <returns>请求是否成功，1成功，0不成功</returns>
        public static int RequestService(string strUri, Dictionary<string, string> dic, ref string strResult)
        {
            int intResult = 1;
            string message = string.Empty;
            strResult = string.Empty;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(strUri);

                myRequest.Method = "POST";

                myRequest.ContentType = "application/x-www-form-urlencoded";
                //myRequest.ContentType = "application/json; charset=utf-8";
                //string paraUrlCoded = strPara;
                byte[] payload;
                ////将URL编码后的字符串转化为字节
                //payload = System.Text.Encoding.GetEncoding("utf-8").GetBytes(paraUrlCoded);
                StringBuilder builder = new StringBuilder();
                int i = 0;
                foreach (var item in dic)
                {
                    if (i > 0)
                        builder.Append("&");
                    builder.AppendFormat("{0}={1}", item.Key, item.Value);
                    i++;
                }
                payload = Encoding.UTF8.GetBytes(builder.ToString());

                //设置请求的 ContentLength 
                myRequest.ContentLength = payload.Length;
                //获得请求流
                Stream writer = myRequest.GetRequestStream();
                //将请求参数写入流
                writer.Write(payload, 0, payload.Length);
                // 关闭请求流
                writer.Close();

                try
                {
                    HttpWebResponse HttpWResp = (HttpWebResponse)myRequest.GetResponse();
                    Stream myStream = HttpWResp.GetResponseStream();
                    StreamReader sr = new StreamReader(myStream, Encoding.GetEncoding("utf-8"));
                    strResult = sr.ReadToEnd();

                    //将json格式的字符串转换成实例
                    JObject obj = JsonConvert.DeserializeObject(strResult) as JObject;
                    if (obj["result"].ToString().Equals("0"))   //没有结果或没有数据
                    {
                        intResult = 0;
                    }
                    else
                    {
                        intResult = int.Parse(obj["result"].ToString());
                    }
                    strResult = obj["data"].ToString();
                }
                catch (Exception exp)
                {
                    intResult = 0;
                }
            }
            catch (Exception exp)
            {
                intResult = 0;
            }
            return intResult;
        }


        /// <summary>
        /// 调用接口通用方法
        /// </summary>
        /// <param name="strUri">接口地址</param>
        /// <param name="strPara">传入参数</param>
        /// <param name="obj">返回反序列化的对象</param>
        /// <returns>请求是否成功，1成功，0不成功</returns>
        public static int RequestService<T>(string strUri, string strPara, ref T obj)
        {
            int intResult = 1;
            string message = string.Empty;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(strUri);

                myRequest.Method = "POST";

                myRequest.ContentType = "application/x-www-form-urlencoded";
                if (strPara == null)
                {
                    //获得请求流
                    Stream writer = myRequest.GetRequestStream();
                    // 关闭请求流
                    writer.Close();
                }
                else
                {
                    string paraUrlCoded = strPara;
                    byte[] payload;
                    //将URL编码后的字符串转化为字节
                    payload = System.Text.Encoding.GetEncoding("utf-8").GetBytes(paraUrlCoded);
                    //设置请求的 ContentLength 
                    myRequest.ContentLength = payload.Length;
                    //获得请求流
                    Stream writer = myRequest.GetRequestStream();
                    //将请求参数写入流
                    writer.Write(payload, 0, payload.Length);
                    // 关闭请求流
                    writer.Close();
                }



                try
                {
                    HttpWebResponse HttpWResp = (HttpWebResponse)myRequest.GetResponse();
                    Stream myStream = HttpWResp.GetResponseStream();
                    StreamReader sr = new StreamReader(myStream, Encoding.GetEncoding("utf-8"));
                    string strResult = sr.ReadToEnd();

                    //将json格式的字符串转换成实例
                    JObject jobj = JsonConvert.DeserializeObject(strResult) as JObject;
                    if (jobj["result"].ToString().Equals("0"))   //没有结果或没有数据
                    {
                        intResult = 0;
                        obj = default(T);
                    }
                    else
                    {
                        intResult = int.Parse(jobj["result"].ToString());
                        obj = JsonConvert.DeserializeObject<T>(jobj["data"].ToString());
                    }
                }
                catch (Exception exp)
                {
                    intResult = 0;
                }
            }
            catch (Exception exp)
            {
                intResult = 0;
            }
            return intResult;
        }

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
