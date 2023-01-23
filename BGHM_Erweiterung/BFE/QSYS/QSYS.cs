using System;
using Newtonsoft.Json.Linq;
using System.Timers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Crestron.SimplSharp;

namespace BFE.QSYS
{
    class QSYS
    {
        string IP;
        int Port;

        string Username;
        string Password;

        LOGON LogonObject = new LOGON();


        NetworkStream _tcp_Stream = null;

        Timer keepAliveTimer;
        public QSYS(string ip, int port, string username, string password)
        {
            IP=ip;
            Port=port;
            Username=username;
            Password=password;

            LogonObject.Username = Username;
            LogonObject.Password = Password;

            TcpClient tcp = new TcpClient(ip, port);

            _tcp_Stream = tcp.GetStream();



            keepAliveTimer = new System.Timers.Timer();
            keepAliveTimer.Interval = 19000;
            keepAliveTimer.AutoReset = true;
            keepAliveTimer.Elapsed += keepAlive;

            keepAliveTimer.Start();


        }

        private void keepAlive(object Sender, ElapsedEventArgs args)
        {

            NoOp C = new NoOp();
            IRPCResponse response = Send(C);

        }

        public IRPCResponse LOGON()
        {
            var response = Send(LogonObject);

            //ReadResponseIRCP(_tcp_Stream);

            return null;

        }

        public IRPCResponse Get(string namedControl) //TODO change return Type
        {

            ControlGet C = new ControlGet();

            C.addString(namedControl);
            return Send(C);
        }

        public IRPCResponse SetValue(string namedControl, double Value) //TODO change return Type
        {

            ControlSetDoubleValue C = new ControlSetDoubleValue
            {
                Name = namedControl,
                Value = Value
            };
            return Send(C);
        }

        public IRPCResponse SetValue(string namedControl, bool Value) //TODO change return Type
        {

            ControlSetBoolValue C = new ControlSetBoolValue
            {
                Name = namedControl,
                Value = Value
            };
            return Send(C);
        }

        public IRPCResponse SetPosition(string namedControl, double Pos) //TODO change return Type
        {

            ControlSetPosition C = new ControlSetPosition();

            C.Name = namedControl;
            C.Position = Pos;
            return Send(C);
        }


        public IRPCResponse Send(IRPCCommand C)
        {


            Rpc.Send(_tcp_Stream, C);

            return ReadResponseIRCP(_tcp_Stream);
        }


        private IRPCResponse ReadResponseIRCP(NetworkStream stream)
        {

            JObject obj = Rpc.ReadResponseObject(stream);

            if (obj.GetValue("result").Type == JTokenType.Boolean)
            {
                ResponseGetBool C = new ResponseGetBool();
                C.NamedControl = (string)obj.GetValue("Name");
                C.Value = (bool)obj.GetValue("Value");
            }

            if(obj.GetValue("result").Type == JTokenType.Array)
            {
                CrestronConsole.PrintLine("Hello");
            }


            return new ResponseGetBool();
        }

    }




    public interface IRPCCommand
    {
        string Method { get; }
    }

    public interface IRPCResponse
    {

    }

    class ResponseGetDouble : IRPCResponse
    {
        public string namedControl { get; set; }
        public double Value { get; set; }
        public string @String { get; set; }
        public double Position { get; set; }
    }

    class ResponseGetBool : IRPCResponse
    {
        public string NamedControl { get; set; }
        public bool Value { get; set; }
    }

    // ####################
    // RPC COMMANDS
    // ####################

    class ControlSetDoubleValue : IRPCCommand
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

    class ControlSetBoolValue : IRPCCommand
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

    class ControlSetPosition : IRPCCommand
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

    class ControlGet : IRPCCommand
    {
        public string Method
        {
            get
            {
                return "Control.Get";
            }
        }

        public string[] @params { get; set; }

        public ControlGet()
        {
            @params = new string[1];
        }

        public void addString(string x)
        {
            @params[0] = x;
        }

    }

    class LOGON : IRPCCommand
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
    class WatchEnable : IRPCCommand
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
    class PageSubmit : IRPCCommand
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
    class NoOp : IRPCCommand
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

    class ChangeGroupAddControl : IRPCCommand
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


    class ChangeGroupAutoPoll : IRPCCommand
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

    public class ZoneStatus : IRPCResponse
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

    public class QSYS_Response : IRPCResponse
    {
        public string Name { get; }
        public string @String { get; }

        public QSYS_Response(JObject Response)
        {
            if (Response.ContainsKey("Name")) { Name = Response.GetValue("params").Value<String>("Name"); }
            if (Response.ContainsKey("String")) { @String = Response.GetValue("params").Value<String>("String"); }

            Console.WriteLine(Name);
            Console.WriteLine(String);

        }


    }

    public class QSYS_Gain_response : QSYS_Response
    {
        public double Value { get; }
        public double Pos { get; }
        public QSYS_Gain_response(JObject Response) : base(Response)
        {

            if (Response.ContainsKey("Value")) { Value = (double)Response.GetValue("params").Value<double>("Value"); }
            if (Response.ContainsKey("Position")) { Pos = (double)Response.GetValue("params").Value<double>("Position"); }
            Console.WriteLine(Value);
            Console.WriteLine(Pos);

        }
    }
}

