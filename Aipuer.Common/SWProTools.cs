using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aipuer.Common
{
   public  class SWProTools
    {
        public SWProTools()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        public static string DataTable2Json(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                return "";
            }
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":\"");
                    jsonBuilder.Append(dt.Rows[i][j].ToString());
                    jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]");
            return jsonBuilder.ToString();
        }

        public static string queryData(string url)
        {
            StringBuilder res = new StringBuilder();

            StreamReader sr = null;
            try
            {
                WebRequest wr = WebRequest.Create(url);
                WebResponse wr_result = wr.GetResponse();
                Stream ReceiveStream = wr_result.GetResponseStream();
                Encoding encode = Encoding.GetEncoding("UTF-8");
                sr = new StreamReader(ReceiveStream, encode);
                if (true)
                {
                    Char[] read = new Char[256];
                    int count = sr.Read(read, 0, 256);
                    while (count > 0)
                    {
                        string str = new string(read, 0, count);
                        res.Append(str);
                        count = sr.Read(read, 0, 256);
                    }
                }
            }
            catch (WebException we)
            {

            }
            finally
            {
                sr.Close();
            }

            return res.ToString();
        }
        public static string queryDataPost(string urlstr)
        {
            string url = urlstr.Split('?')[0];
            string args = urlstr.Split('?')[1];
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            Encoding encoding = Encoding.UTF8;
            byte[] bs = Encoding.ASCII.GetBytes(args);
            string responseData = String.Empty;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = bs.Length;
            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
                reqStream.Close();
            }
            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
                {
                    responseData = reader.ReadToEnd().ToString();
                }
            }
            return responseData;
        }

        public static string queryData1(string url)
        {
            StringBuilder res = new StringBuilder();
            string urlAddress = url.Split('?')[0];
            string param = url.Split('?')[1];
            StreamReader sr = null;
            WebResponse wr = null;
            try
            {
                byte[] bs = Encoding.ASCII.GetBytes(param);
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.Method = "GET";
                using (wr = req.GetResponse())
                {
                    //在这里对接收到的页面内容进行处理
                    Stream ReceiveStream = wr.GetResponseStream();
                    Encoding encode = Encoding.GetEncoding("UTF-8");
                    sr = new StreamReader(ReceiveStream, encode);
                    res.Append(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                sr.Close();
                wr.Close();
            }

           return res.ToString();
        }

        public static List<string> getNoRepeatData(List<string> stringArray)
        {
            //List用于存储从数组里取出来的不相同的元素
            List<string> listString = new List<string>();
            foreach (string eachString in stringArray)
            {
                if (!listString.Contains(eachString))
                    listString.Add(eachString);
            }
            return listString;
        }

        //结果是否是数字，是则去掉小数点后没用的0并保留两位小数，不是则是原来的
        public static string resultToSwitch(string data)
        {
            double data_double = 0;
            if (data != "" && Double.TryParse(data, out data_double))
            {
                return Math.Round(data_double, 2).ToString();
            }
            else
            {
                return data;
            }
        }

        //获取查询出来的坐标集合
        public static string getCoordinate(string result)
        {
            string coordinates = "";
            JObject jobj = JObject.Parse(result);

            foreach (var item in jobj["features"])
            {
                string temp_p_mj = item["properties"].ToString();
                JObject jo_mj = JObject.Parse(temp_p_mj);
                if (jo_mj["ReCalcuArea"] != null)
                {
                    if (jo_mj["ReCalcuArea"].ToString().IndexOf("e-") != -1 || float.Parse(resultToSwitch(jo_mj["ReCalcuArea"].ToString())) == 0 || float.Parse(resultToSwitch(jo_mj["ReCalcuArea"].ToString())) < 1) continue;
                }
                string temp_p = item["geometry"].ToString();
                JObject jo = JObject.Parse(temp_p);
                if (jo["type"].ToString() == "GeometryCollection") continue;
                JArray a = JArray.Parse(jo.Last.First.First.ToString());
                if (jo.First.Last.ToString() == "MultiPolygon")
                {
                    a = JArray.Parse(jo.Last.First.ToString());
                    for (int i = 0; i < jo.Last.First.ToArray().Length; i++)
                    {
                        if (i == 0)
                        {
                            a = JArray.Parse(jo.Last.First.ToArray()[i][0].ToString());
                        }
                        else
                        {
                            if (a.ToArray().Length < jo.Last.First.ToArray()[i][0].ToArray().Length)
                            {
                                a = JArray.Parse(jo.Last.First.ToArray()[i][0].ToString());
                            }
                        }
                    }
                }

                string _cur_coordinate = "((";
                foreach (var item2 in a)
                {
                    _cur_coordinate += item2[0] + " " + item2[1] + ",";
                }
                _cur_coordinate = _cur_coordinate.Remove(_cur_coordinate.Length - 1) + "));";
                coordinates += _cur_coordinate;
            }
            if (coordinates != "")
            {
                coordinates = coordinates.Remove(coordinates.Length - 1);
            }


            return coordinates;
        }


        public static string Zuhejieguo(string resultstr, string soursestr)
        {
            string result = "";
            if (resultstr == "")
            {
                result = soursestr;
                return result;
            }

            JObject sourseobj = JObject.Parse(soursestr);
            JArray sourseobjfeatures = (JArray)sourseobj["features"];

            JObject resultobj = JObject.Parse(resultstr);
            JArray resultobjfeatures = (JArray)resultobj["features"];
            foreach (var item in sourseobjfeatures)
            {
                resultobjfeatures.Insert(resultobjfeatures.Count, item);
            }
            resultobj["features"] = resultobjfeatures;

            return resultobj.ToString();
        }
    }
}
