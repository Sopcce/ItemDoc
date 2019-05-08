using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace Sop.Core.Web
{
    /// <summary>
    /// Http���Ӳ���������
    /// </summary>
    public class HttpHelper
    {

        #region Ԥ���巽����
        //Ĭ�ϵı���
        private Encoding encoding = Encoding.Default;
        //Post���ݱ���
        private Encoding postencoding = Encoding.Default;
        //HttpWebRequest����������������
        private HttpWebRequest request = null;
        //��ȡӰ���������ݶ���
        private HttpWebResponse response = null;
        #endregion

        #region Public

        /// <summary>
        /// �����ഫ������ݣ��õ���Ӧҳ������
        /// </summary>
        /// <param name="item">���������</param>
        /// <returns>����HttpResult����</returns>
        public HttpResult GetHtml(HttpItem item)
        {
            //���ز���
            HttpResult result = new HttpResult();
            try
            {
                //׼������
                SetRequest(item);
            }
            catch (Exception ex)
            {
                result.Cookie = string.Empty;
                result.Header = null;
                result.Html = ex.Message;
                result.StatusDescription = "���ò���ʱ����" + ex.Message;
                //���ò���ʱ����
                return result;
            }
            try
            {
                //��������
                using (response = (HttpWebResponse)request.GetResponse())
                {
                    GetData(item, result);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (response = (HttpWebResponse)ex.Response)
                    {
                        GetData(item, result);
                    }
                }
                else
                {
                    result.Html = ex.Message;
                }
            }
            catch (Exception ex)
            {
                result.Html = ex.Message;
            }
            if (item.IsToLower) result.Html = result.Html.ToLower();
            return result;
        }
        #endregion

        #region GetData

        /// <summary>
        /// ����html��ǩ
        /// </summary>
        /// <param name="stringToStrip">html������</param>
        /// <returns></returns>
        public static string StripHTML(string stringToStrip)
        {
            // paring using RegEx           //
            stringToStrip = Regex.Replace(stringToStrip, "</p(?:\\s*)>(?:\\s*)<p(?:\\s*)>", "\n\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = Regex.Replace(stringToStrip, "", "\n", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = Regex.Replace(stringToStrip, "\"", "''", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            stringToStrip = StripHtmlXmlTags(stringToStrip);
            return stringToStrip;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string StripHtmlXmlTags(string content)
        {
            return Regex.Replace(content, "<[^>]+>", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// ��ȡ���ݵĲ������ķ���
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        private void GetData(HttpItem item, HttpResult result)
        {
            #region base
            //��ȡStatusCode
            result.StatusCode = response.StatusCode;
            //��ȡStatusDescription
            result.StatusDescription = response.StatusDescription;
            //��ȡHeaders
            result.Header = response.Headers;
            //��ȡCookieCollection
            if (response.Cookies != null) result.CookieCollection = response.Cookies;
            //��ȡset-cookie
            if (response.Headers["set-cookie"] != null) result.Cookie = response.Headers["set-cookie"];
            #endregion

            #region byte
            //������ҳByte
            byte[] ResponseByte = GetByte();
            #endregion

            #region Html
            if (ResponseByte != null & ResponseByte.Length > 0)
            {
                //���ñ���
                SetEncoding(item, result, ResponseByte);
                //�õ����ص�HTML
                result.Html = encoding.GetString(ResponseByte);
            }
            else
            {
                //û�з����κ�Html����
                result.Html = string.Empty;
            }
            #endregion
        }
        /// <summary>
        /// ���ñ���
        /// </summary>
        /// <param name="item">HttpItem</param>
        /// <param name="result">HttpResult</param>
        /// <param name="ResponseByte">byte[]</param>
        private void SetEncoding(HttpItem item, HttpResult result, byte[] ResponseByte)
        {
            //�Ƿ񷵻�Byte��������
            if (item.ResultType == ResultType.Byte) result.ResultByte = ResponseByte;
            //�����￪ʼ����Ҫ���ӱ�����
            if (encoding == null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(ResponseByte), "<meta[^<]*charset=([^<]*)[\"']", RegexOptions.IgnoreCase);
                string c = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    c = meta.Groups[1].Value.ToLower().Trim();
                }
                if (c.Length > 2)
                {
                    try
                    {
                        encoding = Encoding.GetEncoding(c.Replace("\"", string.Empty).Replace("'", "").Replace(";", "").Replace("iso-8859-1", "gbk").Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(response.CharacterSet))
                        {
                            encoding = Encoding.UTF8;
                        }
                        else
                        {
                            encoding = Encoding.GetEncoding(response.CharacterSet);
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(response.CharacterSet))
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                }
            }
        }
        /// <summary>
        /// ��ȡ��ҳByte
        /// </summary>
        /// <returns></returns>
        private byte[] GetByte()
        {
            byte[] ResponseByte = null;
            MemoryStream _stream = new MemoryStream();

            //GZIIP����
            if (response.ContentEncoding != null && response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
            {
                //��ʼ��ȡ�������ñ��뷽ʽ
                _stream = GetMemoryStream(new GZipStream(response.GetResponseStream(), CompressionMode.Decompress));
            }
            else
            {
                //��ʼ��ȡ�������ñ��뷽ʽ
                _stream = GetMemoryStream(response.GetResponseStream());
            }
            //��ȡByte
            ResponseByte = _stream.ToArray();
            _stream.Close();
            return ResponseByte;
        }

        /// <summary>
        /// 4.0����.net�汾ȡ����ʹ��
        /// </summary>
        /// <param name="streamResponse">��</param>
        private MemoryStream GetMemoryStream(Stream streamResponse)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = streamResponse.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                _stream.Write(buffer, 0, bytesRead);
                bytesRead = streamResponse.Read(buffer, 0, Length);
            }
            return _stream;
        }
        #endregion

        #region SetRequest

        /// <summary>
        /// Ϊ����׼������
        /// </summary>
        ///<param name="item">�����б�</param>
        private void SetRequest(HttpItem item)
        {
            // ��֤֤��
            SetCer(item);
            //����Header����
            if (item.Header != null && item.Header.Count > 0) foreach (string key in item.Header.AllKeys)
                {
                    request.Headers.Add(key, item.Header[key]);
                }
            // ���ô���
            SetProxy(item);
            if (item.ProtocolVersion != null) request.ProtocolVersion = item.ProtocolVersion;
            request.ServicePoint.Expect100Continue = item.Expect100Continue;
            //����ʽGet����Post
            request.Method = item.Method;
            request.Timeout = item.Timeout;
            request.KeepAlive = item.KeepAlive;
            request.ReadWriteTimeout = item.ReadWriteTimeout;
            if (item.IfModifiedSince != null) request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            //Accept
            request.Accept = item.Accept;
            //ContentType��������
            request.ContentType = item.ContentType;
            //UserAgent�ͻ��˵ķ������ͣ�����������汾�Ͳ���ϵͳ��Ϣ
            request.UserAgent = item.UserAgent;
            // ����
            encoding = item.Encoding;
            //���ð�ȫƾ֤
            request.Credentials = item.ICredentials;
            //����Cookie
            SetCookie(item);
            //��Դ��ַ
            request.Referer = item.Referer;
            //�Ƿ�ִ����ת����
            request.AllowAutoRedirect = item.Allowautoredirect;
            if (item.MaximumAutomaticRedirections > 0)
            {
                request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            }
            //����Post����
            SetPostData(item);
            //�����������
            if (item.Connectionlimit > 0) request.ServicePoint.ConnectionLimit = item.Connectionlimit;
        }
        /// <summary>
        /// ����֤��
        /// </summary>
        /// <param name="item"></param>
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.CerPath))
            {
                //��һ��һ��Ҫд�ڴ������ӵ�ǰ�档ʹ�ûص��ķ�������֤����֤��
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //��ʼ�����񣬲����������URL��ַ
                request = (HttpWebRequest)WebRequest.Create(item.URL);
                SetCerList(item);
                //��֤����ӵ�������
                request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //��ʼ�����񣬲����������URL��ַ
                request = (HttpWebRequest)WebRequest.Create(item.URL);
                SetCerList(item);
            }
        }
        /// <summary>
        /// ���ö��֤��
        /// </summary>
        /// <param name="item"></param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    request.ClientCertificates.Add(c);
                }
            }
        }
        /// <summary>
        /// ����Cookie
        /// </summary>
        /// <param name="item">Http����</param>
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie)) request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //����CookieCollection
            if (item.ResultCookieType == ResultCookieType.CookieCollection)
            {
                request.CookieContainer = new CookieContainer();
                if (item.CookieCollection != null && item.CookieCollection.Count > 0)
                    request.CookieContainer.Add(item.CookieCollection);
            }
        }
        /// <summary>
        /// ����Post����
        /// </summary>
        /// <param name="item">Http����</param>
        private void SetPostData(HttpItem item)
        {
            //��֤�ڵõ����ʱ�Ƿ��д�������
            if (!request.Method.Trim().ToLower().Contains("get"))
            {
                if (item.PostEncoding != null)
                {
                    postencoding = item.PostEncoding;
                }
                byte[] buffer = null;
                //д��Byte����
                if (item.PostDataType == PostDataType.Byte && item.PostdataByte != null && item.PostdataByte.Length > 0)
                {
                    //��֤�ڵõ����ʱ�Ƿ��д�������
                    buffer = item.PostdataByte;
                }//д���ļ�
                else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrEmpty(item.Postdata))
                {
                    StreamReader r = new StreamReader(item.Postdata, postencoding);
                    buffer = postencoding.GetBytes(r.ReadToEnd());
                    r.Close();
                } //д���ַ���
                else if (!string.IsNullOrEmpty(item.Postdata))
                {
                    buffer = postencoding.GetBytes(item.Postdata);
                }
                if (buffer != null)
                {
                    request.ContentLength = buffer.Length;
                    request.GetRequestStream().Write(buffer, 0, buffer.Length);
                }
            }
        }
        /// <summary>
        /// ���ô���
        /// </summary>
        /// <param name="item">��������</param>
        private void SetProxy(HttpItem item)
        {
            bool isIeProxy = false;
            if (!string.IsNullOrEmpty(item.ProxyIp))
            {
                isIeProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrEmpty(item.ProxyIp) && !isIeProxy)
            {
                //���ô��������
                if (item.ProxyIp.Contains(":"))
                {
                    string[] plist = item.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(), Convert.ToInt32(plist[1].Trim()));
                    //��������
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //����ǰ�������
                    request.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(item.ProxyIp, false);
                    //��������
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName, item.ProxyPwd);
                    //����ǰ�������
                    request.Proxy = myProxy;
                }
            }
            else if (isIeProxy)
            {
                //����ΪIE����
            }
            else
            {
                request.Proxy = item.WebProxy;
            }
        }
        #endregion

        #region private main
        /// <summary>
        /// �ص���֤֤������
        /// </summary>
        /// <param name="sender">������</param>
        /// <param name="certificate">֤��</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns>bool</returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }
        #endregion
    }
    /// <summary>
    /// Http����ο���
    /// </summary>
    public class HttpItem
    {
      /// <summary>
        /// ����URL������д
        /// </summary>
        public string URL { get; set; } = string.Empty;

      /// <summary>
        /// ����ʽĬ��ΪGET��ʽ,��ΪPOST��ʽʱ��������Postdata��ֵ
        /// </summary>
        public string Method { get; set; } = "GET";

      /// <summary>
        /// Ĭ������ʱʱ��
        /// </summary>
        public int Timeout { get; set; } = 100000;

      /// <summary>
        /// Ĭ��д��Post���ݳ�ʱ��
        /// </summary>
        public int ReadWriteTimeout { get; set; } = 30000;

      /// <summary>
        ///  ��ȡ������һ��ֵ����ֵָʾ�Ƿ��� Internet ��Դ�����־�������Ĭ��Ϊtrue��
        /// </summary>
        public Boolean KeepAlive { get; set; } = true;

      /// <summary>
        /// �����ͷֵ Ĭ��Ϊtext/html, application/xhtml+xml, */*
        /// </summary>
        public string Accept { get; set; } = "text/html, application/xhtml+xml, */*";

      /// <summary>
        /// ���󷵻�����Ĭ�� text/html
        /// </summary>
        public string ContentType { get; set; } = "text/html";

      /// <summary>
        /// �ͻ��˷�����ϢĬ��Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";

      /// <summary>
        /// �������ݱ���Ĭ��ΪNUll,�����Զ�ʶ��,һ��Ϊutf-8,gbk,gb2312
        /// </summary>
        public Encoding Encoding { get; set; } = null;

      /// <summary>
        /// Post����������
        /// </summary>
        public PostDataType PostDataType { get; set; } = PostDataType.String;

      /// <summary>
        /// Post����ʱҪ���͵��ַ���Post����
        /// </summary>
        public string Postdata { get; set; } = string.Empty;

      /// <summary>
        /// Post����ʱҪ���͵�Byte���͵�Post����
        /// </summary>
        public byte[] PostdataByte { get; set; } = null;

      /// <summary>
        /// ���ô�����󣬲���ʹ��IEĬ�����þ�����ΪNull�����Ҳ�Ҫ����ProxyIp
        /// </summary>
        public WebProxy WebProxy { get; set; }

      /// <summary>
        /// Cookie���󼯺�
        /// </summary>
        public CookieCollection CookieCollection { get; set; } = null;

      /// <summary>
        /// ����ʱ��Cookie
        /// </summary>
        public string Cookie { get; set; } = string.Empty;

      /// <summary>
        /// ��Դ��ַ���ϴη��ʵ�ַ
        /// </summary>
        public string Referer { get; set; } = string.Empty;

      /// <summary>
        /// ֤�����·��
        /// </summary>
        public string CerPath { get; set; } = string.Empty;

      /// <summary>
        /// �Ƿ�����Ϊȫ��Сд��Ĭ��Ϊ��ת��
        /// </summary>
        public Boolean IsToLower { get; set; } = false;

      /// <summary>
        /// ֧����תҳ�棬��ѯ���������ת���ҳ�棬Ĭ���ǲ���ת
        /// </summary>
        public Boolean Allowautoredirect { get; set; } = false;

      /// <summary>
        /// ���������
        /// </summary>
        public int Connectionlimit { get; set; } = 1024;

      /// <summary>
        /// ����Proxy �������û���
        /// </summary>
        public string ProxyUserName { get; set; } = string.Empty;

      /// <summary>
        /// ���� ����������
        /// </summary>
        public string ProxyPwd { get; set; } = string.Empty;

      /// <summary>
        /// ���� ����IP ,���Ҫʹ��IE���������Ϊieproxy
        /// </summary>
        public string ProxyIp { get; set; } = string.Empty;

      /// <summary>
        /// ���÷�������String��Byte
        /// </summary>
        public ResultType ResultType { get; set; } = ResultType.String;

      /// <summary>
        /// header����
        /// </summary>
        public WebHeaderCollection Header { get; set; } = new WebHeaderCollection();

      /// <summary>
        /// ��ȡ��������������� HTTP �汾�����ؽ��:��������� HTTP �汾��Ĭ��Ϊ System.Net.HttpVersion.Version11��
        /// </summary>
        public Version ProtocolVersion { get; set; }

      /// <summary>
        ///  ��ȡ������һ�� System.Boolean ֵ����ֵȷ���Ƿ�ʹ�� 100-Continue ��Ϊ����� POST ������Ҫ 100-Continue ��Ӧ����Ϊ true������Ϊ false��Ĭ��ֵΪ true��
        /// </summary>
        public Boolean Expect100Continue { get; set; } = true;

      /// <summary>
        /// ����509֤�鼯��
        /// </summary>
        public X509CertificateCollection ClentCertificates { get; set; }

      /// <summary>
        /// ���û��ȡPost��������,Ĭ�ϵ�ΪDefault����
        /// </summary>
        public Encoding PostEncoding { get; set; }

      /// <summary>
        /// Cookie��������,Ĭ�ϵ���ֻ�����ַ�������
        /// </summary>
        public ResultCookieType ResultCookieType { get; set; } = ResultCookieType.String;

      /// <summary>
        /// ��ȡ����������������֤��Ϣ��
        /// </summary>
        public ICredentials ICredentials { get; set; } = CredentialCache.DefaultCredentials;

      /// <summary>
        /// �������󽫸�����ض���������Ŀ
        /// </summary>
        public int MaximumAutomaticRedirections { get; set; }

      /// <summary>
        /// ��ȡ������IfModifiedSince��Ĭ��Ϊ��ǰ���ں�ʱ��
        /// </summary>
        public DateTime? IfModifiedSince { get; set; } = null;
    }
    /// <summary>
    /// Http���ز�����
    /// </summary>
    public class HttpResult
    {
      /// <summary>
        /// Http���󷵻ص�Cookie
        /// </summary>
        public string Cookie { get; set; }

      /// <summary>
        /// Cookie���󼯺�
        /// </summary>
        public CookieCollection CookieCollection { get; set; }

      /// <summary>
        /// ���ص�String�������� ֻ��ResultType.Stringʱ�ŷ������ݣ��������Ϊ��
        /// </summary>
        public string Html { get; set; } = string.Empty;

      /// <summary>
        /// ���ص�Byte���� ֻ��ResultType.Byteʱ�ŷ������ݣ��������Ϊ��
        /// </summary>
        public byte[] ResultByte { get; set; }

      /// <summary>
        /// header����
        /// </summary>
        public WebHeaderCollection Header { get; set; }

      /// <summary>
        /// ����״̬˵��
        /// </summary>
        public string StatusDescription { get; set; }

      /// <summary>
        /// ����״̬��,Ĭ��ΪOK
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
    }
    /// <summary>
    /// ��������
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// ��ʾֻ�����ַ��� ֻ��Html������
        /// </summary>
        String,
        /// <summary>
        /// ��ʾ�����ַ������ֽ��� ResultByte��Html�������ݷ���
        /// </summary>
        Byte
    }
    /// <summary>
    /// Post�����ݸ�ʽĬ��Ϊstring
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// �ַ������ͣ���ʱ����Encoding�ɲ�����
        /// </summary>
        String,
        /// <summary>
        /// Byte���ͣ���Ҫ����PostdataByte������ֵ����Encoding������Ϊ��
        /// </summary>
        Byte,
        /// <summary>
        /// ���ļ���Postdata��������Ϊ�ļ��ľ���·������������Encoding��ֵ
        /// </summary>
        FilePath
    }
    /// <summary>
    /// Cookie��������
    /// </summary>
    public enum ResultCookieType
    {
        /// <summary>
        /// ֻ�����ַ������͵�Cookie
        /// </summary>
        String,
        /// <summary>
        /// CookieCollection��ʽ��Cookie����ͬʱҲ����String���͵�cookie
        /// </summary>
        CookieCollection
    }
}