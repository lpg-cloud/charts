using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Aipuer.Common
{
    public class DBbeifen
    {
      
        //备份
        public string  BackUp(HttpContext context)
        {
            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();//开始计时

            string Spath = context.Request.MapPath("~");
            string gd= Guid.NewGuid().ToString();
            string path1 = Spath + "resource/DBbeifen/" + gd + "/";
            Directory.CreateDirectory(path1);//创建文件路径
            Process theProcess = new Process();//创建一个进程
            string SP = path1.Replace("\\", "/");  //转换为cmd可执行路径           
            string pathDBname = SP + "weiduqu_onemap";
            string table1 = "weiduqu_onemap";
          
            //cmd中postgresql备份命令
            string arguments = @"--host localhost --port 5432 --username ""postgres"" --no-password  --format custom --blobs --verbose --file "+ pathDBname + " "+ table1 + "";
            //cmd中postgresql--pg_dump.exe启动     
            theProcess.StartInfo.FileName = @"C:\Program Files\PostgreSQL\10\bin\pg_dump.exe";
            theProcess.StartInfo.Arguments = arguments;
            theProcess.Start();
            theProcess.WaitForExit();
            theProcess.Close();
            System.Diagnostics.Process.Start(SP);//打开路径

            //watch.Stop();//停止计时
            //return (watch.ElapsedMilliseconds * 0.001).ToString();//输出时间 秒

            return "success";
        }
        //还原
        public static string Restore(HttpContext context, string conn)
        {
            //System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //watch.Start();//开始计时

            string Spath = context.Request.MapPath("~");
            string gd = Guid.NewGuid().ToString();
            string path1 = Spath + "resource/DBhuanyuan/" + gd + "/";
            Directory.CreateDirectory(path1);//创建文件路径          
            string path2="";
            var upImg1 = context.Request.Files;//获取上传的文件
            for (var i = 0; i < upImg1.Count; i++)
            {
                var upImg = context.Request.Files[i];
                string fileName = System.IO.Path.GetFileName(upImg.FileName);//获取文件名
                path2 = path1 + fileName;
                upImg.SaveAs(path2);
                path2 = path2.Replace("\\", "/");//转换为cmd可执行路径  
                //以下为重点
                string str1 = "DROP SCHEMA public CASCADE;";//删除 public
                string str2 = "DROP SCHEMA tiger CASCADE;"; //删除 tiger
                string str3 = "DROP SCHEMA tiger_data CASCADE;";//删除 tiger_data
                string str4 = "DROP SCHEMA topology CASCADE;";//删除 topology
                string str5 = "CREATE SCHEMA public;";//创建 public
                PostgreHelper ph = new PostgreHelper(conn);
                ph.changeTable(str1);
                ph.changeTable(str2);
                ph.changeTable(str3);
                ph.changeTable(str4);
                ph.changeTable(str5);

                Process theProcess = new Process();
                string arguments = @"--host localhost --port 5432 --username ""postgres"" --dbname ""weiduqu_onemap"" --no-password  --verbose " + path2 + "";
                theProcess.StartInfo.FileName = @"C:\Program Files\PostgreSQL\10\bin\pg_restore.exe";
                theProcess.StartInfo.Arguments = arguments;
                theProcess.Start();
                theProcess.WaitForExit();
                theProcess.Close();
                                           
            }
            //watch.Stop();//停止计时
            //return (watch.ElapsedMilliseconds * 0.001).ToString();//输出时间 秒
            return "success";

        }


    }
   


}
