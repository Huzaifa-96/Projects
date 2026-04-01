using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADS_Assess1
{
    //EmergencyCall class implementation for use with EmergencyQueue structure

    //Please finish the following implementation.
    //Do not delete the current function definitions, just replace the exceptions with the required functionality.

    //See material from 'C# Classes' in week 2 to aid with implementation.
    internal class EmergencyCall : IComparable
    {
        private string callername;
        private string emergencytpe;
        private int severitylevel;

        public EmergencyCall(string callername, string emergencytype, int severitylevel)
        {
            CallerName = callername;
            EmergencyType = emergencytype;
            SeverityLevel = severitylevel;
        }

        public string CallerName
        {
            get { return callername; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) callername = "Unknown Caller"; // prevents blank caller name
                else callername = value.Trim();
            }
        }

        public string EmergencyType
        {
            get { return emergencytpe; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) emergencytpe = "Unknown"; // prevents blank emergency type
                else emergencytpe = value.Trim();
            }
        }

        public int SeverityLevel
        {
            get { return severitylevel; }
            set
            {
                if (value < 1) severitylevel = 1; // enforce minimum severity
                else if (value > 5) severitylevel = 5; // enforce maximum severity
                else severitylevel = value;
            }

        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1; // null is always “lower”
            EmergencyCall other = obj as EmergencyCall;
            if (other == null) throw new ArgumentException("Object is not EmergencyCall"); // ensures safe comparison
            return this.SeverityLevel.CompareTo(other.SeverityLevel);
        }

        public override string ToString()
        {
            return $"Caller: {CallerName}, Type: {EmergencyType}, Severity: {SeverityLevel}"; // formatted output
        }

    }

}
