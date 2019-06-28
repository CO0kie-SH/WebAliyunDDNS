using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAliyunDDNS
{
    /// <summary>
    /// getAll 的摘要说明
    /// </summary>
    public class getAll : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
            API aPI = new API(MY_ID.AccessKeyId, MY_ID.AccessKeySecret, MY_ID.DomainName);
            string log, 解析IP, 解析RecordId;
            解析IP = aPI.select(MY_ID.HostRecord, out 解析RecordId, out log);
            context.Response.Write(log);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}