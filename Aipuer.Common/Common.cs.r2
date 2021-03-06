﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;

namespace Aipuer.Common
{
    public class Common
    {
        public IDBHelper dbHelper = null;

        public Common()
        {
        }

        public Common(string connectionString)
        {
            dbHelper = new PostgreHelper(connectionString);
        }

        public string getPagerData(string tableName, string page, string size, Dictionary<string, List<string>> where_fieldsValue = null)
        {
            string result = "";
            if (tableName == "fenxipeizhi"&& page==null&& size==null) //niujunpengxiugai
            {
                string sql = "select * from public." + tableName +" where state=true";
                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();
            }
            else {
                string sql = "select * from public." + tableName;
                if (tableName == "sys_module" || tableName == "sys_layer")
                {
                    sql = "select ta.*, tb.name as pName from public." + tableName + " as ta left join public." + tableName + " as tb on ta.pid = tb.id";
                }
                else if (tableName == "sys_user")
                {
                    //sql = "select ta.*, tb.name as roleName from public." + tableName + " as ta left join public.sys_role as tb on ta.role_id = tb.id";
                    sql = $"select ta.*, tb.rolenames from {tableName} as ta left join (select id ,array_to_string( array_agg(roleName), ',') as roleNames from (select ta.id, tb.name as roleName from(select id, unnest(string_to_array(role_id, ','))::int as roleid from {tableName}) as ta left join public.sys_role as tb on ta.roleid = tb.id) tc group by id order by id asc) as tb on ta.id = tb.id";
                }
                else if (tableName == "sys_role")
                {
                    sql = "select r.*,(select string_agg(trim(to_char(rm.module_id, '999999999999')), ',') from public.sys_role_module rm where rm.role_id = r.id) as moduleIds,(select string_agg(m.name,',') from public.sys_role_module rm, public.sys_module m where rm.role_id = r.id and m.id= rm.module_id) as moduleNames,(select string_agg(trim(to_char(rl.layer_id,'999999999999')),',') from public.sys_role_layer rl where rl.role_id = r.id) as layerIds,(select string_agg(l.name,',') from public.sys_role_layer rl, public.sys_layer l where rl.role_id = r.id and l.id= rl.layer_id) as layerNames from public." + tableName + " as r";
                }
                string sqlCount = "select count(*) from public." + tableName;
                string whereSql = "", whereSqlCount = "";

                //模糊查询
                if (where_fieldsValue != null && where_fieldsValue.Count == 1)
                {
                    string keywords = "";
                    List<string> queryFields = new List<string>();
                    KeyValuePair<string, List<string>> first = where_fieldsValue.First();
                    keywords = first.Key;
                    queryFields = first.Value;
                    for (int i = 0; i < queryFields.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(queryFields[i]))
                        {

                            if (i == 0)
                            {
                                // ta.多条件存在不一致
                                if (tableName == "sys_module_function")
                                {
                                    whereSql += " and (" + queryFields[i] + " = '" + keywords + "'";
                                    whereSqlCount += " and (" + queryFields[i] + " = '" + keywords + "'";
                                }

                                else if (tableName == "fenxipeizhi")//niujunpengxiugai
                                {
                                    whereSql += " and (" + queryFields[i] + " like '%" + keywords + "%'";
                                    whereSqlCount += " and (" + queryFields[i] + " like '%" + keywords + "%'";
                                }
                                else
                                {
                                    whereSql += " and (" + (tableName == "sys_role" ? "r." : "ta.") + "\"" + queryFields[i] + "\" like '%" + keywords + "%'";
                                    whereSqlCount += " and (\"" + queryFields[i] + "\" like '%" + keywords + "%'";
                                }
                            }
                            else
                            {
                                if (tableName == "fenxipeizhi")//niujunpengxiugai
                                {
                                    whereSql += " or \"" + queryFields[i] + "\" like '%" + keywords + "%'";
                                    whereSqlCount += " or \"" + queryFields[i] + "\" like '%" + keywords + "%'";
                                }
                                else
                                {
                                    whereSql += " or " + (tableName == "sys_role" ? "r." : "ta.") + "\"" + queryFields[i] + "\" like '%" + keywords + "%'";
                                    whereSqlCount += " or \"" + queryFields[i] + "\" like '%" + keywords + "%'";
                                }
                               
                            }
                            if (i == queryFields.Count - 1)
                            {
                                whereSql += ")";
                                whereSqlCount += ")";
                            }
                        }
                    }
                }
                try
                {
                    int start = (Int32.Parse(page) - 1) * Int32.Parse(size);
                    if (!string.IsNullOrEmpty(whereSql))
                    {
                        sql += " where 1 = 1" + whereSql;
                        sqlCount += " where 1 = 1" + whereSqlCount;
                    }
                    sql += " order by id limit " + size + " offset " + start;

                    result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                    int count = dbHelper.GetCount(sqlCount);

                    result = "{\"result\":\"success\",\"count\":\"" + count + "\",\"data\":" + result + "}";
                }
                catch (Exception e)
                {
                    //result = e.ToString();
                    result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
                }
            }
           
            return result;
        }

        public string getModulesAndLayersByUid()
        {
            string result = "";

            return result;
        }

        public string getById(string tableName, string id)
        {
            string result = "";
            string sql = "select * from public." + tableName + " where id = " + id;
            if (tableName == "sys_role")
            {
                sql = "select *,(select string_agg(trim(to_char(rm.module_id,'999999999999')),',') from public.sys_role_module rm where rm.role_id = r.id) as moduleIds,(select string_agg(trim(to_char(rl.layer_id,'999999999999')),',') from public.sys_role_layer rl where rl.role_id = r.id) as layerIds from public." + tableName + " as r where id = " + id;
            }
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                //result = e.ToString();
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }
        public string getCountByCol(string tableName, string val,string col)
        {
            string result = "";
            string sql = $"select count(*) from {tableName} where {col} = {val}";
            try
            {
                int count = dbHelper.GetCount(sql);

                result = "{\"result\":\"success\",\"count\":" + count + "}";
            }
            catch (Exception e)
            {
                //result = e.ToString();
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }
        public string getByRoleXzqId(string tableName, string id)
        {
            string result = "";
            string sql = "select * from public." + tableName + " where id = " + id;
            DataTable dt= dbHelper.GetDataTable(CommandType.Text, sql);
           
            if (tableName == "sys_user")
            {
                sql = "select *,(select string_agg(trim(to_char(ux.xid,'999999999999')),',') from  public.sys_user_xzq ux where ux.uid = u.id) as assigned from sys_user,public.sys_user as u where u.id=sys_user.id and u.id = " + dt.Rows[0]["id"];
               //sql = "select *,(select string_agg(trim(to_char(rm.module_id,'999999999999')),',') from public.sys_xzq rm where rm.role_id = r.id) as moduleIds,(select string_agg(trim(to_char(rl.layer_id,'999999999999')),',') from public.sys_role_layer rl where rl.role_id = r.id) as layerIds from public." + tableName + " as r where id = " + id;
            }
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();
                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                //result = e.ToString();
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }
        public string saveModule(Module model)
        {
            string sql = "insert into[sys_module]([name],[url],[layer_id],[title],[pid],[type],[iClass],[order]) values(@name, @url, @layer_id, @title, @pid, @type, @iClass, @order) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_module] set [name]=@name,[url]=@url,[layer_id]=@layer_id,[title]=@title,[pid]=@pid,[type]=@type,[iClass]=@iClass,[order]=@order where id=@id";
            }

            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@url", model.url));
            parameter.Add(new Npgsql.NpgsqlParameter("@layer_id", model.layer_id));
            parameter.Add(new Npgsql.NpgsqlParameter("@title", model.title));
            parameter.Add(new Npgsql.NpgsqlParameter("@pid", model.pid));
            parameter.Add(new Npgsql.NpgsqlParameter("@type", model.type));
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@iClass", model.iClass));
            parameter.Add(new Npgsql.NpgsqlParameter("@order", model.order));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveUser(User model)
        {
            string sql = "insert into[sys_user]([username],[name],[password],[department],[telephone],[role_id]) values(@username, @name, @password, @department, @telephone, @role_id) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_user] set [username]=@username,[name]=@name,[password]=@password,[department]=@department,[role_id]=@role_id,[telephone]=@telephone where id=@id";
            }

            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@username", model.username));
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@password", model.password));
            parameter.Add(new Npgsql.NpgsqlParameter("@department", model.department));
            parameter.Add(new Npgsql.NpgsqlParameter("@telephone", model.telephone));
            parameter.Add(new Npgsql.NpgsqlParameter("@role_id", model.role_id));
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveRole(Role model)
        {
            try
            {
                string sql = "insert into[sys_role]([name],[role_flag],[describe]) values(@name,@role_flag, @describe) RETURNING id;";
                if (model.id != 0)
                {
                    sql = "update [sys_role] set [name]=@name,[role_flag]=@role_flag,[describe]=@describe where id=@id";
                }
                List<DbParameter> parameter = new List<DbParameter>();
                parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
                parameter.Add(new Npgsql.NpgsqlParameter("@role_flag", model.role_flag));
                parameter.Add(new Npgsql.NpgsqlParameter("@describe", model.describe));
                parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));

                string result = "";
                if (model.id == 0)
                {
                    result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
                }
                else
                {
                    dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                    result = model.id.ToString();
                }
                return result;
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public string saveRole_Relation(string table,string id,string ids,string idsParm)
        {
            try
            {
                //先删除
                string subSql = "delete from "+ table + " where role_id=" + id;
                dbHelper.ExecuteNonQuery(CommandType.Text, subSql);
                //再插入
                if (!string.IsNullOrEmpty(ids))
                {
                    string[] _id = ids.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    for (var i = 0; i < _id.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(_id[i]))
                        {
                            subSql = $"insert into {table} ([role_id],{idsParm}) values('{id}', '{_id[i]}')";
                            dbHelper.ExecuteNonQuery(CommandType.Text, subSql);
                        }
                    }
                }
                string result = "success";
                return result;
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        public string saveLayer(Layer model)
        {
            string sql = "insert into[sys_layer]([name],[dataType],[dataName],[pointQuery],[shpColor],[describe],[files],[pid],[index],[defaultOpen],[ordernumb]) values(@name, @datatype, @dataname, @pointquery, @shpcolor, @describe, @files, @pid, @index, @defaultOpen, @ordernumb) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_layer] set [name]=@name,[dataType]=@dataType,[dataName]=@dataName,[pointQuery]=@pointQuery,[shpColor]=@shpColor,[describe]=@describe,[files]=@files,[pid]=@pid,[index]=@index,[defaultOpen]=@defaultOpen,[ordernumb]=@ordernumb where id=@id";
            }
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@datatype", model.dataType));
            parameter.Add(new Npgsql.NpgsqlParameter("@dataname", model.dataName));
            parameter.Add(new Npgsql.NpgsqlParameter("@pointquery", model.pointQuery));
            parameter.Add(new Npgsql.NpgsqlParameter("@shpcolor", model.shpColor));
            parameter.Add(new Npgsql.NpgsqlParameter("@describe", model.describe));
            parameter.Add(new Npgsql.NpgsqlParameter("@files", model.files));
            parameter.Add(new Npgsql.NpgsqlParameter("@pid", model.pid));
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@index", model.index));
            parameter.Add(new Npgsql.NpgsqlParameter("@defaultOpen", model.defaultOpen));
            parameter.Add(new Npgsql.NpgsqlParameter("@ordernumb", model.ordernumb));

            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveDepartment(department model)
        {
            string sql = "insert into[sys_department]([name],[pid],[order]) values(@name, @pid, @order) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_department] set [name]=@name where id=@id";
            }
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@order", model.order));
            parameter.Add(new Npgsql.NpgsqlParameter("@pid", model.pid));
            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveModule_function(module_function model)
        {
            string sql = "insert into[sys_module_function]([function_name],[function_url],[power_flag],[module_id]) values(@function_name, @function_url, @power_flag,@module_id) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [sys_module_function] set [function_name]=@function_name,[function_url]=@function_url,[power_flag]=@power_flag,[module_id]=@module_id where id=@id";
            }
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@function_name", model.function_name));
            parameter.Add(new Npgsql.NpgsqlParameter("@function_url", model.function_url));
            parameter.Add(new Npgsql.NpgsqlParameter("@power_flag", model.power_flag));
            parameter.Add(new Npgsql.NpgsqlParameter("@module_id", model.module_id));
            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string saveModule_fenxilayer(fenxipeizhi model)
        {
            string sql = "insert into[fenxipeizhi]([layername],[layermc],[state]) values(@layername, @layermc, @state) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [fenxipeizhi] set [layername]=@layername,[layermc]=@layermc,[state]=@state where id=@id";
            }
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@layername", model.layername));
            parameter.Add(new Npgsql.NpgsqlParameter("@layermc", model.layermc));
            parameter.Add(new Npgsql.NpgsqlParameter("@state", model.state));
  
            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string getByPid(string tableName, string pid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public." + tableName + " where pid = " + pid;

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string getArray_to_string(string col, string tableName, string roleID)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = $"select array_to_string( array_agg({col}), ',') as result from {tableName} where role_id = " + roleID;

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string getRole()
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_role";

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string deleteById(string tableName, string id)
        {
            string result = "";
            string sql = "";
            try
            {
                string wherePlus = "";
                //删除子节点数据
                if (tableName == "sys_module" || tableName == "sys_layer")
                {
                    wherePlus = " or pid = " + id;
                }
                sql = "delete from public." + tableName + " where id = " + id + wherePlus;

                result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();

                result = "{\"result\":\"success\",\"data\":\"" + result + "\"}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"" + e.ToString() + "\"}";
            }
            return result;
        }

        public string getModuleData(string checkIds, string checkbox = null)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_module order by id";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (checkIds == null) checkIds = "";
                List<string> ids = checkIds.Split(',').ToList();

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadModuleData("0", dt, ids) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }
        public string getAssignedData(string checkIds, string checkbox = null)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_xzq order by id";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (checkIds == null) checkIds = "";
                List<string> ids = checkIds.Split(',').ToList();

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadAssignedData("0", dt, ids) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        private string loadAssignedData(string pid, DataTable dt, List<string> ids = null)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.RowFilter = "pid=" + pid;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    string id = dv[i]["id"].ToString();
                    string _pid = dv[i]["pid"].ToString();
                    if (_pid == pid)
                    {
                        string children = loadAssignedData(id, dt, ids);
                        var isChecked = "0";
                        if (ids != null && ids.IndexOf(id) != -1)
                        {
                            isChecked = "1";
                        }
                        result += "{";
                        result += "\"id\":\"" + id + "\",";
                        result += "\"title\":\"" + dv[i]["name"] + "\",";
                        result += "\"checkArr\":[{\"type\": \"0\", \"isChecked\": \"" + isChecked + "\"}],";
                        result += "\"parentId\":\"" + pid + "\"";
                        if (children != "")
                        {
                            result += ",\"isLast\":false";
                            result += ",\"children\":[" + children + "]";
                        }
                        {
                            DataView dvlast = new DataView();
                            dvlast.Table = dt;
                            dvlast.RowFilter = "pid=" + id;
                            if (dvlast.Count == 0 || object.Equals(dvlast, null))
                            {
                                result += ",\"isLast\":true";
                            }
                        }
                        result += "}";
                        if (i != dv.Count - 1)
                        {
                            result += ",";
                        }
                    }
                }
            }
            return result;
        }
        public string getXZQModule(string checkIds, string checkbox = null)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_xzq order by id desc";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (checkIds == null) checkIds = "";
                List<string> ids = checkIds.Split(',').ToList();

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadLayerData("0", dt, ids, checkbox) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }
        public string getLayerData(string checkIds, string checkbox = null)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select * from public.sys_layer order by id";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (checkIds == null) checkIds = "";
                List<string> ids = checkIds.Split(',').ToList();

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadLayerData("0", dt, ids, checkbox) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        public string getLayerData()
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select id, pid as parentId, name as title, '0' as checkArr from public.sys_layer order by id";
                //DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);                
                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + dbHelper.GetAsJson(CommandType.Text, sql) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        public string getlognumb()
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select id, pid as parentId, name as title, '0' as checkArr from public.sys_layer order by id";
                //DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);                
                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + dbHelper.GetAsJson(CommandType.Text, sql) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }


        public User getLoginUser(string username, string password)
        {
            string sql = "select * from public.sys_user where username = '" + username + "' and password = '" + password + "'";
            string sqlrole = "select array_to_string(array_agg(role_flag),',') rolename from (select* from sys_role where id in	" +
                "(select to_number(unnest(string_to_array(role_id, ',')),'99G999D9S')from sys_user " +
                "where username = '" + username + "' and password = '" + password + "')) rolename";
            User model = null;
            //try
            //{
            DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
            DataTable dtrole = dbHelper.GetDataTable(CommandType.Text, sqlrole);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                model = new User();
                model.id = Int32.Parse(dt.Rows[0]["id"].ToString());
                model.username = dt.Rows[0]["username"].ToString();
                model.department = dt.Rows[0]["department"].ToString();
                model.telephone = dt.Rows[0]["telephone"].ToString();
                model.password = dt.Rows[0]["password"].ToString();
                model.role_id = dt.Rows[0]["role_id"].ToString();
                model.name = dt.Rows[0]["name"].ToString();
            }
            if (dtrole != null && dtrole.Rows != null && dtrole.Rows.Count > 0)
            {
                if (!string.IsNullOrEmpty(dtrole.Rows[0]["rolename"].ToString()))
                {
                    model.rolecode = dtrole.Rows[0]["rolename"].ToString();
                }
                
            } 
            //}
            //catch (Exception e)
            //{
            //}
            return model;
        }

        public string getModulesByUid(int uid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = "select m.*,(select \"dataName\" from public.sys_layer l where m.layer_id=id limit 1) as layer, \"iClass\" as icon from public.sys_module m, public.sys_user u, public.sys_role_module rm where u.id=" + uid + " and rm.role_id=u.role_id and m.id=rm.module_id order by index, id";

                result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();

            }
            catch (Exception e)
            {
                result = "\"error\"";
            }
            return result;
        }

        public string getLayersByUid(int uid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = $"select n.* from (select tb.layer_id from (select unnest(string_to_array(role_id, ','))::int as roleid from sys_user where id = {uid}) as ta,sys_role_layer as tb where ta.roleid = tb.role_id group by tb.layer_id) as m,sys_layer as n  where m.layer_id = n.id order by ordernumb";
                
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);

                result = "{\"status\":{\"code\":200,\"message\":\"操作成功\"},\"data\":[" + loadLayerData("0", dt) + "]}";

            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        private string loadModuleData(string pid, DataTable dt, List<string> ids = null)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.RowFilter = "pid=" + pid;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    string id = dv[i]["id"].ToString();
                    string _pid = dv[i]["pid"].ToString();
                    if (_pid == pid)
                    {
                        string children = loadModuleData(id, dt, ids);
                        var isChecked = "0";
                        if (ids != null && ids.IndexOf(id) != -1)
                        {
                            isChecked = "1";
                        }
                        result += "{";
                        result += "\"id\":\"" + id + "\",";
                        result += "\"title\":\"" + dv[i]["name"] + "\",";
                        result += "\"checkArr\":[{\"type\": \"0\", \"isChecked\": \"" + isChecked + "\"}],";
                        result += "\"parentId\":\"" + pid + "\"";
                        if (children != "")
                        {
                            result += ",\"isLast\":false";
                            result += ",\"children\":[" + children + "]";
                        }
                        {
                            DataView dvlast = new DataView();
                            dvlast.Table = dt;
                            dvlast.RowFilter = "pid=" + id;
                            if (dvlast.Count == 0 || object.Equals(dvlast, null))
                            {
                                result += ",\"basicData\":{\"url\":\"" + dv[i]["url"] + "\",\"layer_id\":\"" + dv[i]["layer_id"] + "\",\"type\":\"" + dv[i]["type"] + "\"}";
                                result += ",\"isLast\":true";
                            }
                        }
                        result += "}";
                        if (i != dv.Count - 1)
                        {
                            result += ",";
                        }
                    }
                }
            }
            return result;
        }

        private string loadLayerData(string pid, DataTable dt, List<string> ids = null, string checkbox = null)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.RowFilter = "pid=" + pid;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    string id = dv[i]["id"].ToString();
                    string _pid = dv[i]["pid"].ToString();
                    if (_pid == pid)
                    {
                        string children = loadLayerData(id, dt, ids, checkbox);
                        var isChecked = "0";
                        if (ids != null && ids.IndexOf(id) != -1)
                        {
                            isChecked = "1";
                        }

                        result += "{";
                        result += "\"id\":\"" + id + "\",";
                        result += "\"title\":\"" + dv[i]["name"] + "\",";
                        if (string.IsNullOrEmpty(checkbox))
                        {
                            result += "\"checkArr\":[{\"type\": \"0\", \"isChecked\": \"" + isChecked + "\"}],";
                        }
                        result += "\"parentId\":\"" + pid + "\"";
                        if (children != "")
                        {
                            result += ",\"isLast\":false";
                            result += ",\"children\":[" + children + "]";
                        }
                        {
                            DataView dvlast = new DataView();
                            dvlast.Table = dt;
                            dvlast.RowFilter = "pid=" + id;
                            if (dvlast.Count == 0 || object.Equals(dvlast, null))
                            {
                                result += ",\"basicData\":{\"name\":\"" + dv[i]["name"] + "\",\"dataName\":\"" + dv[i]["dataName"] + "\",\"dataType\":\"" + dv[i]["dataType"] + "\",\"pointQuery\":\"" + dv[i]["pointQuery"] + "\",\"shpColor\":\"" + dv[i]["shpColor"] + "\",\"defaultOpen\":\"" + dv[i]["defaultOpen"] + "\"}";
                                result += ",\"isLast\":true";
                            }
                        }
                        result += "}";
                        if (i != dv.Count - 1)
                        {
                            result += ",";
                        }
                    }
                }
            }
            return result;
        }

        public string queryData(string url)
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

        public string queryDataPost(string urlstr)
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

        public string getBySql(string sql)
        {
            string result = "";
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, sql);
            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }

        public string getBySqlXZ(string sql)//做选址时候改写
        {
            string result = "";
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, sql);
            }
            catch (Exception e)
            {
                result = "error";
            }
            return result;
        }
        public string getLayerByName(string name)
        {
            string result = "";
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, "select * from public.sys_layer where \"dataName\" = '" + name + "'");
                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"获取失败\"}";
            }
            return result;
        }


        public string getlayerselect()
        {
            string result = "";
            try
            {
                result = dbHelper.GetAsJson(CommandType.Text, "SELECT * FROM \"sys_layer\" WHERE  pid='0'");
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"获取失败\"}";
            }
            return result;
        }

        public string getlayersthetable(string keyword, int limit, int page, string type)
        {
            string res = "";
            DataTable dt = new DataTable();
            string sql = "";
            string sql1 = "";
            int count = 0;
            string WHERE = "";
            if (type != "全部" && type != "")
            {
                WHERE = "and ( ta.id='" + type + "'  or   ta.pid='" + type + "')";
            }
            sql = $"select ta.*, tb.name as pName from public.sys_layer as ta left join public.sys_layer as tb on ta.pid = tb.id  where  ( ta.name LIKE '%{keyword}%'  )  {WHERE} ORDER BY ta.id DESC   limit {limit} offset {(page - 1) * limit}";
            sql1 = $"SELECT  count(*)  from public.sys_layer as ta left join public.sys_layer as tb on ta.pid = tb.id  where  ( ta.name LIKE '%{keyword}%'  ) {WHERE}";
            try
            {
                dt = dbHelper.GetDataTable(CommandType.Text, sql);
                count = dbHelper.GetCount(sql1);
                res = "{\"code\":0,\"msg\":\"success\",\"count\":" + count + ",\"data\":" + JsonConvert.SerializeObject(dt) + "}";
            }
            catch (Exception e)
            {
                res = "{\"code\":1,\"msg\":\"error\",\"data\":" + e.Message.ToString() + "}";
            }
            return res;
        }

        public string saveBiaozhu(Biaozhu model)
        {
            string sql = "insert into[tool_biaozhu]([name],[dataType],[bzType],[describe],[length],[area],[coordinate],[filePath],[dateTime],[sessionid],[guid],[height]) values(@name, @datatype, @bztype, @describe, @length, @area, @coordinate, @filePath,@dateTime,@sessionid,@guid,@height) RETURNING id;";
            if (model.id != 0)
            {
                sql = "update [tool_biaozhu] set [name]=@name,[datatype]=@dataType,[bztype]=@bzType,[describe]=@describe,[length]=@length,[area]=@area,[coordinate]=@coordinate,[filePath]=@filePath,[dateTime]=@dateTime where id=@id";
            }

            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@datatype", model.dataType));
            parameter.Add(new Npgsql.NpgsqlParameter("@bztype", model.bzType));
            parameter.Add(new Npgsql.NpgsqlParameter("@describe", model.describe));
            parameter.Add(new Npgsql.NpgsqlParameter("@datetime", model.dataTime));
            parameter.Add(new Npgsql.NpgsqlParameter("@length", model.length));
            parameter.Add(new Npgsql.NpgsqlParameter("@area", model.area));
            parameter.Add(new Npgsql.NpgsqlParameter("@coordinate", model.coordinate));
            parameter.Add(new Npgsql.NpgsqlParameter("@filePath", model.filePath));
            parameter.Add(new Npgsql.NpgsqlParameter("@sessionid", model.sessionid));
            parameter.Add(new Npgsql.NpgsqlParameter("@guid", model.guid));
            parameter.Add(new Npgsql.NpgsqlParameter("@height", model.height));
            string result = "";
            if (model.id == 0)
            {
                result = dbHelper.GetOneValue(CommandType.Text, sql, parameter.ToArray());
            }
            else
            {
                dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray());
                result = model.id.ToString();
            }
            return result;
        }

        public string editBiaozhu(string height, string id)
        {
            string result = "";
            try
            {
                string sql = "UPDATE tool_biaozhu set height='"+height+"'  WHERE id='"+id+"'";

                int temp = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                if (temp > 0)
                {
                    result = "{\"result\":\"success\",\"data\":\"" + result + "\"}";
                }
                else
                {
                    result = "{}";
                }
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"删除失败\"}";
            }
            return result;
        }



        public string getBiaozhu(int sessid, string Guid, string id = null, string name = null, string bzType = null)
        {
            string result = "";
            string sql = "";
            try
            {
                if (Guid != "" && Guid != null)
                {
                    sql = "select * from public.tool_biaozhu where 1 = 1 and guid='" + Guid + "'";
                }
                else
                {
                    sql = "select * from public.tool_biaozhu where 1 = 1 and sessionid='" + sessid + "'";
                }
                if (!string.IsNullOrEmpty(id))
                {
                    sql += " and id = " + id;
                }
                if (!string.IsNullOrEmpty(name))
                {
                    sql += " and name like '%" + name + "%'";
                }
                if (!string.IsNullOrEmpty(bzType))
                {
                    sql += " and \"bzType\" = '" + bzType + "'";
                }
                result = dbHelper.GetAsJson(CommandType.Text, sql);
                result = "{\"result\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"获取失败\"}";
            }
            return result;
        }

        public string deleteBiaozhuById(string id)
        {
            string result = "";
            try
            {
                string sql = "delete from public.tool_biaozhu where id = " + id;
                
                int temp = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                if (temp > 0)
                {
                    result = "{\"result\":\"success\",\"data\":\"" + result + "\"}";
                }
                else
                {
                    result = "{}";
                }
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":\"删除失败\"}";
            }
            return result;
        }


        public string getFirstValue(string url)
        {
            string result = queryDataPost(url);
            string returnstr = "";
            var jo = (JObject)JsonConvert.DeserializeObject(result);
            if (jo["result"].ToString() == "success")
            {
                var joo = (JArray)jo["data"];
                var index = 0;
                foreach (JObject items in joo)
                {
                    foreach (var item in items)
                    {
                        returnstr = item.Value.ToString();
                        break;
                    }
                    break;
                }
            }
            return returnstr;
        }

public DataTable getDatatable(string tablename)
        {
            string sql = "";
            sql = $"select * from {tablename}";
            DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
            return dt;
        }
        public string setOrderById(string tableName, string id, int order)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = $"update {tableName} set [order] ={order} where id='{id}'";
                int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                if (count == 1)
                {
                    result = "success";
                }
                else
                {
                    result = "none";
                }
            }
            catch (Exception e)
            {
                result = "error";
            }
            return result;
        }

public string getNewDatatable(DataTable dt,string tablename)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    result += "{";
                    if (tablename == "sys_module_function") {
                        result += "\"basicData\":{\"realID\":\"" + dv[i]["id"] + "\"},\"id\":\"000" + dv[i]["id"] + "\",\"checkArr\":\"0\",\"title\":\"" + dv[i]["function_name"] + "\",\"parentId\":\"" + dv[i]["module_id"] + "\"";
                    }
                    else if (tablename == "sys_module") {
                        result += "\"id\":\"" + dv[i]["id"] + "\",\"checkArr\":\"0\",\"title\":\"" + dv[i]["name"] + "\",\"parentId\":\"" + dv[i]["pid"] + "\"";
                    }
                    result += "}";
                    if (i != dv.Count - 1)
                    {
                        result += ",";
                    }
                }
            }
            return result;
        }

        public string getMenuByUid(int uid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = $"select n.* from (select tb.module_id from (select unnest(string_to_array(role_id, ','))::int as roleid from sys_user where id = {uid}) as ta,sys_role_module as tb where ta.roleid = tb.role_id group by tb.module_id) as m,sys_module as n  where m.module_id = n.id";

                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                DtreeJson tree = new DtreeJson();
                result = tree.createTree(dt);
            }
            catch (Exception e)
            {
                result = "\"error\"";
            }
            return result;
        }
        public string getFunctionByUid(int uid)
        {
            string result = "";
            string sql = "";
            try
            {
                sql = $"select n.* from (select tb.module_function_id from (select unnest(string_to_array(role_id, ','))::int as roleid from sys_user where id = {uid}) as ta,sys_role_module_function as tb where ta.roleid = tb.role_id group by tb.module_function_id) as m,sys_module_function as n  where m.module_function_id = n.id";
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                result = JsonConvert.SerializeObject(dt);
            }
            catch (Exception e)
            {
                result = "\"error\"";
            }
            return result;
        }
		
        public string loadAssignedData(string pid, DataTable dt, List<string> ids = null, string checkbox = null)
        {
            DataView dv = new DataView();
            dv.Table = dt;
            dv.RowFilter = "pid=" + pid;
            if (dv.Count == 0 || object.Equals(dv, null))
            {
                return "";
            }
            string result = "";
            {
                for (int i = 0; i < dv.Count; i++)
                {
                    string id = dv[i]["id"].ToString();
                    string _pid = dv[i]["pid"].ToString();
                    if (_pid == pid)
                    {
                        string children = loadAssignedData(id, dt, ids, checkbox);
                        var isChecked = "0";
                        if (ids != null && ids.IndexOf(id) != -1)
                        {
                            isChecked = "1";
                        }
                        result += "{";
                        result += "\"id\":\"" + id + "\",";
                        result += "\"title\":\"" + dv[i]["name"] + "\",";
                        if (string.IsNullOrEmpty(checkbox))
                        {
                            result += "\"checkArr\":[{\"type\": \"0\", \"isChecked\": \"" + isChecked + "\"}],";
                        }
                        result += "\"parentId\":\"" + pid + "\"";
                        if (children != "")
                        {
                            result += ",\"isLast\":false";
                            result += ",\"children\":[" + children + "]";
                        }
                        {
                            DataView dvlast = new DataView();
                            dvlast.Table = dt;
                            dvlast.RowFilter = "pid=" + id;
                            if (dvlast.Count == 0 || object.Equals(dvlast, null))
                            {
                                result += ",\"basicData\":{\"name\":\"" + dv[i]["name"] + "\",\"defaultOpen\":\"" + dv[i]["defaultOpen"] + "\"}";
                                result += ",\"isLast\":true";
                            }
                        }
                        result += "}";
                        if (i != dv.Count - 1)
                        {
                            result += ",";
                        }
                    }
                }
            }
            return result;
        }
        public DataSet datatable(string sql)
        {
            DataSet dt = dbHelper.ExecuteQuery(CommandType.Text, sql);
            return dt;
        }
        public int GetCount(string sql)
        {
            int dt = dbHelper.GetCount(sql);
            return dt;
        }
        //修改密码
        public string updatepwd(int id, string pwd)
        {
            string sql = $"update sys_user set  password='{pwd}'  where id='{id}' ";
            int dt = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            string returnstr = dt == 1 ? "success" : "error";
            return returnstr;
        }
        //用户基本信息
        public string getUserInfo(int id)
        {
            string sql = $"SELECT * from  sys_user  where  id={id}  ";
            string model = "";
            try
            {
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {

                    model = "{\"code\":0,\"msg\":\"\",\"data\":" + JsonConvert.SerializeObject(dt) + "}";

                }
            }
            catch (Exception e)
            {
            }
            return model;
        }


        public string updateSql(string sql)
        {
            string result = "";
            try
            {
                result = dbHelper.ExecuteNonQuery(CommandType.Text, sql).ToString();
            }
            catch (Exception e)
            {
                result = "{\"status\":{\"code\":100,\"message\":\"失败\"}}";
            }
            return result;
        }
        public DbDataReader GetReader(string sql)
        {
            DbDataReader result = null;
            try
            {
                result = dbHelper.ExecuteReader(CommandType.Text, sql);
            }
            catch (Exception e)
            {
                result = null;
            }
            return result;
        }
		 #region 登录日志
        /// <summary>
        /// 保存登录日志
        /// </summary>
        /// <returns></returns>
        public int SaveLoginlog(int userid, string ip ,string area ,string system="" )
        {
            int count = -1;
            string sql = $" insert into loginlog(userid,ip,area,system,logintime)values" +
                $"('{userid}','{ip}','{area}','{system}','{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}')";
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        public string GetLog(int page ,int limit,string keyword="")
        {
            string  sql =$"select a.*,b.username from loginlog a ,sys_user b where a.userid=b.id ";
            string where = "";
            if (keyword!="")
            {
                where = $" and  POSITION( '{keyword}'in b.username ) >0";
            }
            sql += $" {where} order by logintime  desc  limit {limit} offset {(page - 1) * limit}";

            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        public int GetLogCount(int page, int limit, string keyword = "")
        {
            string sql = $"select count(*) from loginlog a ,sys_user b where a.userid=b.id ";
            string where = "";
            if (keyword != "")
            {
                where = $" and  POSITION( '{keyword}'in b.username ) >0";
            }
            sql += $" {where} ";

            int count = dbHelper.GetCount(sql);
            return count;
        }
        /// <summary>
        /// 删除登录日志
        /// </summary>
        /// <param name="id"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public int DeletebyId(string id = "", string table = "")
        {
            string sql = $" delete from {table} where id='{id}'";
            int count = -1;
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        #endregion
        #region 首页日志统计
        /// <summary>
        /// 获取首页登录日志的数据
        /// </summary>
        /// <returns></returns>
        public List<int> GetMainNum()
        {
            List<int> num =new List<int>();
            string sql = "";
            int count = 0;
            ///人员
            sql = $"select count (*) from sys_user";
            count = dbHelper.GetCount(sql);
            num.Add(count);
            ///角色
            sql = $"select count(*) from sys_role";
            count = dbHelper.GetCount(sql);
            num.Add(count);
            //模块
            sql = $"select count(*) from sys_module";
            count = dbHelper.GetCount(sql);
            num.Add(count);
            ///登录日志
            sql = $"select count(*) from loginlog";
            count = dbHelper.GetCount(sql);
            num.Add(count);
            return num;
        }

 /// <summary>
        /// date为当前日期的前七天的日期
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public string GetMainStatice(string date = "")
        {
            string sql = "";
            string where = "";
            if (!string.IsNullOrEmpty(date))
            {
                where += $" and logintime > '{date}'";
            }
            sql = $"select 年,月,天,日期,count(*) from (select logintime, userid, to_char(to_timestamp(logintime, 'YYYY-MM-DD hh24:mi:ss'), 'YYYY') 年," +
                $" to_char(to_date(logintime, 'YYYY-MM-DD hh24:mi:ss'), 'mm') 月 ,to_char(to_date(logintime, 'YYYY-MM-DD hh24:mi:ss'), 'dd') 天," +
                $"to_char(to_date(logintime,'YYYY-MM-DD hh24:mi:ss'),'YYYY-MM-DD') 日期" +
                $" from loginlog where 1=1 {where}  order by 年 asc, 月 asc,天 asc,日期 asc, userid asc ) a group by 年, 月, 天,日期";
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        #endregion


        public string queryLayerData( string tableName, string wkt = "",string clip = "", string where = "")
        {
            string str = "";

            str = "SELECT * FROM wdq_kgt WHERE 1=1   AND ST_Within (geom,ST_GeomFromText('POLYGON((113.800562 34.060393, 113.790818 34.02226, 113.824983 34.020623, 113.834791 34.046161, 113.800562 34.060393))'))";
         
         

                DBHelper dd = new DBHelper();
                DataTable dt = dd.GetDataSet(str).Tables[0];

                string aa = ProTools.DataTable2Json(dt);


                    return ("{\"Rows\":" + ProTools.DataTable2Json(dt) + "}");
               
            
        
        }

        #region 出让价格

        public string GetSellData(string table, string tableNames, string wkt = "")
        {
            string sql = "";
            string[] jointable = tableNames.Split(',');
            if (jointable.Length > 0)
            {
                foreach (var item in jointable)
                {
                    if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(table))
                    {
                        var ails = item.Substring(item.Length - 4);
                        sql += $"union all select * ,round(cast(ST_area(d.geom::geography)as numeric),4) area from {table} d,{item} {ails} " +
                            $"where d.lx={ails}.用地类型 and ST_Intersects(d.geom,ST_GeomFromText('{wkt}',4326)::geography) and " +
                            $"ST_Intersects({ails}.geom,ST_GeomFromText('{wkt}',4326)::geography) and ST_Intersects(d.geom::geography,{ails}.geom::geography)";
                    }
                }
            }
            //sql = $"select dlmc,用地类型 ydlx , 等级 jb,avg(cast(上限 as numeric)) sx,avg(cast(平均值 as numeric)) pjz,avg(cast(下限 as numeric)) xx" +
            //    $",sum(area) area ,count(*) count from ({sql.Substring(9)}) a group by 用地类型 ,等级,dlmc";
            sql = $"select 用地类型 ydlx , 等级 jb,avg(to_number(上限,'99999999D9999')) sx,avg(to_number(平均值,'99999999D9999')) pjz,avg(to_number(下限,'99999999D9999')) xx" +
                $",avg(to_number(基准地价亩,'99999999D9999')) 基准地价,sum(area) area ,count(*) count from ({sql.Substring(9)}) a group by 用地类型 ,等级";
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }

        public string GetSellStatisc(string table ,string FieldName,string  wkt="")
        {
            string sql = "";
            sql = $"select {FieldName}, count(*) amount, round(cast(sum(St_Area(geom::geography)) as numeric),4) calarea,round(cast(sum(St_Area(geom::geography)) as numeric)/666.7,4) mu from  {table} where  ST_Intersects(geom,ST_GeomFromText('{wkt}',4326)::geography)  group by {FieldName}";
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        public string getSellDetail(string table, string tableNames, string wkt = "",string where = "")
        {
            string sql = "",sql_col="", fieldname="";
            string[] jointable = tableNames.Split(',');
            if (jointable.Length > 0)
            {
                foreach (var item in jointable)
                {
                    if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(table))
                    {
                        var ails = item.Substring(item.Length - 4);
                        sql += $"union all select d.*,round(cast(st_area(d.geom::geography)as numeric),2) calarea ,round(cast(ST_area(d.geom::geography)as numeric),4) area from {table} d,{item} {ails} " +
                            $"where d.lx={ails}.用地类型 and ST_Intersects(d.geom,ST_GeomFromText('{wkt}',4326)::geography) and " +
                            $"ST_Intersects({ails}.geom,ST_GeomFromText('{wkt}',4326)::geography) and ST_Intersects(d.geom::geography,{ails}.geom::geography)  and  {where}";
                    }
                }
            }
            sql_col = $"select string_agg(column_name,',')   from information_schema.columns where table_schema = 'public' and table_name = '{table}'";
            DataTable dataTable = dbHelper.GetDataTable(CommandType.Text, sql_col);
            if (dataTable != null)
            {
                fieldname = dataTable.Rows[0][0].ToString();
                fieldname = fieldname.Replace("geom", "ST_ASGeoJson(geom) as geom");
            }
            sql = $"select {fieldname},calarea from ({sql.Substring(9)}) a  where 1=1 ";

            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;

        }

        #endregion
        #region 升值空间
        public string GetRevalueArea(string wkt, string buffer)
        {
            string sql = "";
            sql = $"select st_area(ST_Buffer(ST_GeomFromText('{wkt}',4326)::geography,{buffer})) buffermj," +
                $"St_area(ST_SymDifference(ST_GeomFromText(ST_asText(ST_Buffer(ST_GeomFromText('{wkt}',4326)::geography,{buffer})),4326)::geometry ,ST_GeomFromText('{wkt}', 4326)::geometry)::geography)bufferarea," +
                $"st_AsGeoJson(ST_SymDifference(ST_GeomFromText(ST_asText(ST_Buffer(ST_GeomFromText('{wkt}',4326)::geography,{buffer})),4326)::geometry,ST_GeomFromText('{wkt}', 4326)::geometry)) geom";
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        #endregion
        

        #region wkt获取圈中的要素面积
        public string wktGetArea(string wkt, string tableName)
        {
            string result = "";
            try
            {
                //获取该表中的除了geom的列名

                string getColNames_sql = "SELECT A.attname AS colname FROM pg_class AS C,pg_attribute AS A WHERE C.relname = '" + tableName + "' AND A.attrelid = C.oid AND A.attnum > 0 and a.attname != 'geom'";
                string colNames = this.getBySql(getColNames_sql).Replace("[{", "").Replace("\"}]", "").Replace("\"colname\":", "").Replace("\"},{\"", ",").Replace("\"", "");


                //获取wkt区相交的要素的表格
                //string table = this.getBySql("select " + colNames + ",ST_Area(st_transform(ST_Intersection(geom,st_geomfromtext ('" + wkt + "', 4326)),4527)) as 面积 from public." + tableName + " where ST_Intersects(geom,st_geomfromtext('" + wkt + "', 4326))");
                //table = JsonConvert.SerializeObject(table);
                string within = this.getBySql("SELECT SUM(ST_Area(st_transform(ST_Intersection(geom,st_geomfromtext ('" + wkt + "', 4326)),4527))) as area,count(jbntmj) as 内部包含 FROM public." + tableName + " where ST_Within(geom,st_geomfromtext ('" + wkt + "', 4326));");


                //within = JsonConvert.SerializeObject(within);
                string polyline = "LINESTRING" + wkt.Substring(wkt.IndexOf('(')).Replace("((", "(").Replace("))", ")");
                string instersects_sql = "select sum(ST_Area(st_transform(ST_Intersection(geom,st_geomfromtext ('" + wkt + "', 4326)),4527))) as area,count(ST_Intersection(geom,st_geomfromtext ('" + wkt + "', 4326))) as 边缘相交 from public." + tableName + " where ST_Intersects(geom,st_geomfromtext('" + polyline + "', 4326))";
                string insert = this.getBySql(instersects_sql);
                if (insert.IndexOf("失败") == -1)
                {
                    insert = insert.Substring(1, insert.Length - 2); //第一个参数为开始的位置，第二个参数为子字符串的长度
                }
                else
                {
                    insert = "\"查询表中没有设置空间坐标系\"";
                }
                if (within.IndexOf("失败") == -1)
                {
                    within = within.Substring(1, within.Length - 2);
                }
                else
                {
                    within = "\"查询的标中没有空间坐标系\"";
                }
                //insert = JsonConvert.SerializeObje(insert);


                //result += "{\"result\":\"success\",\"data\":{\"table\":" + table + ",\"within\":" + within + ",\"insert\":" + insert + "}}";
                result += "{\"result\":\"success\",\"data\":{\"within\":" + within + ",\"insert\":" + insert + "}}";
            }
            catch (Exception e)
            {
                result = "{\"result\":\"error\",\"message\":" + e.Message + "}";
            }
            return result;
        }


        #endregion
        
        public string pglGetStatistics(string wkt)
        {
            string result = "";
            string a= "SELECT name, COUNT(1) as amount, sum(round(cast(clipArea as numeric),2)) as clipArea, sum(round(cast(calArea as numeric),2)) as calArea  FROM ( SELECT xzqmc as name, ST_Area(geom::geography) as calArea,ST_Area(ST_Intersection(ST_GeomFromText('" + wkt + "',4326)::geography,ST_Buffer(geom,0.00000001)::geography)) as clipArea FROM ybgd where 1 = 1 and ST_Intersects(geom,ST_GeomFromText('" + wkt+"',4326)::geography)) a group by name";

            result=this.getBySql(a);

            return result;
        }

        public string getHouseArea(string tableName,string groupByField,string wkt)
        {
            string result = "";
            string sql= "select 房屋结构,sum(面积*房屋层数::numeric) from " + tableName + " where ST_Within(geom,st_geomfromtext ('" + wkt + "', 4326)) group by " + groupByField;
            try
            {
                result = this.getBySql(sql);
                result = "{\"status\":{\"code\":200,\"message\":\"success\"},\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"status\":{\"message\":\"error\"},\"data\":\"" + e.Message + "\"}";
            }
            return result;
        }

        public string Fsw(string wkt)
        {
            string sql= "select 房屋类型,类型,sum(round(st_area(Geography(geom))::decimal,2)) from public.xc_house where ST_Within(geom,st_geomfromtext ('" + wkt + "', 4326)) group by 房屋类型,类型";
            string result = "";
            try
            {
                result = this.dbHelper.GetAsJson(CommandType.Text, sql);
                result = "{\"message\":\"success\",\"data\":" + result + "}";
            }
            catch (Exception e)
            {
                result = "{\"message\":\"error\",\"data\":\""+e.Message+"\"}";

            }
            return result;
        }
}
