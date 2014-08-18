using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Moq;
using FluentAssertions;
using Xbehave;
using Xunit;
using Xunit.Extensions;

using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Channels;

using MVPSI.JAMS.Host;
using MVPSI.JAMSServer;
using MVPSI.JAMSMF;

namespace JAMSMFXunit
{
    public class MFJobHostScenarios
    {
        static MFJobHost _mfJobHost = new MFJobHost();

        static Mock<IServiceProvider> _serviceProvider = new Mock<IServiceProvider>(); 
        static string _jobSource;
        //Todo: create dummy attributes and parameters
        static Dictionary<string, object> _jobAttributes;//  These are read in the JAMSHost Program via named pipe 
        static Dictionary<string, object> _jobParameters;//  from (RoutineJob.PacketOutboundConnectedCallback) in 
                                                         //  either the Local or Agent Executor(sends data via socket to Local Executor)
                                                         //  note: The pipe name is derived from the EntryInfo.JAMSId guid
                                                         //  note: The Password and PrivateKey are 'hidden'... CryptKey is in BatchJobBase

        static MFJobHostScenarios()
        {            
            // mock the service provider i.e. _serviceProvider.RegisterService(...
            // ex. var m = _serviceProvider.Object.GetService(typeof(TraceSource));
            _serviceProvider
                .Setup(sp => sp.GetService(typeof(TraceSource)))
                .Returns(new TraceSource("Testing"));//  Common.TS

            _serviceProvider
                .Setup(sp => sp.GetService(typeof(ConsoleTraceListener)))
                .Returns(new ConsoleTraceListener());//  ctl

            _serviceProvider
                .Setup(sp => sp.GetService(typeof(ReturnObjectService)))
                .Returns(new ReturnObjectService(new BinaryFormatter(), System.IO.Stream.Null));//  ra

            _serviceProvider
                .Setup(sp => sp.GetService(typeof(MVPSI.JAMS.Host.IPersistenceService)))
                .Returns(null);    

            //Todo: set the job source
            
            //Todo: set the job attributes and parameters
        }


        [Scenario, ClassData(typeof(StartUp_InitializeData))]
        public virtual void MFJobHost_Startup_Initialize(string address, Binding binding)
        {
            _mfJobHost.MFMonitorUri = address;
            _mfJobHost.MFMonitorBinding = binding;

            _mfJobHost.Initialize(_serviceProvider.Object, _jobSource);
        }

        [Scenario]
        public virtual void MFJobHost_Startup_Prestart()
        {
            _mfJobHost.PreStart(_serviceProvider.Object, _jobParameters);
        }


        [Scenario]
        public virtual void MFJobHost_Initialize()
        {
            _mfJobHost.Initialize(_serviceProvider.Object, _jobAttributes, _jobParameters);
        }


        [Scenario]
        public virtual void MFJobHost_Execute()
        {
            _mfJobHost.Execute(_serviceProvider.Object, _jobAttributes, _jobParameters);
        }


        [Scenario]
        public virtual void MFJobHost_Cancel()
        {
            _mfJobHost.Cancel(_serviceProvider.Object);
        }


        [Scenario]
        public virtual void MFJobHost_Cleanup()
        {
            _mfJobHost.Cleanup(_serviceProvider.Object, _jobAttributes, _jobParameters);
        }



        EntryInfo _entryInfo;
        [Scenario]
        public virtual void DoSomething()
        {
            "".Given(() =>
            {
                _entryInfo = new MVPSI.JAMSServer.EntryInfo();               
            });

            "".When(() =>
            {
                Console.WriteLine("act");
            });

            "".Then(() =>
            {
                Console.WriteLine("assert");
                
                int x = 0;
                x.Should().Be(1);

                _entryInfo.Should().NotBeNull("because we inititialized it");
            });
        }

        
    }


    /// <summary>
    /// Supplies multiple sets of data parameters for MFJobHost_StartUp_Initialize the test
    /// </summary>
    public class StartUp_InitializeData : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[] { 
                "https://microfocus:9186/IMonitor", 
                new WSHttpBinding() { Security = new WSHttpSecurity() { Mode = SecurityMode.Transport } } 
            },
            new object[] { 
                "tcp://localhost:9186/IMonitor", 
                new NetTcpBinding() 
            }
        };

        public IEnumerator<object[]> GetEnumerator()
        { return _data.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }

    }

}
