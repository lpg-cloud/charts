
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Aipuer.Common
{
    public class GCGLModel
    {
    }
    /// <summary>
    /// 项目工作事件
    /// </summary>
    public class PWE
    {
        public PWE()
        {
            this.addtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.updatetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.progress = "0";
            this.joinperson = "";
            this.content = "";
            this.location = "";
            this.address = "";
            this.lable = "";
            this.upload = "";
            this.addusername = "";
            this.leadername = "";
            this.leadername = "";
            this.workname = "";
            this.joinname = "";
            this.tempname = "";
            this.templeader = "";
            this.nextid = "";
            this.extracontent = "";
            this.extraname = "";
            this.projectname = "";
        }
        #region 数据库字段
        
        /// <summary>
        /// id
        /// </summary>
        public Guid id { get; set; }
        /// <summary>
        /// 所属父id
        /// </summary>
        public Guid pid { get; set; }
        /// <summary>
        /// 添加人
        /// </summary>
        public int adduserid { get; set; }
        /// <summary>
        /// 负责人id
        /// </summary>
        public int leaderid { get; set; }
        /// <summary>
        /// 参与人员
        /// </summary>
        public string joinperson { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 定位坐标
        /// </summary>
        public string location { get; set; }
        /// <summary>
        /// 定位地址
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string lable { get; set; }
        /// <summary>
        /// 进度
        /// </summary>
        public string progress { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string starttime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string endtime { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string addtime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime { get; set; }
        /// <summary>
        /// 附件路径
        /// </summary>
        public string upload { get; set; }
        /// <summary>
        /// 事件是否完成 flag 0 否 1 是
        /// </summary>
        public string flag { get; set; }
        /// <summary>
        /// 评论条数
        /// </summary>
        public string commentcount { get; set; }
        /// <summary>
        /// 工作 事件 排序
        /// </summary>
        public int serialnumber { get; set; }
        /// <summary>
        /// 是否删除 0 否 1 是
        /// </summary>
        public string isdeleted { get; set; }
        /// <summary>
        /// 类型 0 项目 1 工作 2 事件
        /// </summary>
        public int state { get; set; }
        #endregion
        #region 额外添加
        /// <summary>
        /// 添加人姓名
        /// </summary>
        public string addusername { get; set; }
        /// <summary>
        /// 领导人姓名
        /// </summary>
        public string leadername { get; set; }
        /// <summary>
        /// 项目名称
        /// </summary>
        public string projectname { get; set; }
        /// <summary>
        /// 工作名称
        /// </summary>
        public string workname { get; set; }
        /// <summary>
        /// 参与人员姓名
        /// </summary>
        public string joinname { get; set; }
        //public List<PWE> work { get; set; }
        //public List<PWE> eventlist{ get;set; }
        /// <summary>
        /// 阶段名称
        /// </summary>
        public string tempname { get; set; }
        /// <summary>
        /// 阶段负责人
        /// </summary>
        public string templeader { get; set; }
        /// <summary>
        /// 插入 插入的下一个阶段的id
        /// </summary>
        public string nextid { get; set; }
        /// <summary>
        /// 自定义字段
        /// </summary>
        public string extraname { get; set; }
        /// <summary>
        /// 自定义内容
        /// </summary>
        public string extracontent { get; set; }
        /// <summary>
        /// 项目负责人id
        /// </summary>
        public int  pleaderid { get; set; }
        //工作负责人id
        public int wleaderid { get; set; }
        #endregion

    }
    /// <summary>
    /// 自定义属性
    /// </summary>
    public class projectotherfield
    {
        public projectotherfield() { }
        #region 数据库字段
        public Guid id { get; set; }
        public Guid pid { get; set; }
        public string name { get; set; }
        public string content { get; set; }
        public int flag { get; set; }
        public int isdeleted { get; set; }
        #endregion
    }
    /// <summary>
    /// 评论
    /// </summary>
    public class comment
    {
        public comment() {
            this.addtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
        #region 数据库字段
        public Guid id { get; set; }
        /// <summary>
        /// 评论所属的事件
        /// </summary>
        public Guid pid { get; set; }
        /// <summary>
        /// 评论人
        /// </summary>
        public int adduserid { get; set; }
        /// <summary>
        /// 评论内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 评论时间
        /// </summary>
        public string addtime { get; set; }
        /// <summary>
        /// 是否删除 0 否   1是
        /// </summary>
        public Boolean isdeleted { get; set; }
        #endregion
        #region 额外字段
        /// <summary>
        /// 添加人姓名
        /// </summary>
        public string addusername { get; set; }
        #endregion
        
    }
    /// <summary>
    /// 邮件
    /// </summary>
    public class Document
    {
        public Document() {
            this.addtime= DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.updatetime= DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.receiveperson = "";
            this.isread = 0;
            this.addusername = "";
            this.recename = "";
        }
        #region 数据库字段
        public Guid id { get; set; }
        /// <summary>
        /// 邮件添加人
        /// </summary>
        public int adduserid { get; set; }
        /// <summary>
        /// 邮件名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 邮件内容

        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 附件上传路径
        /// </summary>
        public string upload { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string addtime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime { get; set; }
        /// <summary>
        /// 是否发送 0否 1是
        /// </summary>
        public Boolean issend { get; set; }
        /// <summary>
        /// 是否删除 0否 1是
        /// </summary>
        public int isdeleted { get; set; }
        #endregion
        #region 额外字段
        /// <summary>
        /// 邮件接受人id
        /// </summary>
        public string receiveperson { get; set; }
        /// <summary>
        /// 添加人
        /// </summary>
        public string addusername { get; set; }
        /// <summary>
        /// 是否已读未读
        /// </summary>
        public int isread { get; set; }
        /// <summary>
        /// 接收人姓名
        /// </summary>
        public string recename { get; set; }
       
        #endregion
    }
    /// <summary>
    /// 接受发送表
    /// </summary>
    public class send_receive
    {
        public send_receive() {
            this.sendtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.isread = 0;
            this.recename = "";
        }
        #region 数据库字段
        
        public Guid id { get; set; }
        /// <summary>
        /// 邮件id
        /// </summary>
        public Guid pid { get; set; }
        /// <summary>
        /// 发送人 id
        /// </summary>
        public int sendperson { get; set; }
        /// <summary>
        /// 接受人id
        /// </summary>
        public int receiveperson { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public string sendtime { get; set; }
        /// <summary>
        /// 是否已读接受  0 否 1是
        /// </summary>
        public int isread { get; set; }
        /// <summary>
        /// 是否转发的 0 否 1是
        /// </summary>
        public int isrepost { get; set; }
        /// <summary>
        /// 是否删除 0 否 1是
        /// </summary>
        public int isdeleted { get; set; }

        #endregion
        #region 额外字段
        public string recename { get; set; }
        #endregion
    }
    /// <summary>
    /// 消息
    /// </summary>
    public class message
    {
        public message()
        {
            this.sendtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.readtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.isread = 0;
        }
        #region 数据字段
        
        public Guid id { get; set; }
        /// <summary>
        ///  所属父id==pid
        /// </summary>
        public Guid mid { get; set; }
        /// <summary>
        /// Pid.Name
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 负责人id 发送消息中 有负责人则添加无则是空
        /// </summary>
        public int leaderid { get; set; }
        /// <summary>
        /// 项目—项目下工作负责人 
        /// 工作-工作参与人 
        /// 事件—事件参与人  
        /// 若修改人员在参与中或负责中不给该人发送消息
        /// </summary>
        public string assignid { get; set; }
        /// <summary>
        /// 发送时间
        /// </summary>
        public string sendtime { get; set; }
        /// <summary>
        /// 查看消息的时间
        /// </summary>
        public string readtime { get; set; }
        /// <summary>
        /// 类型标志2 0表示新增消息推送 1表示编辑消息推送
        /// </summary>
        public int edittype { get; set; }
        /// <summary>
        ///  0项目 1工作 2事件 3邮件 4新闻公告
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 是否删除 bit 0未删除 1 删除
        /// </summary>
        public int isdeleted { get; set; }
        /// <summary>
        /// 是否已读
        /// </summary>
        public int isread { get; set; }
        #endregion
        #region 额外字段
        public string partA { get; set; }
        public string partC { get; set; }
        #endregion
    }
    /// <summary>
    /// 消息反馈表
    /// </summary>
    public class messagefeedback
    {
        public messagefeedback()
        {
            this.addtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.updatetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.recename = new List<string>();
        }
        #region 数据库字段
        
        public Guid id { get; set; }
        /// <summary>
        /// 通知消息所属事件Id
        /// </summary>
        public Guid mid { get; set; }
        /// <summary>
        /// 消息更新人的Id 添加人
        /// </summary>
        public int msguserid { get; set; }
        /// <summary>
        /// 消息名称 更新消息的姓名
        /// </summary>
        public string mname { get; set; }
        /// <summary>
        /// 消息接受人的id
        /// </summary>
        public string msgreceid { get; set; }
        /// <summary>
        /// 已读消息的人
        /// </summary>
        public string msgisreadid { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string addtime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime { get; set; }
        /// <summary>
        /// 0 项目 1 工作 2 事件 3 公文 4 新闻公告
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 是否删除 bit 0未删除 1 删除
        /// </summary>
        public int isdeleted { get; set; }

        #endregion
        #region 额外字段
        public List<string> recename { get; set; }
        #endregion
    }
    /// <summary>
    /// 新闻
    /// </summary>
    public class news
    {
        public news() {
            this.addtime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.updatetime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }
        #region 数据库字段
        
        public Guid id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 简介
        /// </summary>
        public string brief { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string addtime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public string updatetime { get; set; }
        /// <summary>
        /// Flag=0;新闻 /// Flag=1;公告
        /// </summary>
        public int flag { get; set; }
        /// <summary>
        ///  是否删除 0 是 1否
        /// </summary>
        public int isdeleted { get; set; }

        #endregion

    }
    /// <summary>
    /// 日程
    /// </summary>
    public class Schedule
    {
        public Schedule() {
            this.itemname = "";
        }
        #region 数据库字段
        
        public Guid id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string content { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string starttime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string endtime { get; set; }
        /// <summary>
        /// 添加时间
        /// </summary>
        public string addtime { get; set; }
        /// <summary>
        /// 是否全天 1 是 0否
        /// </summary>
        public int isallday { get; set; }
        /// <summary>
        /// 是否完成 0否 1完成
        /// </summary>
        public int isfinish { get; set; }
        /// <summary>
        /// 是否删除 0否 1 删除
        /// </summary>
        public int isdeleted { get; set; }
        /// <summary>
        /// 添加人员
        /// </summary>
        public int adduserid { get; set; }

        #endregion
        #region 额外字段

        ///////////////////////////////////////////////
        ///没有使用字段
        /// <summary>
        /// 与事件相关的id
        /// </summary>
        public Guid itemid { get; set; }
        /// <summary>
        /// 事件名称
        /// </summary>
        public string itemname { get; set; }
        /// <summary>
        /// 事件类型  0 项目 1 工作 2 事件? 
        /// itemtype 改itemstate 0528
        /// </summary>
        public int itemstate { get; set; }
        /// <summary>
        ///  0 项目 1 工作 2 事件????
        /// </summary>
        public int flag { get; set; }
        #endregion
    }
    /// <summary>
    /// 模板
    /// </summary>
    public class templet
    {
        public templet() { }
        #region 数据库字段
        public Guid id { get; set; }
        /// <summary>
        /// 模板名称id
        /// </summary>
        public Guid pid { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int sort { get; set; }
        /// <summary>
        /// 项目 0 工作1 
        /// </summary>
        public int flag { get; set; }
        /// <summary>
        /// 是否删除 0 否 1是
        /// </summary>
        public int isdeleted { get; set; }
        #endregion
        #region 额外添加
        /// <summary>
        /// 子节点
        /// </summary>
       public List<templet> childs { get; set; }
        /// <summary>
        /// 总共有几个子节点
        /// </summary>
        public int counts { get; set; }
        #endregion
    }
    /// <summary>
    /// 组织架构
    /// </summary>
    public class organization_pwe
    {
        public organization_pwe() { }
        #region 数据库字段
        
        public Guid id { get; set; }
        /// <summary>
        /// 当前项目、工作、事件 父id
        /// </summary>
        public Guid cid { get; set; }
        /// <summary>
        /// 当前项目、工作、事件 id
        /// </summary>
        public Guid pid { get; set; }
        /// <summary>
        /// 所属部门
        /// </summary>
        public Guid org { get; set; }
        /// <summary>
        /// 数（根据参与人、领导人，有一个参与项目，count+1,
        /// count==0代表该项目没有该部门没有人参与改项目，
        /// 则删除该数据，若该项目删除时也要删除与该项目相关的PWE数据）
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 是否删除 0 否 1是
        /// </summary>
        public int isdeleted { get; set; }

        #endregion
    }
    /// <summary>
    /// 极光推送
    /// </summary>
    public class registouser
    {
        public registouser() { }
        #region 
        /// <summary>
        /// 用户id
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 极光推送
        /// </summary>
        public string registrationid { get; set; }
        /// <summary>
        /// 部门id
        /// </summary>
        public string groupid { get; set; }
        /// <summary>
        /// 角色id
        /// </summary>
        public string roleid { get; set; }
        #endregion
    }

    #region config

    public class UserConfig
    {
        public UserConfig()
        {
            this.General_Manager = ConfigurationManager.AppSettings["General_Manager"];//局长
            this.Vice_General_Manager = ConfigurationManager.AppSettings["Vice_General_Manager"];
            this.Division_Manager = ConfigurationManager.AppSettings["Division_Manager"];///科长
            this.Staff = ConfigurationManager.AppSettings["Staff"];///科员
        }
        /// <summary>
        /// 局长
        /// </summary>
        public string General_Manager
        {
            get;set;
        }
        /// <summary>
        /// 副局长
        /// </summary>
        public  string Vice_General_Manager { get; set; }
        /// <summary>
        /// 科长
        /// </summary>
        public string Division_Manager { get; set; }
        /// <summary>
        /// 科员
        /// </summary>
        public string Staff { get; set; }
    }
    #endregion
}