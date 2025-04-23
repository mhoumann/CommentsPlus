using System;
using System.Diagnostics;

using Microsoft.Win32;

namespace CommentsPlus
{
    static class RegistryHelper
    {
        public static bool IsEnabled(string featureName, bool defaultValue = true)
        {
            bool res = defaultValue;

            try
            {
                using (var subKey = Registry.CurrentUser.OpenSubKey("Software\\CommentsPlus", false))
                {
                    if (subKey != null)
                    {
                        int defaultRegVal = defaultValue ? 1 : 0;
                        int value = Convert.ToInt32(subKey.GetValue(featureName, defaultRegVal));
                        res = value != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Failed to read registry: " + ex.Message, "CommentsPlus");
            }

            return res;
        }
    }
}