using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;

namespace Aipuer.Common
{
    public class Meeting_gwh
    {
        private IDBHelper dbHelper = null;
        public Meeting_gwh() { }
        public Meeting_gwh(string connectionString)
        {
            dbHelper = new PostgreHelper(connectionString);
        }

        #region 增/改
        //会议列表
        public string changeMeeting(string id, string name)
        {
            string sql = "";
            string result = "";
            if (!string.IsNullOrEmpty(id))
            {
                sql = "update gwh_meeting_list set name = '" + name + "' where id = " + id;
                int _result = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                result = _result == 0 ? "-1" : id;
            }
            else
            {
                sql = "insert into gwh_meeting_list(name) values('" + name + "') RETURNING id";
                result = dbHelper.GetOneValue(CommandType.Text, sql);
            }
            return string.IsNullOrEmpty(result) ? "-1" : result;
        }

        //项目类型
        public string ChangeXmType(string id, string meetingId, string title)
        {
            string sql = "";
            string result = "-1";
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(title))
            {
                sql = "update gwh_meeting_xmtype set title = '" + title + "' where id = " + id;
                result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
                if (result != "0")
                    result = id;
            }
            else if (!string.IsNullOrEmpty(meetingId) && !string.IsNullOrEmpty(title) && string.IsNullOrEmpty(id))
            {
                sql = "select count(xh) from gwh_meeting_xmtype where meeting_id = " + meetingId;
                string count = dbHelper.GetOneValue(CommandType.Text, sql);
                int xh = 0;
                if (int.TryParse(count, out xh))
                {
                    if (xh > 0)
                    {
                        sql = "select xh from gwh_meeting_xmtype where meeting_id = " + meetingId + " order by xh desc limit 1";
                        count = dbHelper.GetOneValue(CommandType.Text, sql);
                        xh = int.Parse(count) + 1;
                    }
                    sql = "insert into gwh_meeting_xmtype(meeting_id, title, xh) values(" + meetingId + ",'" + title + "'," + xh + ") RETURNING id";
                    result = dbHelper.GetOneValue(CommandType.Text, sql);
                }
            }
            if (string.IsNullOrEmpty(sql))
            {
                result = "-1";
            }
            return result;
        }

        //项目添加
        public string editXmData(gwh_meeting_xm model)
        {
            string sql = "";
            string result = "";
            int xh = 0;
            model.cad = model.cad.Replace("\\", "/");
            model.model = model.model.Replace("\\", "/");
            if (model.id == 0)
            {
                sql = "select count(xh) from gwh_meeting_xm where xm_type_id = " + model.xm_type_id;
                string count = dbHelper.GetOneValue(CommandType.Text, sql);
                if (int.TryParse(count, out xh))
                {
                    if (xh > 0)
                    {
                        sql = "select xh from gwh_meeting_xm where xm_type_id = " + model.xm_type_id + " order by xh desc limit 1";
                        count = dbHelper.GetOneValue(CommandType.Text, sql);
                        xh = int.Parse(count) + 1;
                    }
                }
                sql = "insert into gwh_meeting_xm(xm_type_id,name,geom,ydxz,mj,zdwz,sm,clyj,layer,filepath,xh,cad,model)" +
                " values('" +
                model.xm_type_id + "','" +
                model.name + "','" +
                model.geom + "','" +
                model.ydxz + "','" +
                model.mj + "','" +
                model.zdwz + "','" +
                model.sm + "','" +
                model.clyj + "','" +
                model.layer + "','" +
                model.filepath + "','" +
                xh + "','" +
                model.cad + "','" +
                model.model + "') RETURNING id";
            }
            else
            {
                sql = "update [gwh_meeting_xm] set " +
                    "[xm_type_id] = '" + model.xm_type_id + "'," +
                    "[name] = '" + model.name + "'," +
                    "[geom] = '" + model.geom + "'," +
                    "[ydxz] = '" + model.ydxz + "'," +
                    "[mj] = '" + model.mj + "'," +
                    "[zdwz] = '" + model.zdwz + "'," +
                    "[sm] = '" + model.sm + "'," +
                    "[clyj] = '" + model.clyj + "'," +
                    "[layer] = '" + model.layer + "'," +
                    "[filepath] = '" + model.filepath + "'," +
                    "[cad] = '" + model.cad + "'," +
                    "[model] = '" + model.model + "'" +
                    "where id = " + model.id;
            }
            try
            {
                if (model.id == 0)
                {
                    result = dbHelper.GetOneValue(CommandType.Text, sql);
                }
                else
                {
                    int _count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                    result = _count != 0 ? model.id.ToString() : "-1";
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        //模型添加
        public string saveModel(gwh_model model)
        {
            string sql = "";
            string result = "-1";
            if (model.id == 0)
            {
                sql = "insert into gwh_model(name, path, xh, guid)"+
                "values('"+
                model.name+"','"+
                model.path+"','"+
                model.xh+"','"+
                model.guid+"') RETURNING id";
                result = dbHelper.GetOneValue(CommandType.Text, sql);
            }
            else
            {
                sql = "update gwh_model set ";
                string _sql = "";
                if (!string.IsNullOrEmpty(model.name))
                {
                    _sql += canAdd_sql(_sql) + " name = \'" + model.name + "\' ";
                }
                if (!string.IsNullOrEmpty(model.path))
                {
                    _sql += canAdd_sql(_sql) + " path = \'" + model.path + "\' ";
                }
                if (!string.IsNullOrEmpty(model.xh))
                {
                    _sql += canAdd_sql(_sql) + " xh = \'" + model.xh + "\' ";
                }
                if (!string.IsNullOrEmpty(model.position))
                {
                    _sql += canAdd_sql(_sql) + " position = \'" + model.position + "\' ";
                }
                if (!string.IsNullOrEmpty(model.z))
                {
                    _sql += canAdd_sql(_sql) + " z = \'" + model.z + "\' ";
                }
                if (!string.IsNullOrEmpty(model.heading))
                {
                    _sql += canAdd_sql(_sql) + " heading = \'" + model.heading + "\' ";
                }
                if (!string.IsNullOrEmpty(model.pitch))
                {
                    _sql += canAdd_sql(_sql) + " pitch = \'" + model.pitch + "\' ";
                }
                if (!string.IsNullOrEmpty(model.roll))
                {
                    _sql += canAdd_sql(_sql) + " roll = \'" + model.roll + "\' ";
                }
                if (!string.IsNullOrEmpty(model.cameraposition))
                {
                    _sql += canAdd_sql(_sql) + " cameraposition = \'" + model.cameraposition + "\' ";
                }
                if (!string.IsNullOrEmpty(model.guid))
                {
                    _sql += canAdd_sql(_sql) + " guid = \'" + model.guid + "\' ";
                }
                if (!string.IsNullOrEmpty(model.bottomPos))
                {
                    _sql += canAdd_sql(_sql) + " bottompos = \'" + model.bottomPos + "\' ";
                }
                if (!string.IsNullOrEmpty(model.planePos))
                {
                    _sql += canAdd_sql(_sql) + " planepos = \'" + model.planePos + "\' ";
                }
                if (!string.IsNullOrEmpty(model.wallPos))
                {
                    _sql += canAdd_sql(_sql) + " wallpos = \'" + model.wallPos + "\' ";
                }
                sql += _sql + " where id = " + model.id;
                int count = 0;
                try
                {
                    count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                }
                catch(Exception ex)
                {

                }
                if (count != 0)
                {
                    result = model.id.ToString();
                }
            }
            return result;
        }
        public string AddXianLu(gwh_meeting_modelroad model)
        {
            string result = "-1";
            string sql = "";
            try
            {
                if (model.id != 0)
                {
                    sql = "update gwh_meeting_modelroad set " +
                        "roadname=\'" + model.roadname + "\', " +
                        "roadwkt=\'" + model.roadwkt + "\', " +
                        "guid=\'" + model.guid + "\' " +
                        "where id = " + model.id;
                    int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                    if (count > 0)
                    {
                        result = model.id.ToString();
                    }
                }
                else
                {
                    sql = "insert into gwh_meeting_modelroad (roadname, roadwkt, guid) values('" +
                        model.roadname + "','" +
                        model.roadwkt + "','" +
                        model.guid + "') RETURNING id";
                    result = dbHelper.GetOneValue(CommandType.Text, sql);
                }
            }
            catch (Exception ex)
            {
            }
            return result;
        }
        private string canAdd_sql(string sql)
        {
            string result = "";
            if (!string.IsNullOrEmpty(sql))
            {
                result = ",";
            }
            return result;
        }
        #endregion

        #region 查
        //得到某个项目的模型列表，按照xh升序 | 得到某个模型
        public string getModelList(string guid, string id)
        {
            string sql = "select * from gwh_model where 1 = 1 ";
            if (!string.IsNullOrEmpty(guid))
            {
                sql += " and guid = '" + guid + "' ";
            }
            if (!string.IsNullOrEmpty(id))
            {
                sql += " and id = '" + id + "' ";
            }
            sql += "order by xh asc";
            return dbHelper.GetAsJson(CommandType.Text, sql);
        }

        //得到所有的会议列表
        public string getAllMeeting()
        {
            string sql = "select * from gwh_meeting_list";
            return dbHelper.GetAsJson(CommandType.Text, sql);
        }

        //得到某会议下的所有类型|某个会议类型
        public string getXmType(string id, string meetingId)
        {
            string sql = "select * from gwh_meeting_xmtype where 1=1";
            if (!string.IsNullOrEmpty(id))
            {
                sql += " and id = " + id;
            }
            if (!string.IsNullOrEmpty(meetingId))
            {
                sql += " and meeting_id = " + meetingId;
            }
            sql += " order by xh asc";
            return dbHelper.GetAsJson(CommandType.Text, sql);
        }

        //得到某类型下所有的项目|某个项目
        public string getXm(string id, string xmTypeId)
        {
            string result = "";
            try
            {
                string sql = "select id, xm_type_id, name, ST_ASText(geom) as geom, ydxz, mj, zdwz, sm, clyj, layer, filepath, cad, model " +
            " from gwh_meeting_xm where 1=1 ";
                if (!string.IsNullOrEmpty(id))
                {
                    sql += " and id = " + id;
                }
                if (!string.IsNullOrEmpty(xmTypeId))
                {
                    sql += " and xm_type_id = " + xmTypeId;
                }
                sql += " order by xh asc";
                result = dbHelper.GetAsJson(CommandType.Text, sql);
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        //得到项目的cad位置
        public string getXmCAD(string id)
        {
            string sql = "select cad from gwh_meeting_xm where id=" + id + " limit 1";
            return dbHelper.GetOneValue(CommandType.Text, sql);
        }


        //得到某个模型的漫游路线
        public string manyouRoad(string id, string guid)
        {
            string result = "";
            string sql = "select * from gwh_meeting_modelroad where 1=1 ";
            if (!string.IsNullOrEmpty(id))
            {
                sql += " and id="+id;
            }
            if (!string.IsNullOrEmpty(guid))
            {
                sql += " and guid=\'"+guid+"\'";
            }
            result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        #endregion
        #region 删
        //删除项目
        public string deleteXm(string id)
        {
            string result = "-1";
            if (!string.IsNullOrEmpty(id))
            {
                string sql = "delete from gwh_meeting_xm where id = " + id;
                result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
                if (result == "0")
                    result = "-1";
            }
            return result;
        }

        //删除项目类型
        public string deleteXmType(string id)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(id))
            {
                sql = "delete from gwh_meeting_xmtype where id = " + id + ";";
                sql += "delete from gwh_meeting_xm where xm_type_id = " + id + ";";
            }
            return dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
        }

        //删除会议
        public string deleteMeeting(string id)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(id))
            {
                sql = "delete from gwh_meeting_xm t2 using gwh_meeting_xmtype t1 where t1.meeting_id = " + id + " and t2.xm_type_id = t1.id;";
                sql += "delete from gwh_meeting_xmtype where meeting_id = " + id + ";";
                sql += "delete from gwh_meeting_list where id = " + id;
            }
            return dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
        }
        
        //删除漫游路线
        public string del_manyouRoad(string id)
        {
            string sql = "";
            if (!string.IsNullOrEmpty(id))
            {
                sql = "delete from gwh_meeting_modelroad where id = " + id;
            }
            return dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
        }

        public string del_model(string id){
            string sql="";
            if(!string.IsNullOrEmpty(id)){
                sql="delete from gwh_model where id="+id+";";
                sql+="delete from tool_biaozhu where guid=\'"+id+"\';";
                sql+="delete from gwh_meeting_modelroad where guid=\'"+id+"\'";
            }
            return dbHelper.ExecuteNonQuery(CommandType.Text,sql).ToString();
        }
        #endregion
        #region 序号调整
        //项目序号调整
        public string changeXmXh(string ids)
        {
            string sql = "";
            string[] idArray = ids.Split(',');
            for (var i = 0; i < idArray.Length; i++)
            {
                sql += "update gwh_meeting_xm set xh=" + i + " where id=" + idArray[i] + ";";
            }
            string result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
            return result == "0" ? "-1" : result;

        }

        //类型序号调整
        public string changeXmTypeXh(string ids)
        {
            string sql = "";
            string[] idArray = ids.Split(',');
            for (var i = 0; i < idArray.Length; i++)
            {
                sql += "update gwh_meeting_xmtype set xh=" + i + " where id=" + idArray[i] + ";";
            }
            string result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
            return result == "0" ? "-1" : result;
        }

        #endregion
        #region 文件上传
        public string saveFileType(string xmid, string type, string sub_type)
        {
            // string type_config = "其他";
            // switch (type)
            // {
            //     case "yingxiang":
            //     case "shipin":
            //     case "quanjing":
            //         type_config = "航拍现状";
            //         break;
            //     case "konggui":
            //         type_config = "控规图";
            //         break;
            //     case "huiyiziliao":
            //         type_config = "上会资料";
            //         break;
            //     default:
            //         break;
            // }
            // string sql = "delete from meeting_file_type where xm_id = "+ xmid + " and type='" + type + "' and sub_type='" + sub_type + "'; insert into meeting_file_type(type, xm_id, sub_type) values('" + type + "', " + xmid + ",'" + sub_type + "');delete from meeting_config where pid = "+xmid+" and mc = '"+type_config+"'; insert into meeting_config(mc,pid,checked) values('"+type_config+"',"+xmid+",'1')";
            // return dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
            return "1";
        }
        #endregion
    }
}
