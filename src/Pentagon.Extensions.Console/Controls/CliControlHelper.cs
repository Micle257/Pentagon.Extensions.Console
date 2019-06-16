// -----------------------------------------------------------------------
//  <copyright file="CliControlHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Console.Controls
{
    using System.Security;

    public static class CliControlHelper
    {
        public static bool RunSwitchControl(string text, bool defaultValue)
        {
            var control = new SwitchCliControl(text, defaultValue);

            return control.Run();
        }

        public static string RunTextControl(string text, string defaultValue, string typedText)
        {
            var control = new TextCliControl
                          {
                                  Text = text,
                                  DefaultValue = defaultValue,
                                  TypedText = typedText
                          };

            return control.Run();
        }

        public static SecureString RunSecretTextControl(string text, SecretTextOutputMode outputMode = SecretTextOutputMode.NoOutput)
        {
            var control = new SecretTextCliControl(text, outputMode);

            return control.Run();
        }
    }
}