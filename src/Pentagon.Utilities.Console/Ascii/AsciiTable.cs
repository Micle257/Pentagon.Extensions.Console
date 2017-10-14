// -----------------------------------------------------------------------
//  <copyright file="AsciiTable.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Utilities.Console.Ascii
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class AsciiTable
    {
        [JsonProperty(propertyName: "codes")]
        public IList<AsciiCode> Codes { get; set; }
    }
}