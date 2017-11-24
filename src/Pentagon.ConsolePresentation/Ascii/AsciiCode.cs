// -----------------------------------------------------------------------
//  <copyright file="AsciiCode.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.ConsolePresentation.Ascii
{
    using System.Linq;
    using Newtonsoft.Json;
    using Utilities.Data.Json;

    public class AsciiCode
    {
        [JsonIgnore]
        public char Char => string.IsNullOrEmpty(Symbol) ? '?' : Symbol.FirstOrDefault();

        [JsonProperty(propertyName: "code")]
        public int Code { get; set; }

        [JsonProperty(propertyName: "symbol")]
        public string Symbol { get; set; }

        [JsonProperty(propertyName: "type")]
        [JsonConverter(typeof(EnumJsonConverter<AsciiCodeType>))]
        public AsciiCodeType Type { get; set; }

        [JsonProperty(propertyName: "description")]
        public string Description { get; set; }
    }
}