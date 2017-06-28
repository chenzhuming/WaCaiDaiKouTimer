using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tellyes.Common
{
    public class InterfaceHelper
    {
        /// <summary>
        /// 调用接口通用方法
        /// </summary>
        /// <param name="strUri">接口地址</param>
        /// <param name="strPara">传入参数</param>
        /// <param name="strResult">返回的json串</param>
        /// <returns>请求是否成功，1成功，0不成功</returns>
        public static int RequestService(string strUri, string strPara, ref string strResult)
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
                if (strPara==null)
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
