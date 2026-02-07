// MIT License

// DeusaldServerTools:
// Copyright (c) 2020 Adam "Deusald" Orliński

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using DeusaldSharp;
using JetBrains.Annotations;

namespace DeusaldServerToolsClient
{
    [PublicAPI]
    public static class Verificators
    {
        public static void VerifyEnum<T>(this T value, string nameOfVariable, List<T>? illegalValues = null) where T : Enum
        {
            if (!Enum.IsDefined(value.GetType(), value))
                throw new VerificationException($"enum:{nameOfVariable} have incorrect value {value}.");

            if (illegalValues != null && illegalValues.Contains(value))
                throw new VerificationException($"enum-illegal-value:{nameOfVariable} have incorrect value {value}.");
        }

        public static void VerifyAllowedElement<T>(this T value, string nameOfVariable, List<T> allowedValues)
        {
            if (allowedValues.Contains(value)) return;
            throw new VerificationException($"element:{nameOfVariable} have incorrect value {value}.");
        }

        public static void VerifyDifferentValues<T>(this T value, T secondValue, string nameOfVariable, string secondNameOfVariable) where T : Enum
        {
            if (!EqualityComparer<T>.Default.Equals(value, secondValue)) return;
            throw new VerificationException($"element:{nameOfVariable} have incorrect value {value} as it cannot be the same as {secondNameOfVariable}.");
        }

        public static void VerifyDuplicates<T>(this List<T> values, string nameOfVariable)
        {
            HashSet<T> hashSet = new HashSet<T>(values);
            if (hashSet.Count != values.Count)
                throw new VerificationException($"list-duplicates:{nameOfVariable} have illegal duplicates.");
        }

        public static void VerifyLength<T>(this List<T> values, string nameOfVariable, int min, int max)
        {
            if (values.Count >= min && values.Count <= max) return;
            throw new VerificationException($"list-min-max:{nameOfVariable}:{min}:{max} have illegal length. Length should be between {min} and {max}.");
        }

        public static void VerifyByte(this byte value, string nameOfVariable, byte minValue, byte maxValue)
        {
            if (value >= minValue && value <= maxValue) return;
            throw new VerificationException($"min-max:{nameOfVariable}:{minValue}:{maxValue} have incorrect value {value}. Should be between {minValue} and {maxValue}.");
        }

        public static void VerifyStringLength(this string value, string nameOfVariable, int min, int max)
        {
            if (value.Length >= min && value.Length <= max) return;
            throw new VerificationException($"s-min-max:{nameOfVariable}:{min}:{max} have incorrect value {value}. Length should be between {min} and {max}.");
        }

        public static void VerifySingleBit<T>(this T value, string nameOfVariable) where T : Enum
        {
            if (((uint)(object)value).IsSingleBitOn()) return;
            throw new VerificationException($"single-bit:{nameOfVariable}: have incorrect value {value}.");
        }
    }
}