using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Aipuer.Common
{
    public class DtreeJson
    {
        ///<summary>
        ///节点的实体类，记录了数据库中的3个字段
        ///</summary>
        public class Node
        {
            public int id { get; set; }
            public int pid { get; set; }
            public string name { get; set; }

            ///<summary>
            ///basicData
            ///</summary>
            public string title { get; set; }
            public string url { get; set; }
            public int layer_id { get; set; }
            public string type { get; set; }
            public string iClass { get; set; }
            public int order { get; set; }
            public List<Node> children { get; set; }
            ///<summary>
            ///当你选择开启复选框时，除了必须指定checkbar:true之外，数据中必须存在checkArr字段，复选框才能正常显示。
            ///</summary>
            public string checkArr { get; set; }
            public string basicData { get; set; }
        }
        public string createTree(DataTable dt)
        {
            //DataTable转换为IList<实体类>，得到要转化的树形结构数据
            IList<Node> list = ModelConvertHelper<Node>.ConvertToIList(dt);
            List<Node> lists = new List<Node>();
            //查询最外层
            List<Node> roots = list.Where(c => c.pid == 0).OrderBy(c => c.order).ToList();
            if (roots == null)
            {
                return lists.ToString();
            }
            lists.AddRange(roots);
            while (true)
            {
                if (roots == null || roots.Count == 0)
                {
                    break;
                }
                Node[] cachNodes = roots.ToArray();
                roots.Clear();
                for (int i = 0; i < cachNodes.Length; i++)
                {
                    Node pNode = cachNodes[i];
                    int pIdNode = pNode.id;
                    //
                    List<Node> subs = list.Where(c => c.pid == pIdNode).OrderBy(c => c.order).ToList();
                    if (subs != null)
                    {
                        pNode.children = subs;
                        roots.AddRange(subs);
                    }
                }
            }
            string res = JsonConvert.SerializeObject(lists);
            return res;
        }
        /// <summary>    
        /// 实体转换辅助类    
        /// </summary>   
        public class ModelConvertHelper<T> where T : new()
        {
            public static IList<T> ConvertToIList(DataTable dt)
            {
                // 定义集合    
                IList<T> ts = new List<T>();
                // 获得此模型的类型   
                Type type = typeof(T);
                string tempName = "";
                foreach (DataRow dr in dt.Rows)
                {
                     T t = new T();
                    // 获得此模型的公共属性      
                    PropertyInfo[] propertys = t.GetType().GetProperties();
                    foreach (PropertyInfo pi in propertys)
                    {
                        tempName = pi.Name;  // 检查DataTable是否包含此列  
                        if (dt.Columns.Contains(tempName))
                        {
                            // 判断此属性是否有Setter      
                            if (!pi.CanWrite) continue;
                            object value = dr[tempName];
                            if (value != DBNull.Value)
                            pi.SetValue(t, value, null);
                        }
                        if (tempName == "basicData")
                        {
                            JObject basicData = new JObject();
                            foreach (var item in dt.Columns)
                            {
                                string colName = item.ToString();
                                if (colName != "id" && colName != "pid" && colName != "name") {
                                    basicData.Add(colName, dr[colName].ToString());
                                }
                            }
                            pi.SetValue(t, basicData.ToString(), null);
                        }
                        if (tempName == "checkArr") {
                            pi.SetValue(t, "0", null);
                        }
                    }
                    ts.Add(t);
                }
                return ts;
            }
        }
    }
}
