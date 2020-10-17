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

            Dictionary<string, string> tableDic = new Dictionary<string, string>
            {
                { "草原", "grass" },
                 { "天然草改良", "naturegrassbetter" },
                { "草原执法", "pereport" },
                { "人工草地病虫害", "pestdisease" },
                { "鼠害", "pestmouse" },
                { "虫害", "pestinsect" },
                { "产草量", "prosperitysample" },
                { "毒草害", "pestweeds" },
                { "人工草地建设", "rgcdjs" }
            };

            string tableName = collection.Get("tableName");

            
            string numField = collection.Get("numField");
            if (numField==null)
            {
                numField = collection.Get("numField[]");
            }
            string txtField = collection.Get("txtField");
            string sql = "";
            if (txtField!=null) {
                sql = $"select {numField + "," + txtField} from {tableName} group by {txtField}";
            }
            else
            {
                sql = $"select {numField} from {tableName} group by {txtField}";
            }

            
            string dataString = "";
            try
            {
                dataString = dbhelper.GetAsJson(CommandType.Text, sql);
            }
            catch ( Exception e )
            {
                dataString ="," +e.Message+",";
            }
            result = $"{{\"message\":\"success\",\"data\":{dataString} }}";
            return result;
        }
        [HttpPost]
        public string PreData(FormCollection collection)
        {

            string result = "";

            string tableName = collection.Get("tableName");
           
            string fields = collection.Get("fields[]");

            string[] fields_ = fields.Split(',');
            List<string> textfield = new List<string>();
            List<string> numfield = new List<string>();
            foreach (string value in fields_)
            {
                if (!RowIsNumber(value)) {
                    textfield.Add(value);
                }
                else
                {
                    numfield.Add("sum("+value+")");
                }
            }
            string sql = "";
            if (textfield.Count>0) {
                sql = $"select {string.Join(",", textfield) + string.Join(",", numfield)} from {tableName} group by {string.Join(",", textfield)}";
            }
            else
            {
                sql = $"select { string.Join(",", numfield)} from {tableName} group by {string.Join(",", textfield)}";
            }
            
            string dataString = "";
            try
            {
                dataString = dbhelper.GetAsJson(CommandType.Text, sql);
            }
            catch (Exception e)
            {
                dataString = "," + e.Message + ",";
            }
            result = $"{{\"message\":\"success\",\"data\":{dataString} }}";

            return result;
        }

        public bool RowIsNumber(string str)
        {
            bool flag = true;
            try {
                Convert.ToDecimal(str);
            }
            catch
            {
                flag = false;
            }
            return flag;
        }
    }
}