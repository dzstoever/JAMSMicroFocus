using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using MVPSI.JAMS.Host;
//note: Microfocus.SEE.Proxies.dll has most of it's classes in an empty namespace 
using MicroFocus.SEE.Dispatch;
using MicroFocus.SEE.SEP.Service;

namespace MVPSI.JAMSMF
{
    public class MFJobHost : IJobStartup, IJAMSHost //, IJAMSHostReports,
    {
        private TraceSource m_TraceSource;
        private bool m_Debug;

        /// <summary>
        /// Monitor client proxy to interact with the Micro Focus seemonitor service to control/monitor jobs
        /// </summary>
        private IMonitor m_MFMonitor; 
        public string MFMonitorUri { get; set; } 
        public Binding MFMonitorBinding { get; set; } //  Todo: make the binding? and endpoint configurable
        
        
        //  MF logon parms - todo: add a config file
        private long m_MFTokenVal;
        private string m_MFDomain;
        private string m_MFUsername;
        private string m_MFPassword;
        private string m_MFNewPassword;
        private int m_MFLogonType;
        private int m_MFLogonProvider;
        private bool m_MFSendToken;
        private SEPStatus m_MFSepStatus;
        private string m_MFRequestor;
        //  MF job/region parms
        private int m_MFJobNo = 0;
        private RegionLocation m_MFRegion;
        private string m_MFRegionName;
        private string m_MFDbInstance;
        private string m_MFDispatcherUrl;
        private ExtensionDataObject m_MFExtensionData;
        private int m_MFSpoolId;
        

        private string m_JAMSJobName;
        private int m_JAMSEntry;


        #region IJobStartup Members
        /// The methods of the IJobStartup interface are called by the JAMSScheduler service when it is starting a job.
        /// If a method throws an exception, the job will fail with that exception.

        public object Initialize(IServiceProvider serviceProvider, string jobSource)
        {
            //
            //  Get the Execute TraceSource
            //
            m_TraceSource = serviceProvider.GetService(typeof(TraceSource)) as TraceSource;

            //
            // Create the client
            //
            m_MFMonitor = new MonitorClient(MFMonitorBinding, new EndpointAddress(MFMonitorUri));
            
            //
            //  If we can't ping we are dead in the water
            //
            m_MFMonitor.Ping();//  job should fail?

            //byte[] credentials = new byte[128];
            //bool resultPC = m_MFMonitor.PresentCredentials(m_MFUsername, credentials);
            //bool resultRC = m_MFMonitor.RequestCredentials(m_MFUsername, m_MFRequestor, m_MFDbInstance);
            //long resultU = m_MFMonitor.RequestUser(out m_MFDomain, m_MFUsername, m_MFSepStatus);

            //
            //  If we can't logon we are dead in the water
            //
            int resultL = m_MFMonitor.Logon(out m_MFTokenVal,
                ref m_MFDomain, m_MFUsername, m_MFPassword, m_MFNewPassword,
                m_MFLogonType, m_MFLogonProvider, m_MFSendToken, m_MFSepStatus);

            object m_MFJobSourceJCL = jobSource as object;//  not sure if we need change something here...

            return m_MFJobSourceJCL;//  should be the XM object when parsing the job source?
        }


        public string RequiresSecondaryUser(IServiceProvider serviceProvider)
        {
            m_TraceSource.TraceInformation("MFJobHost returning UserSecurity of >{0}<", m_MFUsername);
            return m_MFUsername;//  does we need this

            //return null;//  does not require a secondary UserSecurity object.
        }


        public void PreStart(IServiceProvider serviceProvider, 
            Dictionary<string, object> parameters)
        {
            return;            
        }

        #endregion


        #region IJAMSHost Members

        public void Initialize(IServiceProvider serviceProvider, 
            Dictionary<string, object> attributes, 
            Dictionary<string, object> parameters) 
        {
            //
            //  Initialize our TraceSource
            //
            m_TraceSource = (TraceSource)serviceProvider.GetService(typeof(TraceSource));
            m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Initializing MFJobHost host");

            m_Debug = attributes.ContainsKey("Debug") ? (bool)attributes["Debug"] : false;

            //
            //  Tell JAMSHost that we need the password and/or private key
            //
            if (attributes.ContainsKey("DecryptPasswords"))
            {
                attributes["DecryptPasswords"] = true;
            }
            else
            {
                attributes.Add("DecryptPasswords", true);
            }

    
        }


        public FinalResults Execute(IServiceProvider serviceProvider, 
            Dictionary<string, object> attributes, 
            Dictionary<string, object> parameters)
        {
            FinalResults finalResults = new FinalResults();

            m_TraceSource.TraceEvent(TraceEventType.Information, 0, "Executing MFJobHost");


            //Todo: get the data we need to run the job

            m_MFRegion = new RegionLocation()
            {
                RegionName = "",
                DbInstance = "",
                DispatcherUrl = "",
                ExtensionData = null
            };



            m_MFMonitor.RunJob(m_MFRegion, m_MFSpoolId);
            
            
            //Todo: get the job results from MF and populate finalResults

            return finalResults;
        }


        public void Cancel(IServiceProvider serviceProvider)
        {
            m_MFMonitor.CancelJob(m_MFRegion, m_MFJobNo);
        }


        public void Cleanup(IServiceProvider serviceProvider, 
            Dictionary<string, object> attributes, 
            Dictionary<string, object> parameters)
        {
            m_MFMonitor = null;
        }

        #endregion

    }
}
