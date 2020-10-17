
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using static Aipuer.Common.GCGLModel;

namespace Aipuer.Common
{
    public class GCGLCommon
    {
        private IDBHelper dbHelper = null;
        public GCGLCommon() { }
        public GCGLCommon(string connectionString)
        {
            dbHelper = new PostgreHelper(connectionString);
        }
        #region 项目工作事件
        /// <summary>
        /// 添加 项目 工作 事件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string savePwe(PWE model ,Guid id)
        {
            string sql = "";
            string result = "";
            string where = "";
            
            //计算排序数列
            if (model.state != 0)
            {
                if (model.pid != Guid.Empty)
                {
                    ///编辑
                    if (model.id != Guid.Empty)
                    {
                        PWE pro = getpwebyid(model.id);
                        ///判断编辑 所属pid改变没有 改变排序改变 没有排序不变
                        if (model.pid != pro.pid)
                        {
                            where = " and pid='" + model.pid + "'";
                            int count = getCount("gcgl_pwe", where);
                            model.serialnumber = count + 1;
                        }
                        else
                        {
                            model.serialnumber = pro.serialnumber;
                        }
                    }
                    else//新增  
                    {
                        ///插入添加
                        if (!string.IsNullOrEmpty(model.nextid))
                        {
                            sql = $" select * from gcgl_pwe  where pid='{model.pid}'and id='{model.nextid}'";
                            DataTable _number = dbHelper.GetDataTable(CommandType.Text, sql);
                            if (_number != null && _number.Rows.Count > 0)
                            {
                                model.serialnumber = Int32.Parse(_number.Rows[0]["serialnumber"].ToString());
                            }
                            sql = $" update gcgl_pwe set serialnumber=serialnumber+1 where pid='{model.pid}' " +
                                $"and serialnumber >=(select serialnumber from gcgl_pwe where pid='{model.pid}' and id='{model.nextid}') ";
                            dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                        }
                        else
                        {
                            where = " and pid='" + model.pid + "'";
                            int count = getCount("gcgl_pwe", where);
                            model.serialnumber = count + 1;
                        }
                        
                    }
                }
                
            }
            if (model.id == Guid.Empty)
            {
                model.id = id;
                sql = $" insert into gcgl_pwe (id,pid,adduserid,leaderid,joinperson,name,content,location,address,lable," +
                    $"progress,starttime,endtime,addtime,updatetime,upload,serialnumber,state)" +
                    $"values(@id,@pid,@adduserid,@leaderid,@joinperson,@name,@content,@location,@address,@lable,@progress," +
                    $"@starttime,@endtime,@addtime,@updatetime,@upload,@serialnumber,@state)";
                
                if (!string.IsNullOrEmpty(model.tempname))
                {
                    var itemname = model.tempname.Split(',');
                    var itemleader = model.templeader.Split(',');
                    if (itemname.Length>0)
                    {
                        for (int i = 0; i < itemname.Length; i++)
                        {
                            Guid wid = Guid.NewGuid();
                            PWE itempwe = new PWE();
                            itempwe.name = itemname[i].Trim();
                            itempwe.leaderid = Convert.ToInt32(itemleader[i].Trim());
                            itempwe.adduserid = model.adduserid;
                            itempwe.pid = model.id;
                            itempwe.state=(model.state==0?1:2);
                            itempwe.starttime = model.starttime;
                            itempwe.endtime = model.endtime;
                            savePwe(itempwe,wid);
                        }
                    }
                }
            }
            else
            {
                sql = $" update gcgl_pwe set pid='{model.pid}', leaderid = '{model.leaderid}',joinperson='{model.joinperson}',name = '{model.name}',content = '{model.content}'" +
                    $",location = '{model.location}',address ='{model.address}',lable ='{model.lable}',progress = '{model.progress}'" +
                  $",starttime='{model.starttime}',endtime='{model.endtime}',updatetime='{model.updatetime}',upload='{model.upload}',serialnumber='{model.serialnumber}'" +
                  $" where id='{model.id}' and state='{model.state}'";
            }
            
            List<DbParameter> parameter = new List<DbParameter>();
            parameter.Add(new Npgsql.NpgsqlParameter("@id", model.id));
            parameter.Add(new Npgsql.NpgsqlParameter("@pid", model.pid));
            parameter.Add(new Npgsql.NpgsqlParameter("@adduserid", model.adduserid));
            parameter.Add(new Npgsql.NpgsqlParameter("@leaderid", model.leaderid));
            parameter.Add(new Npgsql.NpgsqlParameter("@joinperson", model.joinperson));
            parameter.Add(new Npgsql.NpgsqlParameter("@name", model.name));
            parameter.Add(new Npgsql.NpgsqlParameter("@content", model.content));
            parameter.Add(new Npgsql.NpgsqlParameter("@location", model.location));
            parameter.Add(new Npgsql.NpgsqlParameter("@address", model.address));
            parameter.Add(new Npgsql.NpgsqlParameter("@lable", model.lable.Trim()));
            parameter.Add(new Npgsql.NpgsqlParameter("@progress", model.progress));
            parameter.Add(new Npgsql.NpgsqlParameter("@starttime", model.starttime));
            parameter.Add(new Npgsql.NpgsqlParameter("@endtime", model.endtime));
            parameter.Add(new Npgsql.NpgsqlParameter("@addtime", model.addtime));
            parameter.Add(new Npgsql.NpgsqlParameter("@updatetime", model.updatetime));
            parameter.Add(new Npgsql.NpgsqlParameter("@upload", model.upload));
            parameter.Add(new Npgsql.NpgsqlParameter("@serialnumber", model.serialnumber));
            parameter.Add(new Npgsql.NpgsqlParameter("@state", model.state));
            try
            {///添加额外字段参数
                where = $" delete from gcgl_projectotherfield where pid='{model.id}'";
                dbHelper.ExecuteNonQuery(CommandType.Text, where);
                if (!string.IsNullOrEmpty(model.extraname))
                {
                    where = $" insert into gcgl_projectotherfield (id,pid,name,content,flag)values";
                    var othername = model.extraname.Split(',');
                    var othercontent = model.extracontent.Split(',');
                    if (othername.Length > 0)
                    {
                        for (int m = 0; m < othername.Length; m++)
                        {
                            var otherid = Guid.NewGuid();
                            where += $" ( '{otherid}','{model.id}','{othername[m]}','{othercontent[m]}','{model.state}'),";
                        }
                    }
                    where = where.Substring(0, where.Length - 1);
                   result =dbHelper.ExecuteNonQuery(CommandType.Text, where).ToString();
                }

                result = dbHelper.ExecuteNonQuery(CommandType.Text, sql, parameter.ToArray()).ToString();
                ////部门与项目挂钩
                if (!string.IsNullOrEmpty(model.leaderid.ToString()) || !string.IsNullOrEmpty(model.joinperson))
                {
                    string person = model.leaderid.ToString() + ',' + model.joinperson;
                    if (!string.IsNullOrEmpty(person))
                    {
                        string[] userjoin = person.Split(',');
                        if (userjoin!=null&&userjoin.Length>0)
                        {
                            foreach (var join in userjoin)
                            {
                                if (!string.IsNullOrEmpty(join))
                                {
                                    User user = getUser(join);
                                    if (user != null&&!string.IsNullOrEmpty(user.department))
                                    {
                                        int num = (model.id == Guid.Empty ? 0 : 1);
                                        int orgcount = -1; ;
                                        if (num==1)
                                        {//编辑前先清除
                                            orgcount = SaveOrgPwe(model.pid, model.id, user.department, model.state, 1);
                                        }
                                        orgcount = SaveOrgPwe(model.pid, model.id, user.department, model.state, 0);
                                    }
                                    
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }
        /// <summary>
        /// 得到表的所有数据
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        
        #region 前台项目、工作、事件列表查询
        /// <summary>
        /// 得到流加载数据
        /// </summary>
        /// <param name="table"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="state"></param>
        /// <param name="pweflag"></param>
        /// <returns></returns>
        public List<PWE> getPageData( string table,string page,string limit,string state,string pweflag,string keyword="",string lable="",int[] uid=null,string role="",string org="")
        {
            int _page = Convert.ToInt32(page);
            int _limit = Convert.ToInt32(limit);
            string where = wheresql(state,pweflag,keyword,lable,uid,role,org);
            
            string sql = $" select * from {table} where state ='{state}'and isdeleted='0' {where} order by addtime desc  limit {_limit} offset {(_page - 1)*_limit}";
            var result =dbHelper.GetDataTable(CommandType.Text, sql);
            List<PWE> list= DataTablePweToList(result);
            return list;
        }
        /// <summary>
        /// 整理sql语句
        /// state 0 项目 1 工作 2 事件
        ///pweflag （0 全部项目 1 进行中项目 2 负责的项目 3 参与的项目 4 已完结项目) 
        ///pweflag （0 全部工作 1 单独工作   2 负责工作   3 参与的工作)
        ///pweflag （0 全部事件 1 单独事件   2 负责事件   3 参与的事件)
        ///项目          工作       事件
        ///0 1 2 3 4     0 1 2 3    0 1 2 3    总、副总经理 (0包含所有的)
        ///0 1 2 3 4     0 2 3      0 2 3      部门经理 (0包含部门的)
        ///0 1 2         2 3        2 3        员工 (0包含个人)
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pweflag"></param>
        /// <param name="keyword"></param>
        /// <param name="lable"></param>
        /// <returns></returns>
        private string wheresql(string state="",string pweflag="", string keyword="",string lable="",int[] Uid=null,string role="",string org="")
        {
            string where = "";
            UserConfig con = new UserConfig();
            if (!string.IsNullOrEmpty(keyword))
            {
                where += $" and( POSITION('{keyword}'in name)>0)";
            }
            if (!string.IsNullOrEmpty(lable))
            {
                var lab = lable.Split(' ');
                where += $" and(";
                for (int k = 0; k < lab.Length; k++)
                {
                    where += $" POSITION('{lab[k]}' in lable)>0 and";
                }
                where = where.Substring(0, where.Length - 3);
                where += " )";
            }
            if (!string.IsNullOrEmpty(org))
            {
                where += $" and (id in(select cid from gcgl_org_pwe where org='{org}' and count >0 )) ";
            }
            string IssingleF = $"and pid!='{Guid.Empty}'";//单独
            string IsFlagF = "and progress!='100'";///项目未完成
            string IsFlagT = "and progress='100'";///项目已完成
            string IsJoin = "";
            string isLeader = "";
            ////拼装负责人、参与人的id
            if (pweflag!="2"||pweflag!="3")
            {
                if (Uid != null && Uid.Length > 0)//参与
                {
                    
                    isLeader += $"(";
                    IsJoin += $"(";
                    for (int i = 0; i < Uid.Length; i++)
                    {
                        isLeader += $" leaderid='{Uid[i]}' or";
                        IsJoin += $" POSITION(',{Uid[i]},'in joinperson)>0 or";
                    }
                    IsJoin = IsJoin.Substring(0, IsJoin.Length - 2);
                    IsJoin += ")";
                    isLeader = isLeader.Substring(0, isLeader.Length - 2);
                    isLeader += ")";
                }
            }
            switch (pweflag)
            {
                case "0"://全部项目、全部工作、全部事件
                    {
                        ///局长、副局长
                        if (role == con.General_Manager || role == con.Vice_General_Manager)
                        {

                        }
                        else if (role == con.Division_Manager||role==con.Staff)//科长 或科员（科员只有全部项目）
                        {
                            where += $"and(";
                            where += $" {IsJoin} ";
                            where += $" or ({isLeader})";
                            if (state != "2")
                            {
                                //参与事件的id
                                string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                                //参与工作的id
                                if (state == "1")//工作 项目
                                {
                                    where += $" or id in ({join})";
                                }
                                if (state == "0")
                                {
                                    where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF}and ({isLeader})or {IsJoin} or id in({join}) )";
                                }
                            }
                            where += ")";
                        }
                    }
                    break;
                case "1"://进行中项目 单独工作、单独事件
                    {
                        if (role != con.General_Manager && role != con.Vice_General_Manager)
                        {
                            if (state != "0")
                            {
                                where += $" and ( pid='{Guid.Empty}')";
                            }
                            else//进行中项目
                            {
                                where += IsFlagF;//进行中
                                where += $"and(";
                                where += $" {IsJoin} ";
                                //参与事件的id
                                string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                                where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF} and({isLeader} or{IsJoin} or id in({join})) )";
                                where += ")";
                            }
                        }
                        else
                        {
                            where += $" and pid='{Guid.Empty}'";
                        }
                    }
                    break;
                case "2"://负责项目 负责工作 负责事件 个人
                    {
                        if (Uid != null && Uid.Length > 0)
                        {
                            where += $" and ( leaderid='{Uid[0]}')";
                        } 
                    }
                    break;
                case "3"://参与项目 参与工作 参与事件 个人( 工作下事件 项目下工作事件)
                    {

                        if (Uid!=null&&Uid.Length>0)
                        {
                            ///事件 工作 项目
                            IsJoin= $" POSITION(',{Uid[0]},'in joinperson)>0";
                            where += $"and ({IsJoin})";
                            //参与事件的id
                            string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' { IssingleF} and({isLeader}or{IsJoin})";
                            if(state=="1")//工作 项目
                            {
                                where += $" or id in ({join})";
                            }
                            if (state =="0")
                            {
                                where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0' {IssingleF} and({isLeader}or{IsJoin}) or id in({join}) )";
                            }
                        }
                    }
                    break;
                case "4"://已完结项目 参与 负责的项目
                    {
                        where += IsFlagT;//
                        if (role != con.General_Manager || role != con.Vice_General_Manager)
                        {
                            where += $"and(";
                            where += $" {IsJoin} ";
                            //参与事件的id
                            string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                            where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF} and({isLeader} or{IsJoin} or id in({join})) )";
                            where += ")";
                        }
                    }
                    break;
                default:
                    break;
            }
            return where;
        } 

        /// <summary>
        /// 获取列表的标签
        /// 根据状态、登录人查询标签
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pweflag"></param>
        /// <returns></returns>
        public List<string> getLableData (string state,string uid)
        {
            List<string> lable = new List<string>();
            string sql = $" select lable from gcgl_pwe where state='{state}' and isdeleted='0'";
            DataTable dtable = dbHelper.GetDataTable(CommandType.Text, sql);
            lable = DataTableToLableList(dtable);
            return lable;
        }  
        
        /// <summary>
        /// 得到流加载的count值
        /// </summary>
        /// <param name="table"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="state"></param>
        /// <param name="pewfalg"></param>
        /// <returns></returns>
        public int getpweCount(string table,string page,string limit,string state, string pewfalg,string keyword ="",string Lable="",int[] Uid=null,string role="",string org="")
        {
            int _page = Convert.ToInt32(page);
            int _limit = Convert.ToInt32(limit);
            string where = wheresql(state,pewfalg,keyword,Lable,Uid,role,org);
            string sql = $" select count(*) from {table} where state ='{state}' and isdeleted='0' {where}";
            int result = dbHelper.GetCount(sql);
            return result;
        }
        #endregion
        /// <summary>
        /// 通过用户id获取用户信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User getUserbyId(string id)
        {
            string sql = $"select * ,(select array_to_string(array_agg(role_flag),',') rolename from (select* from sys_role where id in" +
                $"(select to_number(unnest(string_to_array(role_id, ',')), '99G999D9S')from sys_user where id = '{id}')) rolename)from sys_user where id = '{id}'";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<User> list = DataTableUserToList(table);
            return list.Count > 0 ? list[0] : null;
        }
        /// <summary>
        /// 暂时 查询所属项目 、所属工作
        /// </summary>
        /// <param name="selsate"></param>
        /// <param name="adduserid"></param>
        /// <param name="joinperson"></param>
        /// <returns></returns>
        public string getpweDatabystate(string selsate, string adduserid, string joinperson)
        {
            //string sql = $"select * from gcgl_pwe  where 1=1";
            string where = "";
            if (!string.IsNullOrEmpty(selsate))
            {
                where = $" and ( state ='{selsate}')";
            }
            string sql = $"select a.name as pname ,b.* from gcgl_pwe a,gcgl_pwe b where b.isdeleted='0' and  a.id=b.pid ";
            if (!string.IsNullOrEmpty(selsate))
            {
                sql += $" and b.state='{selsate}'";
            }
           // sql = sql + where;
            sql += $" union ";
            sql += $"select  name,*from gcgl_pwe where isdeleted='0' and pid = '{Guid.Empty}'";
            sql += where;
            sql += $" order by addtime desc";
            //union
            //select  name,*from gcgl_pwe where pid = '00000000-0000-0000-0000-000000000000' and state = '0' order by addtime desc
            
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        /// <summary>
        /// 通过ID获取信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PWE getpwebyid(Guid id,string state="")
        {
            List<PWE> pwe = null;
            string sql = $"select * from gcgl_pwe where id='{id}' and isdeleted='0'";
            DataTable var = dbHelper.GetDataTable(CommandType.Text, sql);
             pwe= DataTablePweToList(var);
            return pwe.Count > 0 ? pwe[0] : null;
        }
        /// <summary>
        /// 得到详情内容
        /// 将要查询的项目 项目下工作、工作下事件一次性查询
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<PWE> GetPweDescriptByid(string id,string state)
        {
            string sql = $" select * from gcgl_pwe where 1=1 and isdeleted='0' and id ='{id}'";
            string where = $" and id ='{id}'";
            string order = "order by pid ,serialnumber desc";
            //var result = dbHelper.GetDataTable(CommandType.Text, (sql+where));
            //List<PWE> list = DataTableToList(result);
            if (!string.IsNullOrEmpty(id))
            {
                if (state=="0")
                {
                    //where = $"select * from gcgl_pwe where  pid='{id}'";
                    //工作
                    //where = $" and pid='{id}'";
                    //result = dbHelper.GetDataTable(CommandType.Text, (sql + where + order));
                    //list[0].work = DataTableToList(result);
                    /////事件
                    //where = $" and pid in ( select id from gcgl_pwe where 1=1 and pid ='{id}') ";
                    //result = dbHelper.GetDataTable(CommandType.Text, (sql + where + order));
                    //list[0].eventlist = DataTableToList(result);
                     where = $" or (pid='{id}' or pid in ( select id from gcgl_pwe where 1=1 and pid ='{id}'))";
                }
                else if (state=="1")
                {
                    /////项目
                    //where = $" and id in ( select pid from gcgl_pwe where 1=1 and id ='{id}')";
                    //result = dbHelper.GetDataTable(CommandType.Text, (sql + where + order));
                    //list[0].work = DataTableToList(result);
                    /////事件
                    //where = $"pid ='{id}'";
                    //result = dbHelper.GetDataTable(CommandType.Text, (sql + where + order));
                    //list[0].eventlist = DataTableToList(result);
                    where = $" or( pid='{id}' or id in ( select pid from gcgl_pwe where 1=1 and id ='{id}'))";
                }
                else if (state=="2")
                {
                     where = $" or (id in ( select pid from gcgl_pwe where id ='{id}') or id in (select pid from gcgl_pwe  where id in (select pid from gcgl_pwe  where id ='{id}')))";
                }
               // where = $" and (id ='{id}' or pid='{id}' or pid in ( select id from gcgl_pwe where 1=1 and pid ='{id}'))";
            }
            sql += where;
            sql += $"order by state , pid ,serialnumber desc";

            var result = dbHelper.GetDataTable(CommandType.Text, sql);
            List<PWE> list = DataTablePweToList(result);

            return list;
        }
        
        /// <summary>
        /// 统计数字
        /// </summary>
        /// <param name="state"></param>
        /// <param name="id"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public List<int> GetstaticCount(string state,string role,int []uid= null)
        {
            UserConfig con = new UserConfig();
            string sql = $"select count(*) from gcgl_pwe where 1=1 and state='{state}' and isdeleted='0'";
            string countsql = "";
            List<int> countnum = new List<int>();
            string IssingleF = $"and pid!='{Guid.Empty}'";//单独
            string IsFlagF = "and progress!='100'";///未完成
            string IsFlagT = "and progress='100'";///已完成
            string IsJoin = "";
            string isLeader = "";
            string where = "";
            if (uid != null && uid.Length > 0)
            {
                isLeader += $"(";
                IsJoin += $"(";
                for (int i = 0; i < uid.Length; i++)
                {
                    isLeader += $" leaderid='{uid[i]}' or";
                    IsJoin += $" POSITION(',{uid[i]},'in joinperson)>0 or";
                }
                IsJoin = IsJoin.Substring(0, IsJoin.Length - 2);
                IsJoin += ")";
                isLeader = isLeader.Substring(0, isLeader.Length - 2);
                isLeader += ")";
            }
           // string isname = $"and POSITION(',{id},'in joinperson)>0";
            string isname = "";
            //全部项目、全部工作、全部事件
            if (role == con.General_Manager || role == con.Vice_General_Manager)//局长、副局长
            {
                countnum.Add(dbHelper.GetCount(sql));
            }
            else //科长、科员
            {
                where = $"and(";
                where += $" {IsJoin} ";
                where += $" or ({isLeader})";
                if (state != "2")
                {
                    //参与事件的id
                    string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                    //参与工作的id
                    if (state == "1")//工作 项目
                    {
                        where += $" or id in ({join})";
                    }
                    if (state == "0")
                    {
                        where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF}and ({isLeader})or {IsJoin} or id in({join}) )";
                    }
                }
                where += ")";
                countsql = sql + where;
                if (role == con.Division_Manager||(role==con.Staff&&state=="0"))//科员没有全部工作、全部事件
                {
                    countnum.Add(dbHelper.GetCount(countsql));
                }
            }
            //进行中、单独
            if (state == "0")
            {
                if (role == con.General_Manager || role == con.Vice_General_Manager)
                {
                    //进行中
                    countsql = sql + IsFlagF;
                    countnum.Add(dbHelper.GetCount(countsql));
                    ////已完结
                    //countsql = sql + IsFlagT;
                    //countnum.Add(dbHelper.GetCount(countsql));
                }
                else
                {
                    where = IsFlagF;//
                    if (role != con.General_Manager || role != con.Vice_General_Manager)
                    {
                        where += $"and(";
                        where += $" {IsJoin} ";
                        //参与事件的id
                        string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                        where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF} and({isLeader} or{IsJoin} or id in({join})) )";
                        where += ")";
                    }
                    countsql = sql + where;
                    countnum.Add(dbHelper.GetCount(countsql));
                }
            }
            else
            {//单独
                if (role == con.General_Manager || role == con.Vice_General_Manager)
                {
                    countsql = sql + $" and pid = '{Guid.Empty}'";
                    countnum.Add(dbHelper.GetCount(countsql));
                }
            }
            ///已完结项目
            ///
            if (state=="0")
            {
                where = IsFlagT;//
                if (role != con.General_Manager || role != con.Vice_General_Manager)
                {
                    where += $"and(";
                    where += $" {IsJoin} ";
                    //参与事件的id
                    string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                    where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF} and({isLeader} or{IsJoin} or id in({join})) )";
                    where += ")";
                }
                countsql = sql + where;
                countnum.Add(dbHelper.GetCount(countsql));
            }
            ///负责
            countsql = sql + $"and leaderid='{uid[0]}'";
            countnum.Add(dbHelper.GetCount(countsql));
            ///参与
            ///
            if (uid != null && uid.Length > 0)
            {
                ///事件 工作 项目
                IsJoin = $" POSITION(',{uid[0]},'in joinperson)>0";
                where = $"and ({IsJoin})";
                //参与事件的id
                string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' { IssingleF} and({isLeader}or{IsJoin})";
                if (state == "1")//工作 项目
                {
                    where += $" or id in ({join})";
                }
                if (state == "0")
                {
                    where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0' {IssingleF} and({isLeader}or{IsJoin}) or id in({join}) )";
                }
            }
            countsql = sql + where;
            countnum.Add(dbHelper.GetCount(countsql));
            return countnum;
        }
        /// <summary>
        /// 根据工作 id对排序工作下事件进行排序
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public List<PWE> GetRankDatabyPid(string pid,string rank)
        {
            List<PWE> list = new List<PWE>();
            string sql = $"select * from gcgl_pwe where isdeleted='0' and pid='{pid}' order by serialnumber {rank}";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            list = DataTablePweToList(table);
            return list;
        }
        /// <summary>
        /// 更新事件的完成
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int Isfinishbyid(string id="",string state="")
        {
            int count = -1;
            string sql = $" update gcgl_pwe set flag='1' where id='{id}' and state='{state}'";
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        /// <summary>
        /// 后台项目管理 删除
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="state"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public string GetIndexpage(int? page,int? limit,string table="",string state="",string keyword="")
        {
            string sql =$" select * from {table} where isdeleted='0'";
            if (!string.IsNullOrEmpty(state))
            {
                sql += $" and( state='{state}')";
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                sql += $" and   POSITION('{keyword}'in name)>0";
            }
            sql += $" order by addtime desc  limit {limit} offset {(page - 1) * limit}";
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        /// <summary>
        /// 得到查询的总条数
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="table"></param>
        /// <param name="state"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public int GetIndexPageCount(int? page, int? limit, string table = "", string state = "", string keyword = "")
        {
            int count = -1;
            string sql = $" select count(*) from {table} where isdeleted='0'";
            if (!string.IsNullOrEmpty(state))
            {
                sql += $" and( state='{state}')";
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                sql += $" and  POSITION('{keyword}'in name)>0";
            }
            count = dbHelper.GetCount(sql);
            return count;
        }
        /// <summary>
        /// 根据状态、id删除项目工作事件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public int DeletedPwebyid(string id ,string state)
        {
            int count = -1;
            string sql = "";
            string where = "";
            PWE list = getpwebyid(new Guid(id));
            if (list!=null)
            {
                if (!string.IsNullOrEmpty(list.leaderid.ToString())||!string.IsNullOrEmpty(list.joinperson))
                {
                    string join = list.leaderid + ',' + list.joinperson;
                    if (!string.IsNullOrEmpty(join))
                    {
                        string[] joinid = join.Split(',');
                        if (joinid != null&&joinid.Length>0)
                        {
                            foreach (var item in joinid)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    User user = getUser(item);
                                    if (user != null && !string.IsNullOrEmpty(user.department))
                                    {
                                        count = SaveOrgPwe(list.pid, list.id, user.department, list.state, 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (state!="2")
            {
                where = $" select id from  gcgl_pwe where id ='{id}'or pid ='{id}'";// 项目下工作、工作下事件
                if (state=="0")
                {
                    where += $" or id in ( select id from gcgl_pwe where pid in  (select id from gcgl_pwe where pid ='{id}'))";//项目下的工作下的事件
                }
            }
            
            sql = $" update gcgl_pwe  set isdeleted='1' where id='{id}' ";
            if (!string.IsNullOrEmpty(where))
            {
                sql += $"  or id in ({ where})";
            }
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        /// <summary>
        /// 通过pid获取数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<PWE> GetPwebyPid(Guid id)
        {
            string sql = $" select * from gcgl_pwe where pid ='{id}' order by serialnumber desc ";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<PWE> list = DataTablePweToList(table);
            return list;
        }
        public string getAllData(string tableName, string page, string size)
        {
            string sql = $"select * from {tableName}";
            string result = dbHelper.GetAsJson(CommandType.Text, sql).ToString();
            return result;
        }
        /// <summary>
        /// 根据表名和where语句获取条数
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="where"></param>
        /// <returns></returns>
        public int getCount(string tableName, string where)
        {
            string sql = $" select count(*) from {tableName} where 1=1 {where}";
            int count = dbHelper.GetCount(sql);
            return count;
        }

        #endregion
        #region 导出Excel查询pwe
        /// <summary>
        /// type 0 基础查询 1查询项目下的工作 工作下的事件
        /// state 0 项目 1 工作 2 事件 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pweflag"></param>
        /// <param name="keyword"></param>
        /// <param name="lable"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public DataTable DownExcel(string type="",  string state="",string pweflag="",string keyword="",string lable="",string[] pid=null)
        {
            DataTable table = null;
            string where = "";
            string sql = "";
            sql = $"select (select name from sys_user where id=gcgl_pwe.leaderid) leadername,* from gcgl_pwe  where isdeleted ='0' and state='{state}'";
            switch (type)
            {
                case "0"://基础查询
                    {
                        where = wheresql(state, pweflag, keyword, lable);
                    }
                    break;
                case "1"://导出
                    {
                        where = whereexcel(pid); 
                    }
                    break;
                default:
                    break;
            }
           
            sql = sql + where;
            table = dbHelper.GetDataTable(CommandType.Text, sql);
            if (state!="0")//工作 事件
            {
                if (state=="2")
                {
                    if (table != null && table.Rows.Count > 0)
                    {
                        table.Columns.Add("wname", typeof(string));
                        for (int i = 0; i < table.Rows.Count; i++)
                        {
                            Guid guid = new Guid(table.Rows[i]["pid"].ToString());
                            if (guid != null && guid != Guid.Empty)
                            {
                                table.Rows[i]["wname"] = getpwebyid(guid).name;
                            }
                        }
                    }
                }
                //工作 事件
                if (table!=null&&table.Rows.Count>0)
                {
                    table.Columns.Add("pname", typeof(string));
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        Guid guid = new Guid(table.Rows[i]["pid"].ToString());
                        if (guid != null&&guid!=Guid.Empty)
                        {
                            table.Rows[i]["pname"] = getpwebyid(guid).name;
                        }
                    }
                }
            }
            return table;
        }
        private string whereexcel(string [] pid=null)
        {
            string where = "";
            if (pid != null && pid.Length > 0)
            {
                where = " and (";
                for (int k = 0; k < pid.Length; k++)
                {
                    where += $" pid='{pid[k]}' or";
                }
                where = where.Substring(0, where.Length - 2);
                where += " )";
            }
            return where;
        }
        #endregion
        #region 属性字段
        /// <summary>
        /// 通过pid得到属性字段
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public List<projectotherfield> GetotherfieldsbyPid(string pid="")
        {
            string sql = $" select * from gcgl_projectotherfield where 1=1 ";
            if (!string.IsNullOrEmpty(pid))
            {
                sql += $" and ( pid ='{ pid}')";
            }
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<projectotherfield> list = DataTableOtherfiled(table);
            return list;
        }

        #endregion
        #region 模板

        /// <summary>
        /// 添加模板管理
        /// flag 0项目 1工作
        /// </summary>
        /// <param name="name"></param>
        /// <param name="itemname"></param>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public int SaveTemplet(string itemname,string name ,string flag,string id = "")
        {
            string sql = "";
            int count = -1;
            if (new Guid(id)==Guid.Empty)
            {
                Guid pid = Guid.NewGuid();
                sql = $"insert into gcgl_templet(id,pid,name,sort,flag)";
                if (!string.IsNullOrEmpty(name))
                {
                    sql += $"values('{pid}','{Guid.Empty}','{name}',0,'{flag}')";
                }
                if (!string.IsNullOrEmpty(itemname))
                {
                    var step = itemname.Split(',');
                    for (int i = 0; i < step.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(step[i]))
                        {
                            sql += $",('{Guid.NewGuid()}','{pid}','{step[i].Trim()}','{i + 1}','{flag}')";
                        }

                    }
                }
                count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            }
            else
            {
                sql = $"delete from gcgl_templet where pid='{id}'";
                count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                if (count >-1)
                {
                    sql = $"update gcgl_templet set name='{name}',flag='{flag}' where id='{id}';";
                    if (!string.IsNullOrEmpty(itemname))
                    {
                        sql += $"insert into gcgl_templet(id,pid,name,sort,flag) values";
                        var step = itemname.Split(',');
                        for (int i = 0; i < step.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(step[i]))
                            {
                                sql += $"('{ Guid.NewGuid()}','{id}','{step[i].Trim()}','{i + 1}','{flag}'),";
                            }
                        }
                        sql = sql.Substring(0, sql.Length - 1);
                    }
                    count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
                }
            }
            return count;
        }
        /// <summary>
        /// 查询页面模板页面
        /// </summary>
        /// <param name="state"></param>
        /// <param name="pid"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<templet> GetpageTemplet( string state,Guid? pid,int? page=0, int? limit=0,string keyword="")
        {
            string sql = "";
            sql = $"select * from gcgl_templet where isdeleted='0' and pid='{pid}'";
            if (!string.IsNullOrEmpty(state))
            {
                sql += $" and flag='{state}'";
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                sql += $"and ( POSITION('{keyword}'in name)>0)";
            }
            if (limit != 0)
            {
                sql += $" limit {limit} offset {(page - 1) * limit}";
            }
            DataTable result = dbHelper.GetDataTable(CommandType.Text, sql);
            List<templet> list = DataTableTemplettoList(result);
            return list;
        }
        /// <summary>
        /// 通过flag或pid获取模板
        /// </summary>
        /// <param name="state"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetTempletbyflagorPid(string state="",string id = "")
        {
            string sql = $" select * from gcgl_templet where 1=1 and isdeleted='0'";
            if (!string.IsNullOrEmpty(state))
            {
                sql += $" and flag= '{state}'";
            }
            if (!string.IsNullOrEmpty(id))
            {
                sql += $" and pid= '{id}'";
            }
            else
            {
                sql += $" and pid= '{Guid.Empty}'";
            }
            string result = dbHelper.GetAsJson(CommandType.Text, sql);
            return result;
        }
        /// <summary>
        /// 根据模板id得到模板的详情
        /// </summary>
        /// <param name="state"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public templet  GetTempletDetailById(string state,Guid id)
        {
            string sql = $"select * from gcgl_templet where isdeleted='0' and id='{id}'";
            if (!string.IsNullOrEmpty(state))
            {
                sql += $" and flag='{state}'";
            }
            DataTable result = dbHelper.GetDataTable(CommandType.Text, sql);
            List<templet> list = DataTableTemplettoList(result);
            return list[0];
        }
        /// <summary>
        ///根据模板id 、pid 删除模板
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DelTempletById(string id)
        {
            string sql = $"update gcgl_templet set isdeleted='1' where id='{id}' or pid ='{id}'";
            int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        #endregion
        #region 评论
        /// <summary>
        /// 保存评论信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int SaveComment(comment model)
        {
            string sql = "";
            int count = -1;
            if (model.id==Guid.Empty)
            {
                model.id = Guid.NewGuid();
                sql = $"insert into gcgl_comment (id,pid,adduserid,content,addtime)values" +
                    $"('{model.id}','{model.pid}','{model.adduserid}','{model.content}','{model.addtime}')";
                sql += $" ; update gcgl_pwe set commentcount=commentcount+1 where id='{model.pid}'";
                count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            }
            else
            {
                sql = $" update gcgl_comment set content='{model.content}' where pid='{model.pid}'and id='{model.id}'";
                count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            }
            
            return count;
        }
        /// <summary>
        /// 通过pid得到评论的信息
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public List<comment> GetCommentbypid(string pid = "")
        {
            string sql = $" select a.* ,b.name addusername from gcgl_comment a,sys_user b where isdeleted='0' and to_number(a.adduserid, '99G999D9S')=b.id ";
            if (!string.IsNullOrEmpty(pid))
            {
                sql += $" and pid ='{pid}' ";
            }
            sql += " order by addtime ";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<comment> list = DataTableCommentToList(table);
            return list;
        }
        #endregion
        #region 新闻
        /// <summary>
        /// 保存新闻公告的信息
        /// </summary>
        /// <param name="modle"></param>
        /// <returns></returns>
        public int SaveNews(news modle,Guid id)
        {
            int count = -1;
            string sql = "";
            if (modle.id==Guid.Empty)
            {
                sql = $" insert into gcgl_news  (id,name,brief,content,addtime,updatetime,flag)values" +
                    $"('{id}','{modle.name}','{modle.brief}','{modle.content}','{modle.addtime}','{modle.updatetime}','{modle.flag}')";
            }
            else
            {
                sql = $" update gcgl_news set name='{modle.name}',brief='{modle.brief}',content='{modle.content}'" +
                    $",flag='{modle.flag}',updatetime='{modle.updatetime}' where id='{modle.id}'";
            }
            count = dbHelper.ExecuteNonQuery(CommandType.Text,sql);
            return count;
        }
        /// <summary>
        /// 通过id得到新闻公告的详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public news GetNewsbyId(string id)
        {
            string sql = $" select * from gcgl_news where isdeleted='0'";
            if (!string.IsNullOrEmpty(id))
            {
                sql += $" and id='{id}'";
            }
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<news> list = DataTableNewsToList(table);
            return list.Count>0?list[0]:null;  
        }
        /// <summary>
        /// 通过id删除新闻公告
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DeletedNewsbyId(string id)
        {
            int count = -1;
            string sql = $" update gcgl_news set isdeleted ='1' where id='{id}'";
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        #endregion
        #region 邮件中心
        /// <summary>
        /// 保存邮件信息
        /// </summary>
        /// <param name="modle"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int SaveDocument(Document modle,Guid? id)
        {
            int count = -1;
            string sql = "";
            if (modle.id==Guid.Empty)
            {
                sql = $"insert into gcgl_document(id ,adduserid,name,content,upload,addtime,updatetime,issend)" +
                    $"values('{id}','{modle.adduserid}','{modle.name}','{modle.content}','{modle.upload}','{modle.addtime}'," +
                    $"'{modle.updatetime}','{modle.issend}')";
            }
            else
            {
                sql = $" update gcgl_document set name='{ modle.name}',content='{modle.content}',upload='{modle.upload}',updatetime='{modle.updatetime}', issend='{modle.issend}' where id='{modle.id}'";
            }
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        /// <summary>
        /// 通过id得到邮件信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Document GetDocumentbyid(Guid id)
        {
            string sql = "";
            sql = $" select * from gcgl_document where id ='{id}'";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<Document> list = DataTableDocumentToList(table);
            return list.Count > 0 ? list[0] : null;
        }
        /// <summary>
        /// 查询邮件列表 
        /// flag 0 草稿箱 1 发件箱 2 已读邮件 3 未读邮件
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public List<Document> GetDocumentbyflag(string flag ,int ? user=null)
        {
            string sql = $"select a.*,b.name as addusername from gcgl_document a,sys_user b where a.adduserid=b.id";
            string where = "";
            switch (flag)
            {
                case"1"://草稿箱
                    {
                        where += $" and  a.isdeleted='0'";
                        where += $" and (a.adduserid='{user}' and a.issend='false' )";
                    }
                    break;
                case "2"://发件箱
                    {
                        where += $" and   a.isdeleted='0'";
                        where += $" and (a.adduserid='{user}' and a.issend='true' )";
                    }
                    break;
                case "3"://未读邮件
                    {
                        where = $" and a.id in( select pid from gcgl_send_receive where receiveperson='{user}'and isread='0' and isdeleted='0')";
                    }
                    break;
                case "4"://已读邮件
                    {
                        where = $" and a.id in( select pid from gcgl_send_receive where receiveperson='{user}'and isread='1' and isdeleted='0')";
                    }
                    break;
                case "5"://收件箱
                    {
                        sql = $"select a.*,b.name as addusername ,c.isread as isread from gcgl_document a,sys_user b, gcgl_send_receive c where a.adduserid=b.id and c.pid=a.id and receiveperson='{user}' and c.isdeleted='0'";
                    }
                    break;
                default:
                    break;
            }
            sql = sql + where;
            sql += $" order by addtime desc";
            DataTable table = dbHelper.GetDataTable(CommandType.Text,sql);
            List<Document> list = DataTableDocumentToList(table);
            return list;
        }
        /// <summary>
        /// 根据 邮件状态、接受人 删除邮件
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int DelDocumentbyIdFlag(string id ,string flag,string userid)
        {
            string sql = "";
            int count = -1;
            if (flag == "1"||flag=="2")
            {
                sql = $"update gcgl_document set isdeleted ='1' where id='{id}'";
            }
            else
            {
                sql = $" update gcgl_send_receive set isdeleted='1' where pid='{id}' and receiveperson='{userid}'";
            }
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;

        }
        #endregion
        #region 发送接受
        /// <summary>
        /// 保存接受发送信息
        /// </summary>
        /// <param name="modle"></param>
        /// <returns></returns>
        public int SaveSend_receive(send_receive modle)
        {
            int count = -1;
            string sql = "";
            
            if (modle.id==Guid.Empty)
            {
                sql = $" insert into gcgl_send_receive (id,pid,sendperson,receiveperson,sendtime,isread,isrepost)" +
                    $"values('{Guid.NewGuid()}','{modle.pid}','{modle.sendperson}','{modle.receiveperson}','{modle.sendtime}'," +
                    $"'{modle.isread}','{modle.isrepost}')";
            }
            else
            {
                sql = $" update gcgl_send_receive set isread='{modle.isread}' where id='{modle.id}'";
            }
            count = dbHelper.ExecuteNonQuery(CommandType.Text,sql);
            return count;
        }
        /// <summary>
        /// 更新接受发送表
        /// 更新已读状态
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="receiveid"></param>
        /// <returns></returns>
        public int IsreadbyPidUser(Guid pid,int receiveid)
        {
            string sql = "";
            sql = $" update gcgl_send_receive set isread='1' where isdeleted='0' and pid='{pid}' and receiveperson='{receiveid}'";
            int count = dbHelper.ExecuteNonQuery(CommandType.Text,sql);
            return count;
        }
        /// <summary>
        /// 通过pid、接受人id得到接受发送信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="receiveid"></param>
        /// <returns></returns>
        public send_receive GetAllReceiveidbyPid(Guid id,int receiveid)
        {
            string sql = $"select * from gcgl_send_receive where pid='{id}'and receiveperson ='{receiveid}'";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<send_receive> list = DataTableSend_ReceivesToList(table);
            return list.Count>0?list[0]:null;
        }

        public List<send_receive> GetAllRecenamebyPid(Guid id)
        {
           // string sql = $"select * from gcgl_send_receive where pid='{id}'";
            string sql = $"select a.*,b.name  from gcgl_send_receive a,sys_user b where a.pid='{id}' and a.receiveperson=b.id";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<send_receive> list = DataTableSend_ReceivesToList(table);
            return list;
        }
        #endregion
        #region 消息
        /// <summary>
        /// 保存消息
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int SaveMessage(List<message> list)
        {

            string sql = "";
            if (list!=null&&list.Count>0)
            {
                sql = $"insert into gcgl_message (id,mid,name,leaderid,assignid,sendtime,edittype,type)values";
                foreach (var modle in list)
                {
                    string delSql = $"delete from gcgl_message where mid='{modle.mid}' and leaderid='{modle.leaderid}' and assignid='{modle.assignid}' and edittype='{modle.edittype}'";
                    dbHelper.ExecuteNonQuery(CommandType.Text, delSql);
                    sql += $"('{Guid.NewGuid()}','{modle.mid}','{modle.name}','{modle.leaderid}','{modle.assignid}'" +
                        $",'{modle.sendtime}','{modle.edittype}','{modle.type}'),";
                }
                sql = sql.Substring(0, sql.Length - 1);
            }
            
            int count=dbHelper.ExecuteNonQuery(CommandType.Text,sql);
            return count;
        }
        /// <summary>
        /// 通过用户id得到消息列表
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<message> GetMessageByUserid(string id)
        {
            string sql = $"select * from gcgl_message where isdeleted='0'and (( leaderid='{id}' and type!='3') or assignid='{id}') order by sendtime desc";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<message> list = DataTableMessageToList(table);
            return list;
        }
        /// <summary>
        /// 通过id改变消息的已读状态
        /// 并且改变消息反馈中的状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
         public int MsgIsreadById(string id,int userid)
        {
            string sql = $" update  gcgl_message  set isread='1',readtime='{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}' where id='{id}'";
            sql += $";update gcgl_messagefeedback set msgisreadid =(case  when (msgisreadid is null)then '{userid},' else  msgisreadid||'{userid},' end), updatetime='{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}'" +
                $" where mid in (select mid from gcgl_message where id='{id}')";
            int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }

        public int DeleteMsgById(string id,string state,int userid)
        {
            string sql = " update gcgl_message set isdeleted ='1' where 1=1 and isread='1'";
            if (state=="1")//根据人员删除已读的消息
            {
                sql += $" and ((leaderid='{userid}' and type!='3') or assignid='{userid}')";
            }
            else//根据id删除已读消息
            {
                sql += $" and (id='{id}')";
            }
            int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        #endregion
        #region 消息反馈
        /// <summary>
        /// 添加消息反馈
        /// </summary>
        /// <param name="modle"></param>
        /// <returns></returns>
        public int SaveMessageFeedback(messagefeedback modle)
        {
            string sql = "";
            string delSql = $"delete from gcgl_messagefeedback where mid='{modle.mid}'";
            sql = $"insert into gcgl_messagefeedback (id ,mid,mname,msgreceid,addtime,updatetime,state,msguserid)" +
                $"values('{Guid.NewGuid()}','{modle.mid}','{modle.mname}','{modle.msgreceid}','{modle.addtime}','{modle.updatetime}'," +
                $"'{modle.state}','{modle.msguserid}')";
            dbHelper.ExecuteNonQuery(CommandType.Text, delSql);
            int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        /// <summary>
        /// 根据用户id获取消息反馈
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public  List<messagefeedback> GetMsgFeedback(int userid)
        {
            string sql = $"select * from gcgl_messagefeedback where isdeleted ='0' and  msguserid='{userid}'";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<messagefeedback> list = DataTableMsgFeedbackToList(table);
            return list ;
        }
        /// <summary>
        /// 通过Id获取消息反馈
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<messagefeedback> GetMsgFBById(string id)
        {
            string sql = $" select * from gcgl_messagefeedback where isdeleted ='0' and id='{id}' order by addtime desc";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<messagefeedback> list = DataTableMsgFeedbackToList(table);
            return list;
        }
        /// <summary>
        /// 根据状态删除单个或多个消息反馈
        /// 0单个 1多个
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public int DelMsgFBById(string id,string state,string userid="")
        {
            string sql = $" update gcgl_messagefeedback set isdeleted ='1' where 1=1 ";
            string where = "";
            if (state=="0")//单个删除
            {
                where += $" and id='{id}'";
            }
            else
            {
                where += $" and msguserid='{userid}'";
            }
            sql += where;
            int count =dbHelper.ExecuteNonQuery(CommandType.Text,sql);
            return count;
        }
        
        #endregion
        #region 日程
        /// <summary>
        /// 保存日程数据
        /// </summary>
        /// <param name="modle"></param>
        /// <returns></returns>
        public int SaveSchedule(Schedule modle)
        {
            string sql = "";
            if (modle.id==Guid.Empty)
            {
                sql += $"insert into gcgl_schedule (id, name,content,starttime,endtime,addtime,isallday,adduserid)values('{Guid.NewGuid()}','{modle.name}','{modle.content}','{modle.starttime}','{modle.endtime}','{modle.addtime}','{modle.isallday}','{modle.adduserid}')";
            }
            else
            {
                sql = $" update gcgl_schedule set name='{modle.name}', content='{modle.content}',starttime='{modle.starttime}'," +
                    $"endtime='{modle.endtime}',isallday='{modle.isallday}' where id='{modle.id}'and adduserid='{modle.adduserid}'";

            }
            int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        /// <summary>
        /// 根据用户 查询年月、
        /// state 0上月  1 下月
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="datetime"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<Schedule> GetSchedule(string userid,string datetime,string state)
        {
            string sql = $"select * from gcgl_schedule where isdeleted='0' and adduserid='{userid}'";
            if (!string.IsNullOrEmpty(datetime))
            {
                sql += $" and POSITION('{datetime}'in starttime) >0";
            }
            sql += " order by addtime desc";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<Schedule> list = DataTableScheduleToList(table);
            return list;
        }
        /// <summary>
        /// 通过id获取日程信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Schedule> GetScheduleById(string id)
        {
            string sql = $"select *  from gcgl_schedule where isdeleted ='0' and id='{id}'";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<Schedule> list = DataTableScheduleToList(table);
            return list;
        }
        /// <summary>
        /// 根据id删除日程
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int DelScheduleById(string id)
        {
            string sql = $" update gcgl_schedule set isdeleted='1' where id='{id}'";
            int count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        #endregion
        #region 用户
        /// <summary>
        /// 得到用户信息通过id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User getUser(string id)
        {
            string sql = $"select * ,(select array_to_string(array_agg(role_flag),',') rolename from (select* from sys_role where id in" +
                $"(select to_number(unnest(string_to_array(role_id, ',')), '99G999D9S')from sys_user where id = '{id}')) rolename)from sys_user where id = '{id}'";
            //string sql = $"select * from sys_user where id='{id}'";

            User model = null;
            try
            {
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    model = new User();
                    model.id = Int32.Parse(dt.Rows[0]["id"].ToString());
                    model.username = dt.Rows[0]["username"].ToString();
                    model.name = dt.Rows[0]["name"].ToString();
                    model.department = dt.Rows[0]["department"].ToString();
                    model.telephone= dt.Rows[0]["telephone"].ToString();
                    model.password = dt.Rows[0]["password"].ToString();
                    model.role_id = dt.Rows[0]["role_id"].ToString();
                    model.rolecode = dt.Rows[0]["rolename"].ToString();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return model;
        }
        /// <summary>
        /// 得到所有用户
        /// </summary>
        /// <returns></returns>
        public List<User> getAllUser()
        {
            //string sql = $"select * from public.sys_user ";
            string sql = $"select * from public.sys_user ";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<User> list = DataTableUserToList(table);
            return list;
        }

        public int UpdatePassword(string id,string password)
        {
            int count = -1;
            string sql = $" update sys_user set password='{password}'where id='{id}' ";
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            return count;
        }
        #endregion
        #region 全局搜索
        /// <summary>
        /// flag 0 项目 1 工作 2 事件 3 人员
        /// state 0 名称 1 内容 2  标签 3 时间 4 姓名
        /// 
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="state"></param>
        /// <param name="keyword"></param>
        /// <param name="role"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<PWE> GetAllSearch(string flag="",string state="",string keyword="",string role="3", int [] userid=null)
        {
           
            string sql = $"select * from gcgl_pwe where 1=1 and isdeleted='0' and state='{flag}'";
            string where = "";
            string isLeader = "";
            string IsJoin = "";
            string IssingleF = $"and pid!='{Guid.Empty}'";//单独
            switch (state)
            {
                case "0"://名称
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            where += $" and ( POSITION ( '{keyword}'in name )>0)";
                        }
                    }
                    break;
                case "1"://内容
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            where += $" and ( POSITION ( '{keyword}'in content )>0)";
                        }
                    }
                    break;
                case "2"://标签
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            where += $" and ( POSITION ( '{keyword}'in lable )>0)";
                        }
                    }
                    break;
                case "3"://事件
                    {
                        if (!string.IsNullOrEmpty(keyword))
                        {
                            string[] Time = new string[2];
                            Time[0] = keyword.Substring(0, 10);
                            Time[1] = keyword.Substring(13);
                            where += $" and ( starttime >='{Time[0]}' and  starttime <='{Time[1]}')";
                        }
                    }
                    break;
                case "4"://人员
                    {
                        where += $" and ( leaderid='{userid[0]}')";
                    }
                    break;
                default:
                    break;
            }
            switch (role)
            {
                case "0"://总经理、副总
                case "1":
                    {
                    }
                    break;
                case "2"://部门经理
                    {
                        if (userid != null && userid.Length > 0)//参与
                        {
                            isLeader += $"(";
                            IsJoin += $"(";
                            for (int i = 0; i < userid.Length; i++)
                            {
                                isLeader += $" leaderid='{userid[i]}' or";
                                IsJoin += $" POSITION(',{userid[i]},'in joinperson)>0 or";

                            }
                            IsJoin = IsJoin.Substring(0, IsJoin.Length - 2);
                            IsJoin += ")";
                            isLeader = isLeader.Substring(0, isLeader.Length - 2);
                            isLeader += ")";
                            
                        }
                        where += $"and(";
                        where += $" {IsJoin} ";
                        where += $" or ({isLeader})";
                        if (state != "2")
                        {
                            //参与事件的id
                            string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' {IssingleF} and ({isLeader} or{IsJoin})";
                            //参与工作的id
                            if (state == "1")//工作 项目
                            {
                                where += $" or id in ({join})";
                            }
                            if (state == "0")
                            {
                                where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0'{IssingleF}and ({isLeader})or {IsJoin} or id in({join}) )";
                            }
                        }
                        where += ")";
                    }
                    break;
                case "3"://员工
                    {
                        string  isjoin= $"(leaderid='{userid[0]}' or POSITION(',{userid[0]},'in joinperson)>0)";
                        where += $"and ({isjoin}";
                        //参与事件的id
                        string join = $"select pid from  gcgl_pwe where state ='2' and isdeleted ='0' and {isjoin}";
                        if (flag == "1")//工作 项目
                        {
                            where += $" or id in ({join})";
                        }
                        if (flag == "0")
                        {
                            where += $" or id in (select pid from gcgl_pwe where state ='1' and isdeleted='0' and  {isjoin} or id in({join}) )";
                        }
                        where += ")";
                    }
                    break;
                default:
                    break;
            }
            sql += where;
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<PWE> list = DataTablePweToList(table);
            return list;
        }

        #endregion

        #region 私有函数转化

        /// <summary>
        /// 转化项目工作事件
        /// </summary>
        /// <param name="datatable"></param>
        /// <returns></returns>
        private List<PWE> DataTablePweToList(DataTable datatable)
        {
            List<PWE> pwe = new List<PWE>();
            PWE modle =null;
            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                modle = new PWE();
                modle.id = new Guid(datatable.Rows[i]["id"].ToString());
                modle.pid = new Guid(datatable.Rows[i]["pid"].ToString());
                if (!string.IsNullOrEmpty(datatable.Rows[i]["adduserid"].ToString()))
                {
                    modle.adduserid = Convert.ToInt32(datatable.Rows[i]["adduserid"].ToString());
                }
                if (!string.IsNullOrEmpty(datatable.Rows[i]["leaderid"].ToString()))
                {
                    modle.leaderid = Convert.ToInt32(datatable.Rows[i]["leaderid"].ToString());
                }
                modle.joinperson = datatable.Rows[i]["joinperson"].ToString();
                modle.name = datatable.Rows[i]["name"].ToString();
                modle.content = datatable.Rows[i]["content"].ToString();
                modle.location = datatable.Rows[i]["location"].ToString();
                modle.address = datatable.Rows[i]["address"].ToString();
                modle.lable = datatable.Rows[i]["lable"].ToString();
                modle.progress = datatable.Rows[i]["progress"].ToString();
                modle.starttime = datatable.Rows[i]["starttime"].ToString();
                modle.endtime = datatable.Rows[i]["endtime"].ToString();
                modle.addtime = datatable.Rows[i]["addtime"].ToString();
                modle.updatetime = datatable.Rows[i]["updatetime"].ToString();
                modle.upload = datatable.Rows[i]["upload"].ToString();
                modle.flag = datatable.Rows[i]["flag"].ToString();
                modle.commentcount = datatable.Rows[i]["commentcount"].ToString();
                modle.serialnumber = Convert.ToInt32(datatable.Rows[i]["serialnumber"].ToString());
                modle.isdeleted = datatable.Rows[i]["isdeleted"].ToString();
                modle.state = Convert.ToInt32(datatable.Rows[i]["state"].ToString());
                pwe.Add(modle);
            }
            return pwe;
        }

        /// <summary>
        /// 转化标签
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<string> DataTableToLableList(DataTable dataTable)
        {
            List<string> list = new List<string>();
            string lable;
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                lable =(dataTable.Rows[i]["lable"].ToString()==null|| dataTable.Rows[i]["lable"].ToString() == "") ? null : dataTable.Rows[i]["lable"].ToString();
                if (lable!=null)
                {
                    list.Add(lable);
                }
            }
            return list;
        }
        /// <summary>
        /// 转化模板
        /// </summary>
        /// <param name="datatable"></param>
        /// <returns></returns>
        private List<templet> DataTableTemplettoList(DataTable datatable)
        {
            List<templet> list = new List<templet>();
            templet modle = null;
            for (int i = 0; i < datatable.Rows.Count; i++)
            {
                modle = new templet();
                modle.id = new Guid(datatable.Rows[i]["id"].ToString());
                modle.pid = new Guid(datatable.Rows[i]["pid"].ToString());
                modle.name = datatable.Rows[i]["name"].ToString();
                modle.flag = Convert.ToInt32(datatable.Rows[i]["flag"].ToString());
                //modle.isdeleted = Convert.ToInt32(datatable.Rows[i]["isdeleted"].ToString());
                modle.sort = Convert.ToInt32(datatable.Rows[i]["sort"].ToString());
                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 属性字段转化
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<projectotherfield> DataTableOtherfiled(DataTable dataTable)
        {
            List<projectotherfield> list = new List<projectotherfield>();
            projectotherfield modle = null;
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                modle = new projectotherfield();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.pid = new Guid(dataTable.Rows[i]["pid"].ToString());
                modle.name = dataTable.Rows[i]["name"].ToString();
                modle.content = dataTable.Rows[i]["content"].ToString();
                modle.flag = Int32.Parse(dataTable.Rows[i]["flag"].ToString());
                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 转化评论
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<comment> DataTableCommentToList(DataTable dataTable)
        {
            List<comment> list = new List<comment>();
            comment modle = null;
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                modle = new comment();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.pid = new Guid(dataTable.Rows[i]["pid"].ToString());
                modle.adduserid = Int32.Parse(dataTable.Rows[i]["adduserid"].ToString());
                modle.content = dataTable.Rows[i]["content"].ToString();
                modle.addtime = dataTable.Rows[i]["addtime"].ToString();
                modle.isdeleted = Boolean.Parse(dataTable.Rows[i]["isdeleted"].ToString());
                if (dataTable.Columns.Count>6)
                {
                    modle.addusername = dataTable.Rows[i]["addusername"].ToString();
                }
                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 转化新闻公告
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<news> DataTableNewsToList(DataTable dataTable)
        {
            List<news> list = new List<news>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                news modle = new news();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.name = dataTable.Rows[i]["name"].ToString();
                modle.brief = dataTable.Rows[i]["brief"].ToString();
                modle.content = dataTable.Rows[i]["content"].ToString();
                modle.addtime = dataTable.Rows[i]["addtime"].ToString();
                modle.updatetime = dataTable.Rows[i]["updatetime"].ToString();
                modle.flag = Int32.Parse(dataTable.Rows[i]["flag"].ToString());
                modle.isdeleted = Int32.Parse(dataTable.Rows[i]["isdeleted"].ToString());

                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 转化公文
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<Document> DataTableDocumentToList(DataTable dataTable)
        {
            List<Document> list = new List<Document>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                Document modle = new Document();
                modle.id = Guid.Parse(dataTable.Rows[i]["id"].ToString());
                modle.adduserid = Int32.Parse(dataTable.Rows[i]["adduserid"].ToString());
                modle.name = dataTable.Rows[i]["name"].ToString();
                modle.content = dataTable.Rows[i]["content"].ToString();
                modle.upload = dataTable.Rows[i]["upload"].ToString();
                modle.addtime = dataTable.Rows[i]["addtime"].ToString();
                modle.updatetime = dataTable.Rows[i]["updatetime"].ToString();
                modle.issend = Boolean.Parse(dataTable.Rows[i]["issend"].ToString());
                modle.isdeleted = Int32.Parse(dataTable.Rows[i]["isdeleted"].ToString());
                if (dataTable.Columns.Count>9)
                {
                    modle.addusername = dataTable.Rows[i]["addusername"].ToString();
                    if (dataTable.Columns.Count > 10)
                    {
                        modle.isread = Int32.Parse(dataTable.Rows[i]["isread"].ToString());
                    }
                }
                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 转化接受发送表
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<send_receive> DataTableSend_ReceivesToList(DataTable dataTable)
        {
            List<send_receive> list = new List<send_receive>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                send_receive modle = new send_receive();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.pid = new Guid(dataTable.Rows[i]["pid"].ToString());
                modle.sendperson = Int32.Parse(dataTable.Rows[i]["sendperson"].ToString());
                modle.sendtime = dataTable.Rows[i]["sendtime"].ToString();
                modle.isread = Int32.Parse(dataTable.Rows[i]["isread"].ToString());
                modle.isrepost = Int32.Parse(dataTable.Rows[i]["isrepost"].ToString());
                modle.isdeleted = Int32.Parse(dataTable.Rows[i]["isdeleted"].ToString());
                if (dataTable.Columns.Count>8)
                {
                    modle.recename = dataTable.Rows[i]["name"].ToString();
                }
                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 转化用户
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<User> DataTableUserToList(DataTable dataTable)
        {
            List<User> list = new List<User>();
            if (dataTable != null && dataTable.Rows != null && dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    User model = new User();
                    model = new User();
                    model.id = Int32.Parse(dataTable.Rows[i]["id"].ToString());
                    model.name = dataTable.Rows[i]["name"].ToString();
                    model.department = dataTable.Rows[i]["department"].ToString();
                    model.telephone = dataTable.Rows[i]["telephone"].ToString();
                    model.role_id = dataTable.Rows[i]["role_id"].ToString();
                    model.password = dataTable.Rows[i]["password"].ToString();
                    model.username = dataTable.Rows[i]["username"].ToString();
                    if (dataTable.Columns.Count>7)
                    {
                        model.rolecode = dataTable.Rows[i]["rolename"].ToString();
                    }
                    list.Add(model);
                }
            }
            return list;
        }
        /// <summary>
        /// 转化消息
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<message> DataTableMessageToList(DataTable dataTable)
        {
            List<message> list = new List<message>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                message modle = new message();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.name = dataTable.Rows[i]["name"].ToString();
                modle.leaderid = int.Parse(dataTable.Rows[i]["leaderid"].ToString());
                modle.assignid = dataTable.Rows[i]["assignid"].ToString();
                modle.sendtime = dataTable.Rows[i]["sendtime"].ToString();
                modle.readtime = dataTable.Rows[i]["readtime"].ToString();
                modle.edittype = Int32.Parse(dataTable.Rows[i]["edittype"].ToString());
                modle.type = Int32.Parse(dataTable.Rows[i]["type"].ToString());
                modle.isdeleted = Int32.Parse(dataTable.Rows[i]["isdeleted"].ToString());
                modle.isread = Int32.Parse(dataTable.Rows[i]["isread"].ToString());
                modle.mid = new Guid(dataTable.Rows[i]["mid"].ToString());
                list.Add(modle);
            }
            return list;
        }
        /// <summary>
        /// 转化消息反馈
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        private List<messagefeedback> DataTableMsgFeedbackToList(DataTable dataTable)
        {
            List<messagefeedback> list = new List<messagefeedback>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                messagefeedback modle = new messagefeedback();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.mid = new Guid(dataTable.Rows[i]["mid"].ToString());
                modle.mname = dataTable.Rows[i]["mname"].ToString();
                modle.msgreceid = dataTable.Rows[i]["msgreceid"].ToString();
                modle.addtime = dataTable.Rows[i]["addtime"].ToString();
                modle.updatetime = dataTable.Rows[i]["updatetime"].ToString();
                modle.state = Int32.Parse(dataTable.Rows[i]["state"].ToString());
                modle.msguserid = Int32.Parse(dataTable.Rows[i]["msguserid"].ToString());
                modle.msgisreadid = dataTable.Rows[i]["msgisreadid"].ToString();
                modle.isdeleted = Int32.Parse(dataTable.Rows[i]["isdeleted"].ToString());
                list.Add(modle);
            }
            return list;
        }

        private List<Schedule> DataTableScheduleToList(DataTable dataTable)
        {
            List<Schedule> list = new List<Schedule>();
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                Schedule modle = new Schedule();
                modle.id = new Guid(dataTable.Rows[i]["id"].ToString());
                modle.name = dataTable.Rows[i]["name"].ToString();
                modle.adduserid = Int32.Parse(dataTable.Rows[i]["adduserid"].ToString());
                modle.starttime = dataTable.Rows[i]["starttime"].ToString();
                modle.endtime = dataTable.Rows[i]["endtime"].ToString();
                modle.addtime = dataTable.Rows[i]["addtime"].ToString();
                modle.content = dataTable.Rows[i]["content"].ToString();
                modle.isallday = Int32.Parse(dataTable.Rows[i]["isallday"].ToString());
                modle.isdeleted = Int32.Parse(dataTable.Rows[i]["isdeleted"].ToString());
                list.Add(modle);
            }
            return list;
        }
        #endregion
        #region 安卓登录
        /// <summary>
        /// gcgl 安卓端的登录
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User getLoginUser(string username, string password)
        {
            string sql = "select * from public.sys_user where username = '" + username + "' and password = '" + password + "'";
            string sqlrole = "select array_to_string(array_agg(role_flag),',') rolename from (select* from sys_role where id in	" +
                "(select to_number(unnest(string_to_array(role_id, ',')),'99G999D9S')from sys_user " +
                "where username = '" + username + "' and password = '" + password + "')) rolename";
            User model = null;
            try
            {
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                DataTable dtrole = dbHelper.GetDataTable(CommandType.Text, sqlrole);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    model = new User();
                    model.id = Int32.Parse(dt.Rows[0]["id"].ToString());
                    model.name = dt.Rows[0]["name"].ToString();
                    model.department = dt.Rows[0]["department"].ToString();
                    model.telephone = dt.Rows[0]["telephone"].ToString();
                    model.role_id = dt.Rows[0]["role_id"].ToString();
                    model.password = dt.Rows[0]["password"].ToString();
                    model.username = dt.Rows[0]["username"].ToString();
                }
                if (dtrole!= null&& dtrole.Rows != null && dtrole.Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(dtrole.Rows[0]["rolename"].ToString()))
                    {
                        model.rolecode = dtrole.Rows[0]["rolename"].ToString();
                    }
                    
                }
            }
            catch (Exception e)
            {
            }
            return model;
        }
        /// <summary>
        /// 安卓端根据用户查询负责得的项目、工作、事件
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public List<PWE> GetLeaderPwebyUid(string uid,string state)
        {
            string sql = $"select * from gcgl_pwe where  isdeleted='0' and state='{state}' and leaderid='{uid}'";
            DataTable table = dbHelper.GetDataTable(CommandType.Text, sql);
            List<PWE> list = DataTablePweToList(table);
            return list.Count > 0 ? list : null;
        }
        #endregion

        #region 组织架构
        /// <summary>
        /// 根据部门获取部门下的所有人员
        /// </summary>
        /// <param name="depart"></param>
        /// <returns></returns>
        public List<User> GetUserByDepart(string depart)
        {
            string sql = $"select * from sys_user where department ='{depart}'";
            DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
            List<User> list=DataTableUserToList(dt);
            return list.Count>0?list:null;
        }
        
        /// <summary>
        /// 通过角色获取
        /// </summary>
        /// <param name="role"></param>
        /// <param name="department"></param>
        /// <returns></returns>
        public List<User> GetUserByOrg(string role="",string department="")
        {
            string sql = $"";
            List<User> list = new List<User>();
            if (!string.IsNullOrEmpty(department))
            {
                sql = $"select * from sys_user where department ='{department}'";
            }
            else
            {
                if (!string.IsNullOrEmpty(role))
                {
                    sql = $"select * from (select  to_number(unnest(string_to_array(role_id,',')),'99G999D9S') rolename, *  from  sys_user )a where rolename in " +
                    $"(select id from sys_role where role_flag = '{role}')";
                    if (!string.IsNullOrEmpty(department))
                    {
                        sql += $" or department='{department}'";
                    }
                }
            }
            if (!string.IsNullOrEmpty(sql))
            {
                DataTable dt = dbHelper.GetDataTable(CommandType.Text, sql);
                list = DataTableUserToList(dt);
            }
            
            return list.Count > 0 ? list : null;
        }
        /// <summary>
        /// 将 项目、工作、事件跟部门挂钩
        /// 新增将部门内的count+1 
        /// 跟新先将之前的-1 后+1
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="cid"></param>
        /// <param name="org"></param>
        /// <param name="state"></param>
        /// <param name="num"></param>
        /// <returns></returns>
        public int SaveOrgPwe(Guid? pid,Guid? cid,string org="",int state=0,int num=0)
        {
            int count = -1;
            string sql = "";
            string where = "count =count+1";
            if (num==1)
            {
                where = " count =count-1";
            }
            sql = $"delete from gcgl_org_pwe where count<=0";
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            sql = $" select count(*) from gcgl_org_pwe where cid='{cid}'and org='{org}'";
            count = dbHelper.GetCount(sql);
            if (count==0)
            {
                sql = $"insert into gcgl_org_pwe (id,pid,cid,org,count,state) values('{Guid.NewGuid()}','{pid}','{cid}','{org}','1','{state}')";
            }
            else
            {
                sql = $" update gcgl_org_pwe set {where} where id in (select id from gcgl_org_pwe where cid='{cid}' and org='{org}')";
            }
            count = dbHelper.ExecuteNonQuery(CommandType.Text, sql);
            if (state!=0)
            {
                if (pid != Guid.Empty)
                {
                    if (state == 1)
                    {
                        sql = $" select * from gcgl_org_pwe where cid='{pid}'and pid='{Guid.Empty}'";
                        DataTable tab = dbHelper.GetDataTable(CommandType.Text, sql);
                        if (tab != null && tab.Rows.Count > 0)
                        {
                            string wpid = tab.Rows[0]["pid"].ToString();
                           count= SaveOrgPwe(new Guid(wpid), pid, org, 0);
                        }
                    }
                    else if (state == 2)
                    {

                        sql = $" select * from gcgl_org_pwe where cid='{pid}'and pid!='{Guid.Empty}'";
                        DataTable tab = dbHelper.GetDataTable(CommandType.Text, sql);
                        if (tab != null && tab.Rows.Count > 0)
                        {
                            string wpid = tab.Rows[0]["pid"].ToString();
                           count= SaveOrgPwe(new Guid(wpid), pid, org, 1);
                        }
                    }
                }
                
            }
            return count;
        }

        //public List<string> GetPweidbyorg(string )
        #endregion
    }
}