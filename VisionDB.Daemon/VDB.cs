using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using VisionDB.Controllers;
using VisionDB.Models;

namespace VisionDB.Daemon
{
    public partial class VDB : ServiceBase
    {
        bool On = true;
        public VDB()
        {
            InitializeComponent();
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Daemon, "Daemon started", null, false);

            Process(); 
        }

        async private void Process() 
        {
            MessagesController messagesController = new MessagesController();
            RecallsController recallsController = new RecallsController();

            while (On)
            {
                recallsController.ProcessRecalls();
                messagesController.ProcessMessages();
                BackupDatabaseIfDue();

                await System.Threading.Tasks.Task.Delay(60000);
            }
        }

        private void BackupDatabaseIfDue()
        {
            try
            {
                CustomersDataContext db = new CustomersDataContext();
                Setting setting = db.Settings.Find(1);
                TimeSpan dailyDatabaseBackupTime = new TimeSpan(setting.DailyDatabaseBackupTime.Hour, setting.DailyDatabaseBackupTime.Minute, setting.DailyDatabaseBackupTime.Second);

                if (setting.DailyBackupLastTaken == null || (setting.DailyBackupLastTaken.Value.Date < DateTime.Now.Date && setting.DailyBackupLastTaken.Value.Date.Add(dailyDatabaseBackupTime) < DateTime.Now))
                {
                    db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, "exec sp_VDB_Backup_Database");
                    VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Database_Backup, "Backup successful", null, false);
                    setting.DailyBackupLastTaken = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Daemon, string.Concat("Error saving database backup", Environment.NewLine, ex.Message), null, false);
            }
        }

        protected override void OnStop()
        {
            VisionDBController.AddAuditLogEntry("Daemon", Enums.AuditLogEntryType.Daemon, "Daemon stopped", null, false);
        }
    }
}
