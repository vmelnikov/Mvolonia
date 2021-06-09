using System.Collections.Generic;
using System.Globalization;

namespace Mvolonia.Controls.Collections.Comparers
{
        /// <summary>
        /// Creates a comparer class that takes in a CultureInfo as a parameter,
        /// which it will use when comparing strings.
        /// </summary>
        internal class CultureSensitiveComparer : Comparer<object>
        {
            /// <summary>
            /// Private accessor for the CultureInfo of our comparer
            /// </summary>
            private readonly CultureInfo _culture;

            /// <summary>
            /// Creates a comparer which will respect the CultureInfo
            /// that is passed in when comparing strings.
            /// </summary>
            /// <param name="culture">The CultureInfo to use in string comparisons</param>
            public CultureSensitiveComparer(CultureInfo culture)
            {
                _culture = culture ?? CultureInfo.InvariantCulture;
            }

            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to or greater than the other.
            /// </summary>
            /// <param name="x">first item to compare</param>
            /// <param name="y">second item to compare</param>
            /// <returns>Negative number if x is less than y, zero if equal, and a positive number if x is greater than y</returns>
            /// <remarks>
            /// Compares the 2 items using the specified CultureInfo for string and using the default object comparer for all other objects.
            /// </remarks>
            public override int Compare(object x, object y)
            {
                if (x is null)
                {
                    if (y is null)
                        return 0;
                    return -1;
                }
                if (y is null)
                    return 1;
                

                // at this point x and y are not null
                if (x is string xStr && y is string yStr)
                    return _culture.CompareInfo.Compare(xStr, yStr);
                return Default.Compare(x, y);
            }

        }
}