using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using Common.Logging;
using Quartz;
using Quartz.Impl;

namespace WindowsServiceTest
{


    public partial class ServiceTest : ServiceBase
    {

        #region 字段 变量
        private readonly ILog logger;
        GISConvert cj = new GISConvert();
        #endregion
        public ServiceTest()
        {
            InitializeComponent();
            logger = LogManager.GetLogger(GetType());
            
        }

        protected override void OnStart(string[] args)
        {
            logger.Info("服务开始...");
            cj.UpdateGISXY();
        }

        protected override void OnStop()
        {
            logger.Info("服务结束...");
        }

        protected override void OnPause()
        {
            logger.Info("暂停调度中所有的job任务...");
        }

        protected override void OnContinue()
        {
            logger.Info("恢复调度中所有的job的任务...");
        }
    }
}
