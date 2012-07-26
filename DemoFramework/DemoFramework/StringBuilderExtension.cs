#region Using

using System;
using System.Globalization;
using System.Text;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// The extension class for StringBuilder avoids the boxing occuring
    /// when is appended numbers.
    /// </summary>
    public static class StringBuilderExtension
    {
        static int[] numberGroupSizes = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSizes;

        static char[] numberString = new char[32];

        public static StringBuilder AppendNumber(this StringBuilder builder, int number)
        {
            return AppendNumberInternal(builder, number, 0, AppendNumberOptions.None);
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, int number, AppendNumberOptions options)
        {
            return AppendNumberInternal(builder, number, 0, options);
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, float number)
        {
            return AppendNumber(builder, number, 2, AppendNumberOptions.None);
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, float number, AppendNumberOptions options)
        {
            return AppendNumber(builder, number, 2, options);
        }

        public static StringBuilder AppendNumber(this StringBuilder builder, float number, int decimalCount, AppendNumberOptions options)
        {
            if (float.IsNaN(number)) return builder.Append("NaN");
            if (float.IsNegativeInfinity(number)) return builder.Append("-Infinity");
            if (float.IsPositiveInfinity(number)) return builder.Append("+Infinity");

            int intNumber = (int) (number * (float) Math.Pow(10, decimalCount) + 0.5f);
            return AppendNumberInternal(builder, intNumber, decimalCount, options);
        }


        static StringBuilder AppendNumberInternal(StringBuilder builder, int number, int decimalCount, AppendNumberOptions options)
        {
            var numberFormat = CultureInfo.CurrentCulture.NumberFormat;

            int index = numberString.Length;
            int decimalPos = index - decimalCount;

            if (decimalPos == index)
                decimalPos = index + 1;

            int numberGroupIndex = 0;
            int numberGroupCount = numberGroupSizes[numberGroupIndex] + decimalCount;

            bool showNumberGroup = (options & AppendNumberOptions.NumberGroup) != 0;
            bool showPositiveSign = (options & AppendNumberOptions.PositiveSign) != 0;

            bool isNegative = number < 0;
            number = Math.Abs(number);

            do
            {
                // decimal point.
                if (index == decimalPos)
                {
                    numberString[--index] = numberFormat.NumberDecimalSeparator[0];
                }

                // digit comma.
                if (--numberGroupCount < 0 && showNumberGroup)
                {
                    numberString[--index] = numberFormat.NumberGroupSeparator[0];

                    if (numberGroupIndex < numberGroupSizes.Length - 1)
                        numberGroupIndex++;

                    numberGroupCount = numberGroupSizes[numberGroupIndex++];
                }

                // number to char.
                numberString[--index] = (char) ('0' + (number % 10));
                number /= 10;

            } while (number > 0 || decimalPos <= index);


            // append a positive/negative sign.
            if (isNegative)
            {
                numberString[--index] = numberFormat.NegativeSign[0];
            }
            else if (showPositiveSign)
            {
                numberString[--index] = numberFormat.PositiveSign[0];
            }

            // finish.
            return builder.Append(numberString, index, numberString.Length - index);
        }
    }
}
