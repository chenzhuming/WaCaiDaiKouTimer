using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Tellyes.Common;
using log4net;
using System.Reflection;
using System.Configuration;
using System.IO;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace WindowsTimeService
{
   
    public partial class Service1 : ServiceBase
    {
        ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        System.Timers.Timer timer1;  //计时器
        private Paramlist paramList=null;
        public Service1()
        {
            
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            read();
            timer1 = new System.Timers.Timer();
            timer1.Interval = 1000;  //设置计时器事件间隔执行时间
            timer1.Elapsed += new System.Timers.ElapsedEventHandler(timer1_Elapsed);
            timer1.Enabled = true;
        }

        protected override void OnStop()
        {
            this.timer1.Enabled = false;
        }
        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                int intHour = e.SignalTime.Hour;
                int intMinute = e.SignalTime.Minute;
                int intSecond = e.SignalTime.Second;
                string result = "";
                if (paramList != null)
                {
                    foreach (var item in paramList.DataSource)
                    {
                        Param p = item as Param;
                        if (intHour == int.Parse(p.Hour) && intMinute == int.Parse(p.Minute) && intSecond == 0)
                        {
                            InterfaceHelper.RequestService(p.Url, "", ref result);
                            log.Info("调用: " + p.Url);
                            log.Info("返回:" + result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return;
            }
           

        }
        private void read()
        {
            try
            {
                string configPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\timer.xml";
                if (!File.Exists(configPath))
                {
                    log.Error("配置文件丢失，程序无法运行！");
                    return;
                }
                FileStream fs = File.Open(configPath, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                string text = sr.ReadToEnd();
                var list = XmlSerializeHelper.DeSerialize<Paramlist>(text);
                paramList = list as Paramlist;
                sr.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return;
            }
           
        }
            
    }
}
