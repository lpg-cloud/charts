using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aipuer.Common
{
    public class Module
    {
        public Module()
        {
            this.id = 0;
            this.name = "";
            this.url = "";
            this.title = "";
            this.pid = 0;
            this.layer_id = 0;
            this.type = "";
            this.iClass = "";
            //this.order = -1;
        }

        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public int pid { get; set; }
        public int layer_id { get; set; }
        public string type { get; set; }
        public string iClass { get; set; }
        public int order { get; set; }

    }

    public class User
    {
        public User()
        {
            this.id = 0;
            this.username = "";
            this.name = "";
            this.password = "";
            this.department = "";
            this.telephone = "";
            this.role_id = "";
            this.rolecode = "";
        }

        public int id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string department { get; set; }
        public string telephone { get; set; }
        public string role_id { get; set; }
        //权限标识集合
        public string rolecode { get; set; }
    }

    public class Role
    {
        public Role()
        {
            this.id = 0;
            this.name = "";
            this.role_flag = "";
            this.describe = "";
            this.module_ids = "";
            this.layer_ids = "";
        }

        public int id { get; set; }
        public string name { get; set; }
        public string role_flag { get; set; }
        public string describe { get; set; }
        public string module_ids { get; set; }
        public string layer_ids { get; set; }
    }

    public class Layer
    {
        public Layer()
        {
            this.id = 0;
            this.name = "";
            this.dataType = "";
            this.dataName = "";
            this.pointQuery = false;
            this.defaultOpen = false;
            this.shpColor = "";
            this.pid = 0;
            this.describe = "";
            this.files = "";
            this.index = -1;
            this.ordernumb = 9999999;
        }

        public int id { get; set; }
        public string name { get; set; }
        public string dataType { get; set; }
        public string dataName { get; set; }
        public bool pointQuery { get; set; }
        public bool defaultOpen { get; set; }
        public string shpColor { get; set; }
        public string describe { get; set; }
        public string files { get; set; }
        public int pid { get; set; }
        public int index { get; set; }
        public int ordernumb { get; set; }
    }




    public class Biaozhu
    {
        public Biaozhu()
        {
            this.id = 0;
            this.name = "";
            this.dataType = "";
            this.bzType = "";
            this.dataTime = "";
            this.coordinate = "";
            this.describe = "";
            this.length = "";
            this.area = "";
            this.filePath = "";
            this.sessionid = "";
            this.guid = "";
            this.height = "";
        }

        public int id { get; set; }
        public string name { get; set; }
        public string dataType { get; set; }
        public string bzType { get; set; }
        public string dataTime { get; set; }
        public string coordinate { get; set; }
        public string length { get; set; }
        public string describe { get; set; }
        public string area { get; set; }
        public string filePath { get; set; }
        public string sessionid { get; set; }
        public string guid { get; set; }
        public string height { get; set; }
    }
    public class department
    {
        public int id { get; set; }
        public string name { get; set; }
        public int pid { get; set; }
        public int order { get; set; }
    }
    public class module_function
    {
        public int id { get; set; }
        public string function_name { get; set; }
        public string power_flag { get; set; }
        public string function_url { get; set; }
        public int module_id { get; set; }
    }

    public class fenxipeizhi  //niujunpengxiugai
    {
        public fenxipeizhi()
        {
            this.id = 0;
            this.layername = "";
            this.layermc = "";
            this.state = false;
           
        }
        public int id { get; set; }
        public string layername { get; set; }
        public string layermc { get; set; }
        public bool state { get; set; }

    }

    //zsl 2019-07-20 规委会
    //gwh_meeting_xm
    public class gwh_meeting_xm
    {
        public gwh_meeting_xm(){
            id = 0;
            xm_type_id = 0;
            name = "";
            geom = "";
            ydxz = "";
            mj = "";
            zdwz = "";
            sm = "";
            clyj = "";
            layer = "";
            filepath = "";
            xh = 0;
            cad="";
            model="";
        }
        public int id { get; set; }
        public int xm_type_id { get; set; }
        public string name { get; set; }
        public string geom { get; set; }
        public string ydxz { get; set; }
        public string mj { get; set; }
        public string zdwz { get; set; }
        public string sm { get; set; }
        public string clyj { get; set; }
        public string layer { get; set; }
        public string filepath { get; set; }
        public int xh { get; set; }
        public string cad { get; set; }
        public string model { get; set; }
    }


    public class gwh_model{
        public gwh_model(){
            id=0;
            name="";
            path="";
            xh="";
            position = "";
            z = "";
            heading="";
            pitch="";
            roll= "";
            cameraposition = "";
            guid = "";
            bottomPos="";
            planePos="";
            wallPos="";
        }
        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        //数据库中该字段为int,为了便于操作，这里设置为string
        public string xh { get; set; }
        public string position { get; set; }
        public string z { get; set; }
        public string heading { get; set; }
        public string pitch { get; set; }
        public string roll { get; set; }
        public string cameraposition { get; set; }
        public string guid { get; set; }
        public string bottomPos { get; set; }
        public string planePos { get; set; }
        public string wallPos { get; set; }
    }


    public class gwh_meeting_modelroad
    {
        public gwh_meeting_modelroad()
        {
            id = 0;
            roadname = "";
            roadwkt = "";
            guid = "";
        }
        public int id { get; set; }
        public string roadname { get; set; }
        public string roadwkt { get; set; }
        public string guid { get; set; }
    }
}
