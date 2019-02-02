using System;
using System.ServiceProcess;
using System.Threading;
using NLog;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace RdpAttackNotificator.Service
{
    public partial class Service : ServiceBase
    {
        public Service()
        {
            InitializeComponent();

            this.Logger = LogManager.GetLogger("Service");
            this.Logger.Info("Initializing service.");
            this.Logger.Info($"OS version is {Environment.OSVersion}.");
            this._timer = new Timer(this.Callback);
        }

        private void Callback(object state)
        {
            new RdpAccessHandler().Process();
        }

        public IDisposable HostingProcess { get; private set; }

        private Timer _timer;

        public Logger Logger { get; }

        protected override void OnStart(string[] args)
        {
            try
            {
                if (TimeSpan.TryParse(ConfigurationManager.AppSettings["RefreshPeriod"], out TimeSpan refreshPeriod))
                {
                    this.Logger.Info($"Starting service with refresh period {refreshPeriod}.");
                    this._timer.Change(TimeSpan.Zero, refreshPeriod);
                }
                else
                {
                    this.Logger.Info("Starting service with no refresh period.");
                }

                this.Logger.Info("Service started.");
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
            }
        }

        protected override void OnStop()
        {
            try
            {
                this.Logger.Info("Stopping service.");
                this._timer.Change(TimeSpan.Zero, TimeSpan.Zero);
                this.Logger.Info("Service stopped.");
            }
            catch (Exception exception)
            {
                this.Logger.Error(exception);
            }
        }
    }
}
