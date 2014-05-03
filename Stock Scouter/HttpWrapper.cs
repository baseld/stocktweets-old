using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Stock_Scouter
{
    class HttpWrapper
    {
        public delegate void RESTSuccessCallback(Stream stream);
        public delegate void RESTErrorCallback(String reason);

        public static void get(Uri uri, Dictionary<String, String> extra_headers, RESTSuccessCallback success_callback, RESTErrorCallback error_callback)
        {
            HttpWebRequest request = WebRequest.CreateHttp(uri);

            if (extra_headers != null)
                foreach (String header in extra_headers.Keys)
                    try
                    {
                        request.Headers[header] = extra_headers[header];
                    }
                    catch (Exception) { }

            request.BeginGetResponse((IAsyncResult result) =>
            {
                HttpWebRequest req = result.AsyncState as HttpWebRequest;
                if (req != null)
                {
                    try
                    {
                        WebResponse response = req.EndGetResponse(result);
                        success_callback(response.GetResponseStream());
                    }
                    catch (WebException e)
                    {
                        error_callback(e.Message);
                        return;
                    }
                }
            }, request);
        }

        public static void post(Uri uri, Dictionary<String, String> post_params, Dictionary<String, String> extra_headers, RESTSuccessCallback success_callback, RESTErrorCallback error_callback)
        {
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            if (extra_headers != null)
                foreach (String header in extra_headers.Keys)
                    try
                    {
                        request.Headers[header] = extra_headers[header];
                    }
                    catch (Exception) { }


            request.BeginGetRequestStream((IAsyncResult result) =>
            {
                HttpWebRequest preq = result.AsyncState as HttpWebRequest;
                if (preq != null)
                {
                    Stream postStream = preq.EndGetRequestStream(result);

                    StringBuilder postParamBuilder = new StringBuilder();
                    if (post_params != null)
                        foreach (String key in post_params.Keys)
                            postParamBuilder.Append(String.Format("{0}={1}&", key, post_params[key]));

                    Byte[] byteArray = Encoding.UTF8.GetBytes(postParamBuilder.ToString());

                    postStream.Write(byteArray, 0, byteArray.Length);
                    postStream.Close();


                    preq.BeginGetResponse((IAsyncResult final_result) =>
                    {
                        HttpWebRequest req = final_result.AsyncState as HttpWebRequest;
                        if (req != null)
                        {
                            try
                            {
                                WebResponse response = req.EndGetResponse(final_result);
                                success_callback(response.GetResponseStream());
                            }
                            catch (WebException e)
                            {
                                error_callback(e.Message);
                                return;
                            }
                        }
                    }, preq);
                }
            }, request);
        }

    }
}
