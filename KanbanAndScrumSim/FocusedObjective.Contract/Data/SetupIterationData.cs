using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FocusedObjective.Common;
using FocusedObjective.Contract.Data;

namespace FocusedObjective.Contract
{
    [SimMLElement("iteration", "Contains details about iteration length and scope (Scrum models only).", false)]
    public class SetupIterationData : SensitivityBase, IValidate
    {
        public SetupIterationData()
        {
        }

        public SetupIterationData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        // private members and defaults
        private double _storyPointsPerIterationLowBound = 0.0;
        private double _storyPointsPerIterationHighBound = 0.0;
        private string _storyPointsPerIterationDistribution = "";
        private bool _allowedToOverAllocate = true;

        // public properties
        [SimMLAttribute("storyPointsPerIterationLowBound", "The lowest number of story points per iteration (Scrum only). Used as the lowest random number created when forecasting each iteration.", false)]
        public double StoryPointsPerIterationLowBound
        {
            get { return _storyPointsPerIterationLowBound; }
            set { _storyPointsPerIterationLowBound = value; }
        }

        [SimMLAttribute("storyPointsPerIterationHighBound", "The highest number of story points per iteration (Scrum only). Used as the highest random number created when forecasting each iteration.", false)]
        public double StoryPointsPerIterationHighBound
        {
            get { return _storyPointsPerIterationHighBound; }
            set { _storyPointsPerIterationHighBound = value; }
        }

        [SimMLAttribute("storyPointsPerIterationDistribution", "The probability distribution used to generate randon interation story points targets as an alternative to specifying low and high bounds (Scrum only). The distributions themselves are specified by name and defined in the <distributions>...</distributions> section.", false)]
        public string StoryPointsPerIterationDistribution
        {
            get { return _storyPointsPerIterationDistribution; }
            set { _storyPointsPerIterationDistribution = value; }
        }

        [SimMLAttribute("allowedToOverAllocate", "Defines whether a sprint is allocated work above or below the iteration target points (over and under is by the value of a single item). DEfault is true.", false, ValidValues="true|false")]
        public bool AllowedToOverAllocate
        {
            get { return _allowedToOverAllocate; }
            set { _allowedToOverAllocate = value; }
        }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _storyPointsPerIterationLowBound,
                source,
                errors,
                "storyPointsPerIterationLowBound",
                _storyPointsPerIterationLowBound,
                false);

            success = success && ContractCommon.ReadAttributeDoubleValue(
                out _storyPointsPerIterationHighBound,
                source,
                errors,
                "storyPointsPerIterationHighBound",
                _storyPointsPerIterationHighBound,
                false);

            _storyPointsPerIterationDistribution = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "storyPointsPerIterationDistribution",
                _storyPointsPerIterationDistribution,
                false);


            _allowedToOverAllocate = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "allowedToOverAllocate",
                "true", true,
                "false", false,
                "yes", true,
                "no", false);

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("iteration");

            result.Add(new XAttribute("storyPointsPerIterationLowBound", _storyPointsPerIterationLowBound.ToString()));
            result.Add(new XAttribute("storyPointsPerIterationHighBound", _storyPointsPerIterationHighBound.ToString()));
            result.Add(new XAttribute("storyPointsPerIterationDistribution", _storyPointsPerIterationDistribution.ToString()));

            if (_allowedToOverAllocate)
                result.Add(new XAttribute("allowedToOverAllocate", "true"));
            else
                result.Add(new XAttribute("allowedToOverAllocate", "false"));

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            if (data.Execute.SimulationType == SimulationTypeEnum.Scrum)
            {
                if (StoryPointsPerIterationDistribution == "")
                {
                    success = ContractCommon.CheckValueGreaterThan(errors, StoryPointsPerIterationLowBound, 0, "storyPointsPerIterationLowBound", "iteration", Source) && success;
                    success = ContractCommon.CheckValueGreaterThan(errors, StoryPointsPerIterationHighBound, 0, "storyPointsPerIterationHighBound", "iteration", Source) && success;
                }
                else
                {
                    SetupDistributionData dist =
                        data.Setup.Distributions.Where(d => string.Compare(d.Name, StoryPointsPerIterationDistribution, true) == 0).FirstOrDefault();

                    if (dist == null)
                    {
                        success = false;
                        Helper.AddError(errors, ErrorSeverityEnum.Error, 47, string.Format(Strings.Error47, StoryPointsPerIterationDistribution, "setup/iteration", "iteration"), Source);
                    }

                    // distribution validated in setup/distribution

                }
            }

            return success;
        }
    }


}
