using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Virinco.WATS.Integration.TextConverter;
using Virinco.WATS.Interface;

namespace ICTKeysight3070Converter

{
    public class ICTKeysight3070Importer : TextConverterBase
    {
        public ICTKeysight3070Importer() : base()
        {
            ConverterParameters.Add("numberFormatMode", "prefix");
        }

        public void CleanUp()
        {

        }

        private StepStatusType GetStepStatusType(string status)
        {
            if (status == "0" || status == "00")
            {
                return StepStatusType.Passed;
            }
            else
            {
                return StepStatusType.Failed;
            }
        }

        private UUTStatusType GetUUTStatusType(string status)
        {
            if (status == "0" || status == "00")
            {
                return UUTStatusType.Passed;
            }
            else
            {
                return UUTStatusType.Failed;
            }
        }

        string group = "";
        string compRef = "";
        string reportText = "";
        UUTStatusType endStatus = UUTStatusType.Passed;

        NumericLimitStep multiNumericStep = null;

        protected override bool ProcessMatchedLine(SearchFields.SearchMatch match, ref ReportReadState readState)
        {
            //apiRef.TestMode = TestModeType.Import;
            if (match == null)
            {
                if (!string.IsNullOrEmpty(currentUUT.SerialNumber))
                {
                    if (!string.IsNullOrEmpty(reportText))
                    {
                        if (reportText.Length > 5000)
                        {
                            reportText = reportText.Substring(0, 5000);
                        }
                        currentUUT.Comment = reportText;

                    }
                    currentUUT.OperationType = apiRef.GetOperationType(ConverterParameters["operationTypeCode"]);

                    // override status at the end because we dont use TestModeType.Import
                    currentUUT.Status = endStatus;

                    apiRef.Submit(currentUUT);
                    group = "";
                    compRef = "";
                    reportText = "";

                }
                return true;

            }

            switch (match.matchField.fieldName)
            {
                case "BTEST":
                    {
                        currentUUT.SerialNumber = (string)match.GetSubField("SerialNumber");
                        currentUUT.StartDateTime = (DateTime)match.GetSubField("StartDate");
                        endStatus = GetUUTStatusType((string)match.GetSubField("Status"));
                        currentUUT.ExecutionTime = (double)match.GetSubField("ExecutionTime");
                        multiNumericStep = null;
                        break;

                    }
                case "BLOCK":
                    {
                        string BlockDesignator = (string)match.GetSubField("BlockDesignator");
                        string[] BlockDesignatorParts = BlockDesignator.Split('%');

                        if (Regex.IsMatch(BlockDesignator, @"^\d+%"))
                        {
                            string socketIndex = BlockDesignatorParts[0];
                            currentUUT.TestSocketIndex = short.Parse(socketIndex);

                            compRef = string.Join(" ", BlockDesignatorParts.Skip(1));
                        }
                        else
                        {
                            compRef = string.Join(" ", BlockDesignatorParts);
                        }

                        multiNumericStep = null;
                        reportText = "";
                        break;
                    }
                case "TestLIM2":
                case "TestLIM3":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);

                        }



                        double multiplier = 1;
                        string unit = "";
                        if (ConverterParameters["numberFormatMode"] == "prefix")
                        {
                            (multiplier, unit) = UnitMapper.GetMultiplierAndUnitDynamicallyBasedOnNumberSize(group, (double)match.GetSubField("LowLim"));
                        }
                        else
                        {
                            (multiplier, unit) = UnitMapper.GetMultiplierAndUnitHardcoded(group);
                        }


                        NumericLimitStep currentStep = currentSequence.AddNumericLimitStep(compRef);
                        NumericLimitTest currentTest = currentStep.AddTest((double)match.GetSubField("Meas") * multiplier, CompOperatorType.GELE, (double)match.GetSubField("LowLim") * multiplier, (double)match.GetSubField("HighLim") * multiplier, unit, GetStepStatusType((string)match.GetSubField("Status")));
                        multiNumericStep = null;
                        break;
                    }
                case "TestLIM2Multi":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                            multiNumericStep = null;
                        }
                        if (multiNumericStep == null)
                        {
                            multiNumericStep = currentSequence.AddNumericLimitStep(compRef);
                        }

                        double multiplier = 1;
                        string unit = "";
                        if (ConverterParameters["numberFormatMode"] == "prefix")
                        {
                            (multiplier, unit) = UnitMapper.GetMultiplierAndUnitDynamicallyBasedOnNumberSize(group, (double)match.GetSubField("LowLim"));
                        } else
                        {
                            (multiplier, unit) = UnitMapper.GetMultiplierAndUnitHardcoded(group);
                        }

                       
                        NumericLimitTest currentTest = multiNumericStep.AddMultipleTest((double)match.GetSubField("Meas") * multiplier, CompOperatorType.GELE, (double)match.GetSubField("LowLim") * multiplier, (double)match.GetSubField("HighLim") * multiplier, unit, (string)match.GetSubField("MeasName"), GetStepStatusType((string)match.GetSubField("Status")));
                        break;
                    }
                case "BS-CON":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                        }

                        var testNameString = (string)match.GetSubField("testName");
                        string[] testNameParts = testNameString.Split('%');
                        string testName;

                        if (Regex.IsMatch(testNameString, @"^\d+%"))
                        {
                            string socketIndex = testNameParts[0];
                            currentUUT.TestSocketIndex = short.Parse(socketIndex);

                            testName = string.Join(" ", testNameParts.Skip(1));
                        }
                        else
                        {
                            testName = string.Join(" ", testNameParts);
                        }

                        NumericLimitStep currentStep = currentSequence.AddNumericLimitStep(testName);
                        currentStep.AddMultipleTest((int)match.GetSubField("shortCount"), "", "Shorts count");
                        currentStep.AddMultipleTest((int)match.GetSubField("opensCount"), "", "Opens count");
                        int status;
                        if (Int32.TryParse((string)match.GetSubField("status"), NumberStyles.Any, CultureInfo.InvariantCulture, out status))
                        {
                            currentStep.AddMultipleTest(status, "", "Test status", GetStepStatusType((string)match.GetSubField("status")));
                        }

                        multiNumericStep = null;
                        break;
                    }
                case "Report":
                    {
                        string repTxt = (string)match.GetSubField("RepTxt");
                        if (!string.IsNullOrEmpty(repTxt))
                        {
                            if (reportText.Length + repTxt.Length > 5000)
                            {
                                reportText += repTxt.Substring(0, 5000 - reportText.Length);
                            }
                            else
                            {
                                reportText += repTxt;
                            }
                        }
                        multiNumericStep = null;
                        break;
                    }
                case "D-T":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                        }

                        var testNameString = (string)match.GetSubField("TestName");
                        string[] testNameParts = testNameString.Split('%');
                        string testName;

                        if (Regex.IsMatch(testNameString, @"^\d+%"))
                        {
                            string socketIndex = testNameParts[0];
                            currentUUT.TestSocketIndex = short.Parse(socketIndex);

                            testName = string.Join(" ", testNameParts.Skip(1));
                        }
                        else
                        {
                            testName = string.Join(" ", testNameParts);
                        }

                        NumericLimitStep currentStep = currentSequence.AddNumericLimitStep(testName);
                        currentStep.AddMultipleTest((int)match.GetSubField("PinCount"), "", "Pin count");
                        currentStep.AddMultipleTest((int)match.GetSubField("TestSubstatus"), "", "Test substans");
                        currentStep.AddMultipleTest((int)match.GetSubField("FailingVectorNumber"), "", "Failing vector number");
                        int status;
                        if (Int32.TryParse((string)match.GetSubField("TestStatus"), NumberStyles.Any, CultureInfo.InvariantCulture, out status))
                        {
                            currentStep.AddMultipleTest(status, "", "Test status", GetStepStatusType((string)match.GetSubField("TestStatus")));
                        }

                        multiNumericStep = null;
                        break;
                    }
                case "TJET":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                        }

                        var testNameString = (string)match.GetSubField("TestName");
                        string[] testNameParts = testNameString.Split('%');
                        string testName;

                        if (Regex.IsMatch(testNameString, @"^\d+%"))
                        {
                            string socketIndex = testNameParts[0];
                            currentUUT.TestSocketIndex = short.Parse(socketIndex);

                            testName = string.Join(" ", testNameParts.Skip(1));
                        }
                        else
                        {
                            testName = string.Join(" ", testNameParts);
                        }

                        NumericLimitStep currentStep = currentSequence.AddNumericLimitStep(testName);
                        currentStep.AddMultipleTest((int)match.GetSubField("PinCount"), "", "Pin count");
                        int status;
                        if (Int32.TryParse((string)match.GetSubField("TestStatus"), NumberStyles.Any, CultureInfo.InvariantCulture, out status))
                        {
                            currentStep.AddMultipleTest(status, "", "Test status", GetStepStatusType((string)match.GetSubField("TestStatus")));
                        }
                        multiNumericStep = null;
                        break;
                    }
                case "PF":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                        }
                        var testNameString = (string)match.GetSubField("TestName");
                        string[] testNameParts = testNameString.Split('%');
                        string testName;

                        if (Regex.IsMatch(testNameString, @"^\d+%"))
                        {
                            string socketIndex = testNameParts[0];
                            currentUUT.TestSocketIndex = short.Parse(socketIndex);

                            testName = string.Join(" ", testNameParts.Skip(1));
                        }
                        else
                        {
                            testName = string.Join(" ", testNameParts);
                        }

                        NumericLimitStep currentStep = currentSequence.AddNumericLimitStep(testName);
                        currentStep.AddMultipleTest((int)match.GetSubField("totalPins"), "", "Total pins");

                        int status;
                        if (Int32.TryParse((string)match.GetSubField("TestStatus"), NumberStyles.Any, CultureInfo.InvariantCulture, out status))
                        {
                            currentStep.AddMultipleTest(status, "", "Test status", GetStepStatusType((string)match.GetSubField("TestStatus")));
                        }

                        multiNumericStep = null;
                        break;
                    }
                case "TS":
                    {
                        if ((string)match.GetSubField("Group") != group)
                        {
                            group = (string)match.GetSubField("Group");
                            currentSequence = currentUUT.GetRootSequenceCall().AddSequenceCall(group);
                        }

                        var testNameString = (string)match.GetSubField("TestName");
                        string[] testNameParts = testNameString.Split('%');
                        string testName;

                        if (Regex.IsMatch(testNameString, @"^\d+%"))
                        {
                            string socketIndex = testNameParts[0];
                            currentUUT.TestSocketIndex = short.Parse(socketIndex);

                            testName = string.Join(" ", testNameParts.Skip(1));
                        }
                        else
                        {
                            testName = string.Join(" ", testNameParts);
                        }

                        NumericLimitStep currentStep = currentSequence.AddNumericLimitStep(testName);
                        currentStep.AddMultipleTest((int)match.GetSubField("ShortsCount"), "", "Shorts count");
                        currentStep.AddMultipleTest((int)match.GetSubField("OpensCount"), "", "Opens count");
                        currentStep.AddMultipleTest((int)match.GetSubField("PhantomsCount"), "", "Phantoms count");
                        int status;
                        if (Int32.TryParse((string)match.GetSubField("TestStatus"), NumberStyles.Any, CultureInfo.InvariantCulture, out status))
                        {
                            currentStep.AddMultipleTest(status, "", "Test status", GetStepStatusType((string)match.GetSubField("TestStatus")));
                        }
                        multiNumericStep = null;
                        break;
                    }
                default:
                    break;

            }
            return true;
        }

        public ICTKeysight3070Importer(IDictionary<string, string> args) : base(args)
        {
            // Format: {@BATCH|UUT type (string)|UUT type rev (string)|fixture id (int)|testhead number (int)|testhead type (string)|process step (string)|batch id (string)
            // |operator id (string)|controller (string)|testplan id (string)|testplan rev (string)|parent panel type (string)|parent panel type rev (string)}
            SearchFields.RegExpSearchField regExpSearchField = searchFields.AddRegExpField(UUTField.UseSubFields, ReportReadState.InHeader, @"^\{@BATCH\|([^|]*)\|([^|]*)\|([^|]*)\|([^|]*)\|([^|]*)\|([^|]*)\|([^|]*)\|([^|]*)\|(?<Station>([^|]*))\|([^|]*)\|(?<Revision>([^|]*))\|([^|]*)\|([^|]*)\|(?<PartNumber>([^|]*))$", null, typeof(string));
            regExpSearchField.AddSubField("Station", typeof(string), null, UUTField.StationName);
            regExpSearchField.AddSubField("PartNumber", typeof(string), null, UUTField.PartNumber);
            regExpSearchField.AddSubField("Revision", typeof(string), null, UUTField.PartRevisionNumber);

            // Format: {@BTEST|board id (string)|test status (int)|start datetime (int)|duration (int)|multiple test (bool)|log level (string)|log set (int)
            // |learning (bool)|known good (bool)|end datetime (int)|status qualifier (string)|board number (int)|parent panel id (string)}
            regExpSearchField = searchFields.AddRegExpField("BTEST", ReportReadState.InHeader, @"^\{@BTEST\|((?<SerialNumber>[^|]*))\|(?<Status>([^|]*))\|(((?<StartDate>[^|]*)))\|((\b0*(?<ExecutionTime>[1-9][0-9]*|0)\b))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))$", null, typeof(string), ReportReadState.InTest);
            regExpSearchField.AddSubField("Status", typeof(string), null);
            regExpSearchField.AddSubField("SerialNumber", typeof(string), null);
            regExpSearchField.AddSubField("StartDate", typeof(DateTime), "yyMMddHHmmss");
            regExpSearchField.AddSubField("ExecutionTime", typeof(double), null);

            // For some reason some reports have 12 separators (|) and another reports har 13 separators. This may be because a the field 'parent panel type rev' is a field newly added to the format. Anyways we check for both.
            regExpSearchField = searchFields.AddRegExpField("BTEST", ReportReadState.InHeader, @"^\{@BTEST\|((?<SerialNumber>[^|]*))\|(?<Status>([^|]*))\|(((?<StartDate>[^|]*)))\|((\b0*(?<ExecutionTime>[1-9][0-9]*|0)\b))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))\|(([^|]*))$", null, typeof(string), ReportReadState.InTest);
            regExpSearchField.AddSubField("Status", typeof(string), null);
            regExpSearchField.AddSubField("SerialNumber", typeof(string), null);
            regExpSearchField.AddSubField("StartDate", typeof(DateTime), "yyMMddHHmmss");
            regExpSearchField.AddSubField("ExecutionTime", typeof(double), null);

            // Format: {@BLOCK|block designator (string)|block status (int)}
            regExpSearchField = searchFields.AddRegExpField("BLOCK", ReportReadState.InTest, @"^{@BLOCK[|](?<BlockDesignator>[^|]*)[|][^|]*$", null, typeof(string));
            regExpSearchField.AddSubField("BlockDesignator", typeof(string), null);

            // Format: {@group name|test status (int)|measured value (float)|{@LIM2|high limit (float)|low limit (float)}}
            // Matches these group names: @A-RES, @A-MEA, @A-NFE, @A-NPN, @A-PFE, @A-PNP, @A-SWI, @A-DIO, @A-JUM
            regExpSearchField = searchFields.AddRegExpField("TestLIM2", ReportReadState.InTest, @"{@(?<Group>[^|]+)[|](?<Status>[^|]*)[|]\s*(?<Meas>[0-9+-E.]+)[|]*{@LIM2[|]\s*(?<HighLim>[0-9+-E.]+)[|]\s*(?<LowLim>[0-9+-E.]+)}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("Status", typeof(string), null);
            regExpSearchField.AddSubField("Meas", typeof(double), null);
            regExpSearchField.AddSubField("HighLim", typeof(double), null);
            regExpSearchField.AddSubField("LowLim", typeof(double), null);

            // Format: {@group name|test status (int)|measured value (float)|measurment name (sting){@LIM2|high limit (float)|low limit (float)}}
            // Matches these group names: @A-RES, @A-MEA, @A-NFE, @A-NPN, @A-PFE, @A-PNP, @A-SWI, @A-DIO, @A-JUM
            regExpSearchField = searchFields.AddRegExpField("TestLIM2Multi", ReportReadState.InTest, @"{@(?<Group>[^|]+)[|](?<Status>[^|]*)[|]\s*(?<Meas>[0-9+-E.]+)[|](?<MeasName>[^}]+)[|]*{@LIM2[|]\s*(?<HighLim>[0-9+-E.]+)[|]\s*(?<LowLim>[0-9+-E.]+)}}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("Status", typeof(string), null);
            regExpSearchField.AddSubField("Meas", typeof(double), null);
            regExpSearchField.AddSubField("MeasName", typeof(string), null);
            regExpSearchField.AddSubField("HighLim", typeof(double), null);
            regExpSearchField.AddSubField("LowLim", typeof(double), null);

            // Format: {@group name|test status (int)|measured value (float)|{@LIM3|nominal value (float)|high limit (float)|minus tolerance (float)}}
            // Matches these group names: @A-RES, @A-ZEN, @A-CAP, @A-FUS, @A-IND, @A-POT
            regExpSearchField = searchFields.AddRegExpField("TestLIM3", ReportReadState.InTest, @"{@(?<Group>[^|]+)[^|]*[|](?<Status>[^|]*)[|](?<Meas>[0-9+-E.]+)[|]*{@LIM3[|](?<Nominal>[0-9+-E.]+)[|](?<HighLim>[0-9+-E.]+)[|](?<LowLim>[0-9+-E.]+)}}", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("Status", typeof(string), null);
            regExpSearchField.AddSubField("Meas", typeof(double), null);
            regExpSearchField.AddSubField("Nominal", typeof(double), null);
            regExpSearchField.AddSubField("HighLim", typeof(double), null);
            regExpSearchField.AddSubField("LowLim", typeof(double), null);

            // Format: {@BS-CON|test designator (string)|status (int)|shorts count (int)|opens count (int)}
            regExpSearchField = searchFields.AddRegExpField("BS-CON", ReportReadState.InTest, @"{@(?<Group>BS-CON)[|](?<testName>.*)[|](?<status>[^|]*)[|](?<shortCount>\d+)[|](?<opensCount>\d+)", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("testName", typeof(string), null);
            regExpSearchField.AddSubField("status", typeof(string), null);
            regExpSearchField.AddSubField("shortCount", typeof(int), null);
            regExpSearchField.AddSubField("opensCount", typeof(int), null);

            // Format: {@RPT|message (string)}
            regExpSearchField = searchFields.AddRegExpField("Report", ReportReadState.InTest, @"{@RPT[|](?<RepTxt>.*)}", null, typeof(string));
            regExpSearchField.AddSubField("RepTxt", typeof(string), null);

            // Format: {@D-T|test status (int)|test substatus (int)|failing vector number (int)|pin count (int)|test designator (string)}
            regExpSearchField = searchFields.AddRegExpField("D-T", ReportReadState.InTest, @"\{@(?<Group>D-T)\|(?<TestStatus>(0|1|5|7|8))*\|(?<TestSubstatus>([0-9]*))\|(?<FailingVectorNumber>([0-9]*))\|(?<PinCount>([0-9]*))\|(?<TestName>(.*))", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("TestStatus", typeof(string), null);
            regExpSearchField.AddSubField("TestSubstatus", typeof(int), null);
            regExpSearchField.AddSubField("FailingVectorNumber", typeof(int), null);
            regExpSearchField.AddSubField("PinCount", typeof(int), null);
            regExpSearchField.AddSubField("TestName", typeof(string), null);

            // Format: {@TJET|test status (int)|pin count (int)|test designator (string)}
            regExpSearchField = searchFields.AddRegExpField("TJET", ReportReadState.InTest, @"\{@(?<Group>TJET)\|(?<TestStatus>(00|01|07))*\|(?<PinCount>([0-9]*))\|(?<TestName>(.*))", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("TestStatus", typeof(string), null);
            regExpSearchField.AddSubField("PinCount", typeof(int), null);
            regExpSearchField.AddSubField("TestName", typeof(string), null);

            // Format: {@PF|designator (string)|test status (int)|total pins (int)}
            regExpSearchField = searchFields.AddRegExpField("PF", ReportReadState.InTest, @"\{@(?<Group>PF)\|(?<TestName>(.*))\|(?<TestStatus>(0|1))\|((?<totalPins>([0-9]*)))", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("totalPins", typeof(int), null);
            regExpSearchField.AddSubField("TestStatus", typeof(string), null);
            regExpSearchField.AddSubField("TestName", typeof(string), null);

            // Format: {@TS|test status (int)|shorts count (int)|opens count (int)|phantoms count (int)|designator (string)}
            regExpSearchField = searchFields.AddRegExpField("TS", ReportReadState.InTest, @"\{@(?<Group>TS)\|(?<TestStatus>(0|1|20))*\|(?<ShortsCount>([0-9]*))\|((?<OpensCount>([0-9]*)))\|((?<PhantomsCount>([0-9]*)))\|(?<TestName>(.*))", null, typeof(string));
            regExpSearchField.AddSubField("Group", typeof(string), null);
            regExpSearchField.AddSubField("TestStatus", typeof(string), null);
            regExpSearchField.AddSubField("ShortsCount", typeof(int), null);
            regExpSearchField.AddSubField("OpensCount", typeof(int), null);
            regExpSearchField.AddSubField("PhantomsCount", typeof(int), null);
            regExpSearchField.AddSubField("TestName", typeof(string), null);
        }
    }
}
