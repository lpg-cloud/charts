using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Mvc;
using Aipuer.Common;
using Newtonsoft.Json;

namespace charts.Controllers
{
    public class HomeController : Controller
    {
        PostgreHelper dbhelper = new PostgreHelper();
        
        
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public string Tables()
        {
            string result = "";
            string dataString = "";
            try
            {
                dataString = dbhelper.GetTableNames();
            }
            catch (Exception e)
            {
                dataString = "'"+e.Message.ToString()+"'";
            }
            finally
            {
                result = $"{{\"message\":\"faild\",\"data\":{dataString}}}";
            }

            return result;
        }
        public string Fields(string id)
        {
            string result = "";
            string dataString = "";
            try
            {
                dataString = dbhelper.GetFieldNames(id);
            }
            catch (Exception e)
            {
                dataString = "'" + e.Message.ToString() + "'";
            }
            finally
            {
                result = $"{{\"message\":\"success\",\"data\":{dataString}}}";
            }

            return result;
        }
        [HttpPost]
        public string Data(FormCollection collection)
        {
            
            string result = "";
            string tableName = collection.Get("tableName");

            string types = collection.Get("types[]");
            string fields = collection.Get("fields[]");
            
            Dictionary<string,string> fieldDic=new  Dictionary<string, string>();

            string[] typesArray= types.Split(',');
            string[] fieldsArray= fields.Split(',');
            
            List<string> select =new List<string>();
            List<string> textFields=new List<string>();
            

            for (int i = 0; i < fieldsArray.Length; i++)
            {
                if (fieldsArray[i]!="year")
                {
                    if (typesArray[i] == "n")
                    {
                        select.Add($"sum({fieldsArray[i]}::float) as { fieldsArray[i]}");
                    }
                    else
                    {
                        select.Add(fieldsArray[i]);
                        textFields.Add(fieldsArray[i]);
                    }
                }
            }

            string sql = $"select {string.Join(",",select.ToArray())} from {tableName} group by {string.Join(",",textFields)}";
            
            string dataString = "";
            try
            {
                dataString = dbhelper.GetAsJson(CommandType.Text, sql);
            }
            catch ( Exception e )
            {
                dataString ="'" +e.Message+"'";
            }
            result = $"{{\"message\":\"success\",\"data\":{dataString} }}";
            return result;
        }
    }
}