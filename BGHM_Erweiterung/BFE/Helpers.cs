using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace BFE
{
    public class Helpers
    {
        private static string ConvertPath(string pathToCheck)
        {
            if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Server || CrestronEnvironment.ProgramCompatibility.HasFlag(eCrestronSeries.Series4))
                return pathToCheck.Replace("\\", "/");

            return pathToCheck.Replace("/", "\\");
        }

        public static string UserFolder
        {
            get
            {
                if (CrestronEnvironment.DevicePlatform == eDevicePlatform.Server)
                    return ConvertPath(Path.Combine(Directory.GetApplicationRootDirectory(), "user"));
                else
                    return ConvertPath(Path.Combine("/", "user"));
            }
        }

        public static void PrintInfo()
        {
            CrestronConsole.PrintLine("");
            CrestronConsole.PrintLine("    ____  ____________");
            CrestronConsole.PrintLine("   / __ )/ ____/ ____/");
            CrestronConsole.PrintLine("  / __  / /_  / __/   ");
            CrestronConsole.PrintLine(" / /_/ / __/ / /___   ");
            CrestronConsole.PrintLine("/_____/_/   /_____/   ");
            CrestronConsole.PrintLine("");
            CrestronConsole.PrintLine("");
            CrestronConsole.PrintLine("**************************************");
            CrestronConsole.PrintLine("*      Crestron-Mediensteuerung      *");
            CrestronConsole.PrintLine("*                                    *");
            CrestronConsole.PrintLine("*            Software by             *");
            CrestronConsole.PrintLine("*         Maximilian Detlof          *");
            CrestronConsole.PrintLine("* BFE Studio und Medien Systeme GmbH *");
            CrestronConsole.PrintLine("* +49 6131 946-267 / mdetlof@bfe.tv  *");
            CrestronConsole.PrintLine("*            www.bfe.tv              *");
            CrestronConsole.PrintLine("*           An der Fahrt 1           *");
            CrestronConsole.PrintLine("*            55124 Mainz             *");
            CrestronConsole.PrintLine("**************************************");
        }
    }
}

