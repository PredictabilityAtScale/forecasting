using FocusedObjective.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace FocusedObjective.Contract
{
    [SimMLElement("distribution", "Defines a random number generator that returns values following common and custom distribution patterns.", false, HasMandatoryAttributes = true, ParentElement = "distributions")]
    public class SetupDistributionData : Distributions.DistributionData, IValidate
    {
        public SetupDistributionData()
        {
        }
        
        public SetupDistributionData(XElement source, XElement errors)
        {
            Source = source;
            this.fromXML(source, errors);
        }

        [SimMLAttribute("name","Unique name for this distribution. This name is used throughout this model to refer to this instance of a distribution.", true)]
        [SimMLAttribute("numberType", "Number type returned by this distribution.", false, ValidValues = "double|integer")]
        [SimMLAttribute("shape", "Specific distribution shape. There are over 25 built-in distribution types. Refer to the knowledge base for reference.", true)]
        [SimMLAttribute("generator", "Underlying algorithm used for the base uniform random number generator. Normally, you can ignore this value and leave it at its default 'alf' value.", false, ValidValues = "alf|mt19937|xordshift128")]
        [SimMLAttribute("parameters", "Specific distribution parameters separated by ',' character. Refer to the knowledge base for reference on your specific distribution defined in the 'shape' value.", false)]
        [SimMLAttribute("separatorCharacter", "Character that specifies a new entry for distributions that take sample data as an input.", false)]
        [SimMLAttribute("count", "Count of random numbers to ge generated. Default is 1000.", false)]
        [SimMLAttribute("lowBound", "Lowest number allowed to be generated.", false)]
        [SimMLAttribute("highBound", "Highest number allowed to be generated.", false)]
        [SimMLAttribute("boundProcessing", "Determines if the distribution is clipped at the low and high bounds (alters distribution), or if the distribution values are stretched (or squashed) bettwen the bounds. Default is 'clip'.", false, ValidValues="clip|stretch")]
        [SimMLAttribute("location", "The lowest value starting point of the distribution along the X-axis.", false)]
        [SimMLAttribute("zeroHandling", "For distributions based on sample data, zeros can be handled in different ways. 'keep' retains zero as a valid value, 'remove' deltes these values, and 'value' replaces it with the value specified in the zeroValue attribute. Default is keep.", false, ValidValues = "keep|remove|value")]
        [SimMLAttribute("zeroValue", "Value to replace 0 with if the zeroHandling type is 'value'.", false)]
        [SimMLAttribute("multiplier", "Multiplier to apply for all returned values. Used to increase or decrease the values returned from a distribution by the same amount. Use 2.0 to double all values, or 0.5 to halve all values. Default is 1.0.", false)]
        [SimMLAttribute("path", "Value to replace 0 with if the zeroHandling type is 'value'.", false)]
        [SimMLAttribute("decimalSeparator", "Decimal separator character used in sample data supplied for some distribution types.", false)]
        [SimMLAttribute("thousandsSeparator", "Thousands separator character used in sample data supplied for some distribution types.", false)]
        public string CodeCompletion { get; set; }

        // methods
        private bool fromXML(XElement source, XElement errors)
        {
            bool success = true;

            Name = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "name",
                Name,
                true
                );

            Shape = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "shape",
                Shape,
                true
                );

            Generator = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "generator",
                Generator,
                false
                );

            Parameters = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "parameters",
                Parameters,
                false
                );

            Separator = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "separatorCharacter",
                Separator,
                false
                );

            NumberType = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "numberType",
                "double", FocusedObjective.Distributions.DistributionNumberType.Double,
                "int", FocusedObjective.Distributions.DistributionNumberType.Integer,
                "integer", FocusedObjective.Distributions.DistributionNumberType.Integer,
                "int32", FocusedObjective.Distributions.DistributionNumberType.Integer,                
                "int64", FocusedObjective.Distributions.DistributionNumberType.Integer 
            );
            

            int _count;

            ContractCommon.ReadAttributeIntValue(
                out _count,
                source,
                errors,
                "count",
                Count,
                false);

            Count = _count;

            double _lowBound;

            ContractCommon.ReadAttributeDoubleValue(
                out _lowBound,
                source,
                errors,
                "lowBound",
                LowBound,
                false);

            LowBound = _lowBound;

            double _highBound;

            ContractCommon.ReadAttributeDoubleValue(
                out _highBound,
                source,
                errors,
                "highBound",
                HighBound,
                false);

            HighBound = _highBound;


            if (source.HasElements)
                Data = source.Elements().First().ToString();
            else
                Data = source.Value;

            BoundProcessing = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "boundProcessing",
                "clip", FocusedObjective.Distributions.DistributionBoundProcessing.Clip,
                "stretch", FocusedObjective.Distributions.DistributionBoundProcessing.Stretch
                );

            double _location;


            ContractCommon.ReadAttributeDoubleValue(
                out _location,
                source,
                errors,
                "location",
                Location,
                false);

            Location = _location;

            double _zeroValue;

            ContractCommon.ReadAttributeDoubleValue(
                out _zeroValue,
                source,
                errors,
                "zeroValue",
                ZeroValue,
                false);

            ZeroValue = _zeroValue;

            ZeroHandling = ContractCommon.ReadMandatoryAttributeListValue(
                source,
                errors,
                "zeroHandling",
                "keep", FocusedObjective.Distributions.ZeroHandlingEnum.Keep,
                "remove", FocusedObjective.Distributions.ZeroHandlingEnum.Remove,
                "delete", FocusedObjective.Distributions.ZeroHandlingEnum.Remove,
                "value", FocusedObjective.Distributions.ZeroHandlingEnum.Value
                );

            double _multiplier = Multiplier;

            ContractCommon.ReadAttributeDoubleValue(
                out _multiplier,
                source,
                errors,
                "multiplier",
                Multiplier,
                false);

            Multiplier = _multiplier;

            Path = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "path",
                Path,
                false
                );

            DecimalSeparator = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "decimalSeparator",
                DecimalSeparator,
                false
                );

            ThousandsSeparator = ContractCommon.ReadAttributeStringValue(
                source,
                errors,
                "thousandsSeparator",
                ThousandsSeparator,
                false
                );

            return success;
        }

        public XElement AsXML(SimulationTypeEnum simType)
        {
            XElement result = new XElement("distribution");

            result.Add(new XAttribute("name",Name.ToString()));
            result.Add(new XAttribute("shape", Shape.ToString()));
            result.Add(new XAttribute("generator", Generator.ToString()));
            result.Add(new XAttribute("parameters", Parameters.ToString()));
            result.Add(new XAttribute("separatorCharacter", Separator.ToString()));
            result.Add(new XAttribute("numberType", NumberType.ToString()));
            result.Add(new XAttribute("count", Count.ToString()));
            result.Add(new XAttribute("lowBound", LowBound.ToString()));
            result.Add(new XAttribute("highBound", HighBound.ToString()));
            result.Add(new XAttribute("rangeProcessing", BoundProcessing.ToString()));
            result.Add(new XAttribute("location", Location.ToString()));
            result.Add(new XAttribute("zeroValue", ZeroValue.ToString()));
            result.Add(new XAttribute("zeroHandling", ZeroHandling.ToString()));
            result.Add(new XAttribute("multiplier", Multiplier.ToString()));
            result.Add(new XAttribute("path", Path));
            result.Add(new XAttribute("decimalSeparator", DecimalSeparator));
            result.Add(new XAttribute("thousandsSeparator", ThousandsSeparator));

            if (!string.IsNullOrEmpty(Data))
                result.Add(Data); 

            return result;
        }

        public bool Validate(SimulationData data, XElement errors)
        {
            bool success = true;

            // update path with basePath if file doesn't exist
            if (!string.IsNullOrWhiteSpace(Path))
            {
                // if the path doesnt exist the way it is, combine with the base path
                if (!System.IO.File.Exists(Path))
                    Path = System.IO.Path.Combine(data.Execute.BasePath, Path);

                // if still not found, use the app path as the base path
                if (!System.IO.File.Exists(Path))
                    Path =  System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase) +  Path;

            }

            if (FocusedObjective.Distributions.DistributionHelper.CreateDistribution(this, errors) == null)
            {
                // the errors are logged as part of each distribution. Do we need more?
                success = false;
            }

            return success;
        }
    }
}
