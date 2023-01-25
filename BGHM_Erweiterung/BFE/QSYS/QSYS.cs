using System;
using Newtonsoft.Json.Linq;
using System.Timers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Crestron.SimplSharp;

namespace BFE.QSYS
{
    class Qsys
    {
        string _ip;
        int _port;

        string _username;
        string _password;
        private bool status;
        private TcpClient tcp;
        Logon _logonObject = new Logon();


        NetworkStream _tcpStream = null;

        Timer _keepAliveTimer;
        public Qsys(string ip, int port, string username, string password)
        {
            _ip=ip;
            _port=port;
            _username=username;
            _password=password;

            _logonObject.Username = _username;
            _logonObject.Password = _password;

            tcp = new TcpClient(ip, port);

            _tcpStream = tcp.GetStream();



            _keepAliveTimer = new System.Timers.Timer();
            _keepAliveTimer.Interval = 19000;
            _keepAliveTimer.AutoReset = true;
            _keepAliveTimer.Elapsed += KeepAlive;

            _keepAliveTimer.Start();


        }

        private void KeepAlive(object sender, ElapsedEventArgs args)
        {

            NoOp c = new NoOp();
            //IRpcResponse response = 
                Send(c);

        }

        public void Logon()
        {

            //IRpcResponse response = 
                Send(_logonObject);
            
            Rpc.ReadResponseObject(_tcpStream);
            
            //ReadResponseIRCP(_tcp_Stream);
        }

        public IRpcResponse Get(string namedControl) //TODO change return Type
        {

            ControlGet c = new ControlGet();

            c.AddString(namedControl);
            return Send(c);
                
        }

        public IRpcResponse SetValue(string namedControl, double value) //TODO change return Type
        {

            ControlSetDoubleValue c = new ControlSetDoubleValue
            {
                Name = namedControl,
                Value = value
            };
            return Send(c);
        }

        public IRpcResponse SetValue(string namedControl, bool value) //TODO change return Type
        {

            ControlSetBoolValue c = new ControlSetBoolValue
            {
                Name = namedControl,
                Value = value
            };
            return Send(c);
        }

        public IRpcResponse SetPosition(string namedControl, double pos) //TODO change return Type
        {

            ControlSetPosition c = new ControlSetPosition();

            c.Name = namedControl;
            c.Position = pos;
            return Send(c);
        }


        public IRpcResponse Send(IRpcCommand c)
        {
            status = tcp.Connected;
            
            if (status)
            {   
                Rpc.Send(_tcpStream, c);
            }
            

            return ReadResponseIrcp(_tcpStream);
        }


        private IRpcResponse ReadResponseIrcp(NetworkStream stream)
        {
            try
            {
                JObject obj = Rpc.ReadResponseObject(stream);
                if (obj.GetValue("result").Type == JTokenType.Boolean)
                {
                    ResponseGetBool c = new ResponseGetBool();
                    c.NamedControl = (string)obj.GetValue("Name");
                    c.Value = (bool)obj.GetValue("Value");
                    
                    CrestronConsole.PrintLine("ID: {0}",obj.GetValue("id"));
                }

                if(obj.GetValue("result").Type == JTokenType.Array)
                {
                    CrestronConsole.PrintLine("Hello");
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error(e.ToString());
                throw;
            }
            
            CrestronConsole.PrintLine("lOgoN 4" );

            return new ResponseGetBool();
        }

    }




    public interface IRpcCommand
    {
        string Method { get; }
    }

    public interface IRpcResponse
    {

    }

    class ResponseGetDouble : IRpcResponse
    {
        public string NamedControl { get; set; }
        public double Value { get; set; }
        public string @String { get; set; }
        public double Position { get; set; }
    }

    class ResponseGetBool : IRpcResponse
    {
        public string NamedControl { get; set; }
        public bool Value { get; set; }
    }

    // ####################
    // RPC COMMANDS
    // ####################

    class ControlSetDoubleValue : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "Control.Set";
            }
        }



        public string Name { get; set; }

        public double Value { get; set; }

    }

    class ControlSetBoolValue : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "Control.Set";
            }
        }



        public string Name { get; set; }

        public bool Value { get; set; }

    }

    class ControlSetPosition : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "Control.Set";
            }
        }

        public string Name { get; set; }

        public double Position { get; set; }

    }

    class ControlGet : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "Control.Get";
            }
        }

        public string[] Params { get; set; }

        public ControlGet()
        {
            Params = new string[1];
        }

        public void AddString(string x)
        {
            Params[0] = x;
        }

    }

    class Logon : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "Logon";
            }
        }

        public string Username { get; set; }
        public string Password { get; set; }

    }

    /// <summary>
    /// Send Command to tell core to send status changes back
    /// </summary>
    class WatchEnable : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "PA.ZoneStatusConfigure";
            }
        }

        public bool Enabled { get; set; }
        public override string ToString()
        {
            return base.ToString();
        }

    }

    /// <summary>
    /// Send Command to queue a new page
    /// </summary>
    class PageSubmit : IRpcCommand
    {
        public string Mode { get; set; }
        public string Originator { get; set; }
        public string Description { get; set; }
        public int[] Zones { get; set; }
        public string[] ZoneTags { get; set; }
        public int Priority { get; set; }
        public string Preamble { get; set; }
        public string Message { get; set; }
        public bool Start { get; set; }
        public int Station { get; set; }

        public string Method
        {
            get
            {
                return "PA.PageSubmit";
            }
        }

        public override string ToString()
        {
            return string.Format("Request to play {0}", Message);
        }

    }

    /// <summary>
    /// Send Command to ping the core
    /// </summary>
    class NoOp : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "NoOp";
            }
        }

        public override string ToString()
        {
            return "PING";
        }

    }

    class ChangeGroupAddControl : IRpcCommand
    {

        public string Method
        {
            get
            {
                return "ChangeGroup.AddControl";
            }
        }

        // the name of this change group
        public string Id { get; set; }
        public string[] Controls { get; set; }


    }


    class ChangeGroupAutoPoll : IRpcCommand
    {
        public string Method
        {
            get
            {
                return "ChangeGroup.AutoPoll";
            }
        }

        // the name of this change group
        public string Id { get; set; }
        public int Rate { get; set; }

    }

    // ####################
    // RPC RESPONSES
    // ####################

    public class ZoneStatus : IRpcResponse
    {
        public ZoneStatus(JObject o)
        {
            Zone = o.GetValue("params").Value<int>("Zone");
            Active = o.GetValue("params").Value<bool>("Active");
        }

        public DateTime Time { get; set; }
        public int Zone { get; set; }
        public bool Active { get; set; }

        public override string ToString()
        {
            string isActive = Active ? "" : "Not";
            return string.Format("Zone {0} is {1} Active", Zone, isActive);
        }
    }

    public class QsysResponse : IRpcResponse
    {
        public string Name { get; }
        public string @String { get; }

        public QsysResponse(JObject response)
        {
            if (response.ContainsKey("Name")) { Name = response.GetValue("params").Value<String>("Name"); }
            if (response.ContainsKey("String")) { @String = response.GetValue("params").Value<String>("String"); }

            Console.WriteLine(Name);
            Console.WriteLine(String);

        }


    }

    public class QsysGainResponse : QsysResponse
    {
        public double Value { get; }
        public double Pos { get; }
        public QsysGainResponse(JObject response) : base(response)
        {

            if (response.ContainsKey("Value")) { Value = (double)response.GetValue("params").Value<double>("Value"); }
            if (response.ContainsKey("Position")) { Pos = (double)response.GetValue("params").Value<double>("Position"); }
            Console.WriteLine(Value);
            Console.WriteLine(Pos);

        }
    }
}

