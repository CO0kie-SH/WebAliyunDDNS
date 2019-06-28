using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebAliyunDDNS
{
    public class API
    {
        private string AccessKeyId, AccessKeySecret, DomainName;
        public API(string AccessKeyId, string AccessKeySecret, string DomainName)
        {
            this.AccessKeyId = AccessKeyId;
            this.AccessKeySecret = AccessKeySecret;
            this.DomainName = DomainName;
        }

        internal string select(string HostRecord, out string RecordId, out string log)
        {
            string ip = null; RecordId = ""; log = "";
            GetPostString.RequestString requestString = new GetPostString.RequestString();
            requestString.InitializeDict(AccessKeyId);
            requestString.DictData.Add("Action", "DescribeDomainRecords");
            requestString.DictData.Add("DomainName", MY_ID.DomainName);
            requestString.Signature(AccessKeySecret);
            string HttpGetString = requestString.Serialization();
            log += "请求网址=\n" + HttpGetString + "\n\n请求返回=\n";
            HttpGetString = CreateGetHttpResponse("http://alidns.aliyuncs.com/?" + HttpGetString);
            log += HttpGetString + "\n\n";
            try
            {
                int left = HttpGetString.IndexOf($"\"RR\":\"{HostRecord}\"");
                log += "第一层left=" + left;
                left = HttpGetString.IndexOf("\"Value\":\"", left);
                log += "\n第二层left=" + left + "\n当前解析IP = ";
                ip = HttpGetString.Substring(left + 9, HttpGetString.IndexOf("Weight", left) - left - 12);
                log += ip;
                left = HttpGetString.IndexOf("RecordId", left);
                log += "\n第三层left=" + left + "\n当前解析ID = ";
                RecordId = HttpGetString.Substring(left + 11, HttpGetString.IndexOf(",", left) - left - 12);
                log += RecordId;
            }
            catch (Exception)
            {
            }
            return ip;
        }

        internal void update(string IP, string RecordId, out string log)
        {
            log = "";
            GetPostString.RequestString requestString = new GetPostString.RequestString();
            requestString.InitializeDict(AccessKeyId);
            requestString.DictData.Add("Action", "UpdateDomainRecord");
            requestString.DictData.Add("RecordId", RecordId);
            requestString.DictData.Add("RR", MY_ID.HostRecord);
            requestString.DictData.Add("Type", "A");
            requestString.DictData.Add("Value", IP);
            requestString.DictData.Add("TTL", "600");
            requestString.Signature(AccessKeySecret);
            string HttpGetString = requestString.Serialization();
            string Return = CreateGetHttpResponse("http://alidns.aliyuncs.com/?" + HttpGetString);
            log += RecordId;
        }

        internal string getWWWIP(string url, out string log)
        {
            string ip = null; log = $"\n\n {url}网页返回=\n";
            switch (url)
            {
                case "taobao":
                    ip = CreateGetHttpResponse("http://ip.taobao.com/service/getIpInfo.php?ip=myip");
                    log += ip;
                    try
                    {
                        int left = ip.IndexOf("\"ip\":\"");
                        log += "\n第一层left=" + left + "\n得到的IP = ";
                        ip = ip.Substring(left + 6, ip.IndexOf(",", left) - left - 7);
                        log += ip;
                    }
                    catch (Exception) { }
                    break;
                case "138":
                    ip = CreateGetHttpResponse("http://2019.ip138.com", "GBK");
                    log += ip;
                    try
                    {
                        int left = ip.IndexOf("您的IP是：");
                        log += "\n\n第一层left=" + left + "\n得到的IP = ";
                        ip = ip.Substring(left + 7, ip.IndexOf("]", left) - left - 7);
                        log += ip;
                    }
                    catch (Exception) { }
                    break;
                default:
                    break;
            }

            return ip;
        }

        public static string CreateGetHttpResponse(string url, string end = "UTF-8")
        {
            string text = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                using (Stream data = request.GetResponse().GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(data,
                            end.Equals("UTF-8") ? Encoding.UTF8 : Encoding.GetEncoding(end)))
                    {
                        text = reader.ReadToEnd();
                    }
                }
                request.Abort();
            }
            catch (WebException ex)
            {
                HttpWebResponse response = (HttpWebResponse)ex.Response;
                //Console.WriteLine("Error code: {0}", response.StatusCode);
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (Stream data = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(data,
                            end.Equals("UTF-8") ? Encoding.UTF8 : Encoding.GetEncoding(end)))
                        {
                            text = reader.ReadToEnd();
                        }
                    }
                }
                response.Dispose();
                throw new Exception(text);
            }
            return text;
        }//public static string CreateGetHttpResponse(string url)
    }//public class API2 END
    public class GetPostString
    {
        public class RequestString
        {
            public SortedDictionary<string, string> DictData;
            /// <summary>
            /// 初始化字典
            /// </summary>
            /// <param name="AccessKeyId"></param>
            public void InitializeDict(string AccessKeyId)
            {
                DictData = new SortedDictionary<string, string>(StringComparer.Ordinal) {
                {"Format", "json" },
                {"AccessKeyId", AccessKeyId },
                {"SignatureMethod", "HMAC-SHA1" },
                {"SignatureVersion", "1.0" },
                {"SignatureNonce", Guid.NewGuid().ToString() },
                {"Timestamp", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "Version", "2015-01-09"}
                };

            }
            /// <summary>
            /// 对当前对象内的字典进行序列化
            /// </summary>
            /// <returns></returns>
            public string Serialization()
            {
                string Return = "";
                foreach (var kvm in DictData)
                {
                    Return += "&" +
                        HttpUtility.UrlEncode(kvm.Key) + "=" +
                        HttpUtility.UrlEncode(kvm.Value);
                }
                return Return.Substring(1).Replace("%253a", "%253A").Replace("%2b", "%2B").Replace("%3d", "%3D").Replace("%2f", "%2F");
            }
            /// <summary>
            /// 结合Key数据以及对象内的字典进行签名
            /// </summary>
            /// <param name="AccessKeySecret">阿里云签名秘钥</param>
            /// <param name="HttpMethod">http格式：GET或者Post</param>
            /// <returns></returns>
            public string Signature(string AccessKeySecret, string HttpMethod = "GET")
            {
                string RawString = Serialization();
                HMACSHA1 HmacSha1 = new HMACSHA1(Encoding.UTF8.GetBytes(AccessKeySecret + "&"));
                string Data = HttpMethod + "&" + HttpUtility.UrlEncode("/") + "&" + HttpUtility.UrlEncode(RawString);
                string Singer = Convert.ToBase64String(HmacSha1.ComputeHash(Encoding.UTF8.GetBytes(Data.Replace("%253a", "%253A").Replace("%2b", "%2B").Replace("%3d", "%3D").Replace("%2f", "%2F"))));
                DictData.Add("Signature", Singer);
                return Singer;
            }
        }

    }
}
