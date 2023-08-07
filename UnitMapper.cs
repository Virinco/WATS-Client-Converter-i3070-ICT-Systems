using System;
using System.Collections.Generic;

namespace ICTKeysight3070Converter
{
    internal class UnitMapper
    {
        private class TestResultInfo
        {
            public string Prefix { get; set; }
            public string Purpose { get; set; }
            public string Unit { get; set; }
            public string FullUnitName { get; set; }
            public double Multiplier { get; set; }
        }

        private static readonly Dictionary<string, TestResultInfo> map;

        static UnitMapper()
        {
            map = new Dictionary<string, TestResultInfo>
            {
                {"A-CAP",  new TestResultInfo {     Prefix = "A-CAP",   Purpose = "capacitor test results",                    Unit = "F",  FullUnitName = "Farads",    Multiplier = 1}},
                {"A-DIO",  new TestResultInfo {     Prefix = "A-DIO",   Purpose = "diode test results",                        Unit = "V",  FullUnitName = "Volts",     Multiplier = 1}},
                {"A-FUS",  new TestResultInfo {     Prefix = "A-FUS",   Purpose = "fuse test results",                         Unit = "A",  FullUnitName = "Amperes",   Multiplier = 1}},
                {"A-IND",  new TestResultInfo {     Prefix = "A-IND",   Purpose = "inductor test results",                     Unit = "H",  FullUnitName = "Henrys",    Multiplier = 1}},
                {"A-JUM",  new TestResultInfo {     Prefix = "A-JUM",   Purpose = "jumper test results",                       Unit = "Ω",  FullUnitName = "Ohms",      Multiplier = 1}},
                {"A-MEA",  new TestResultInfo {     Prefix = "A-MEA",   Purpose = "measurement results",                       Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"A-NFE",  new TestResultInfo {     Prefix = "A-NFE",   Purpose = "N-channel FET test results",                Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"A-NPN",  new TestResultInfo {     Prefix = "A-NPN",   Purpose = "NPN transistor test results",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"A-PFE",  new TestResultInfo {     Prefix = "A-PFE",   Purpose = "P-channel FET test results",                Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"A-PNP",  new TestResultInfo {     Prefix = "A-PNP",   Purpose = "PNP transistor test results",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"A-POT",  new TestResultInfo {     Prefix = "A-POT",   Purpose = "potentiometer test results",                Unit = "Ω",  FullUnitName = "Ohms",      Multiplier = 1}},
                {"A-RES",  new TestResultInfo {     Prefix = "A-RES",   Purpose = "resistor test results",                     Unit = "Ω",  FullUnitName = "Ohms",      Multiplier = 1}},
                {"A-SWI",  new TestResultInfo {     Prefix = "A-SWI",   Purpose = "switch test results",                       Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"A-ZEN",  new TestResultInfo {     Prefix = "A-ZEN",   Purpose = "zener test results",                        Unit = "V",  FullUnitName = "Volts",     Multiplier = 1}},
                {"ALM",    new TestResultInfo {     Prefix = "ALM",     Purpose = "identify a real-time alarm",                Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"AID",    new TestResultInfo {     Prefix = "AID",     Purpose = "identify board causing an alarm",           Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"ARRAY",  new TestResultInfo {     Prefix = "ARRAY",   Purpose = "digitizer results analysis",                Unit = "V",  FullUnitName = "Volts",     Multiplier = 1}},
                {"BATCH",  new TestResultInfo {     Prefix = "BATCH",   Purpose = "batch identifier log",                      Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"BLOCK",  new TestResultInfo {     Prefix = "BLOCK",   Purpose = "test block identifier",                     Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"BS-CON", new TestResultInfo {     Prefix = "BS-CON",  Purpose = "describe boundary-scan test",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"BS-O",   new TestResultInfo {     Prefix = "BS-O",    Purpose = "list of open pins (boundary-scan)",         Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"BS-S",   new TestResultInfo {     Prefix = "BS-S",    Purpose = "list of shorted pins (boundary-scan)",      Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"BTEST",  new TestResultInfo {     Prefix = "BTEST",   Purpose = "describe board test log",                   Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"DPIN",   new TestResultInfo {     Prefix = "DPIN",    Purpose = "list failing pins for one device",          Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"D-PLD",  new TestResultInfo {     Prefix = "D-PLD",   Purpose = "PLD programming results log",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"D-T",    new TestResultInfo {     Prefix = "D-T",     Purpose = "digital shorts test results",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"INDICT", new TestResultInfo {     Prefix = "INDICT",  Purpose = "list potentially failing devices",          Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"LIM2",   new TestResultInfo {     Prefix = "LIM2",    Purpose = "analog test high/low limits",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"LIM3",   new TestResultInfo {     Prefix = "LIM3",    Purpose = "analog test nominal and tolerance limits",  Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"NETV",   new TestResultInfo {     Prefix = "NETV",    Purpose = "network verification record",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"NODE",   new TestResultInfo {     Prefix = "NODE",    Purpose = "list of nodes",                             Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"PCHK",   new TestResultInfo {     Prefix = "PCHK",    Purpose = "Polarity Check test results",               Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"PIN",    new TestResultInfo {     Prefix = "PIN",     Purpose = "list of pins",                              Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"PF",     new TestResultInfo {     Prefix = "PF",      Purpose = "pins failure results",                      Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"PRB",    new TestResultInfo {     Prefix = "PRB",     Purpose = "probe card identification",                 Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"P-T",    new TestResultInfo {     Prefix = "P-T",     Purpose = "probe tips test results",                   Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"P-VOL",  new TestResultInfo {     Prefix = "P-VOL",   Purpose = "power supply voltage results",              Unit = "V",  FullUnitName = "Volts",     Multiplier = 1}},
                {"P-WAV",  new TestResultInfo {     Prefix = "P-WAV",   Purpose = "waveform digitizer results",                Unit = "V",  FullUnitName = "Volts",     Multiplier = 1}},
                {"RID",    new TestResultInfo {     Prefix = "RID",     Purpose = "rework identifier log",                     Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"SP-CON", new TestResultInfo {     Prefix = "SP-CON",  Purpose = "describe spectral analysis",                Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"SP-D",   new TestResultInfo {     Prefix = "SP-D",    Purpose = "list of defect sites (spectral analysis)",  Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"SP-F",   new TestResultInfo {     Prefix = "SP-F",    Purpose = "list of failing sites (spectral analysis)", Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"TID",    new TestResultInfo {     Prefix = "TID",     Purpose = "identify test invoking the alarm",          Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"V-CON",  new TestResultInfo {     Prefix = "V-CON",   Purpose = "describe vectorless test",                  Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"V-D",    new TestResultInfo {     Prefix = "V-D",     Purpose = "list of defect sites (vectorless test)",    Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"V-F",    new TestResultInfo {     Prefix = "V-F",     Purpose = "list of failing sites (vectorless test)",   Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"V-M",    new TestResultInfo {     Prefix = "V-M",     Purpose = "vectorless test results",                   Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"W",      new TestResultInfo {     Prefix = "W",       Purpose = "description of warning",                    Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"WID",    new TestResultInfo {     Prefix = "WID",     Purpose = "work order identifier log",                 Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"Z-CON",  new TestResultInfo {     Prefix = "Z-CON",   Purpose = "describe impedance test",                   Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"Z-D",    new TestResultInfo {     Prefix = "Z-D",     Purpose = "list of defect sites (impedance test)",     Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"Z-F",    new TestResultInfo {     Prefix = "Z-F",     Purpose = "list of failing sites (impedance test)",    Unit = "",   FullUnitName = "",          Multiplier = 1}},
                {"Z-M",    new TestResultInfo {     Prefix = "Z-M",     Purpose = "impedance test results",                    Unit = "Ω",  FullUnitName = "Ohms",      Multiplier = 1}},
            };
        }
        static public (double, string) GetMultiplierAndUnitHardcoded(string prefix, double value)
        {
            if (map.TryGetValue(prefix, out TestResultInfo info))
            {
                return (info.Multiplier, info.Unit);
            }
            else
            {
                throw new KeyNotFoundException($"No information found for prefix {prefix}");
            }
        }
        static public (double, string) GetMultiplierAndUnitDynamicallyBasedOnNumberSize(string prefix, double value)
        {
            if (map.TryGetValue(prefix, out TestResultInfo info))
            {
                if (info.Unit != "")
                {
                    return ConvertToEngineeringNotation(value, info.Unit);
                }
                else
                {
                    return (1, "");
                }
            }
            else
            {
                throw new KeyNotFoundException($"No information found for prefix {prefix}");
            }
        }

        public static (double, string) ConvertToEngineeringNotation(double value, string unit)
        {
            string[] unitPrefixes = { "p", "n", "µ", "m", "", "k", "M", "G", "T" };
            int unitIndex = 4; // Start from base unit
            double multiplier = 1.0;

            double tempValue = Math.Abs(value);

            while (tempValue >= 1000 && unitIndex < unitPrefixes.Length - 1)
            {
                tempValue /= 1000;
                unitIndex++;
                multiplier /= 1000;
            }
            while (tempValue < 1 && unitIndex > 0)
            {
                tempValue *= 1000;
                unitIndex--;
                multiplier *= 1000;
            }

            string unitPrefix = unitPrefixes[unitIndex];
            string fullUnit = unitPrefix + unit;

            return (multiplier, fullUnit);
        }
    }
}