﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAliyunDDNS
{
    /// <summary>
    /// Handler1 的摘要说明
    /// </summary>
    public class Handler1 : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
            API aPI = new API(MY_ID.AccessKeyId, MY_ID.AccessKeySecret, MY_ID.DomainName);
            string log, 解析IP, 解析RecordId, 当前IP;
            解析IP = aPI.select(MY_ID.HostRecord, out 解析RecordId, out log);
            context.Response.Write(log);
            当前IP = aPI.getWWWIP("taobao", out log);
            context.Response.Write(log);
            当前IP = aPI.getWWWIP("138", out log);
            context.Response.Write(log);

            if (解析IP.Length > 7 && 当前IP.Length > 7 && !解析IP.Equals(当前IP))
            {
                context.Response.Write("\n当前IP不一致，更新解析中。。。\n");
                aPI.update(当前IP, 解析RecordId, out log);
            }
            else context.Response.Write("\n当前IP一致，无需更新。");
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