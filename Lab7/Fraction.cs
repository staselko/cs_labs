using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace lab7
{
    class Fraction : IComparable<Fraction>, IEquatable<Fraction>, IFormattable, ICloneable
    {
        public long Numerator { get; private set; }
        public long Denominator { get; private set; }

        public Fraction(long numerator, long denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("Denominator can't be zero!");
            }
            Reduce(ref numerator, ref denominator);
            Numerator = numerator;
            Denominator = denominator;
        }

        static void Reduce(ref long numerator, ref long denominator)
        {
            long gcd = GreatestCommonDivisor(numerator, denominator);
            if (gcd == 0)
            {
                return;
            }

            numerator /= gcd;
            denominator /= gcd;

            if (denominator < 0)
            {
                numerator *= -1;
                denominator *= -1;
            }
        }


        public static Fraction operator +(Fraction a, Fraction b)
        {
            long lcm = LeastCommonMultiple(a.Denominator, b.Denominator);
            return new Fraction(a.Numerator * (lcm / a.Denominator) + b.Numerator * (lcm / b.Denominator), lcm);
        }

        public static Fraction operator -(Fraction a, Fraction b)
        {
            return a + (-b);
        }
        
        public static Fraction operator *(Fraction a, Fraction b)
        {
            return new Fraction(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
        }

        public static Fraction operator /(Fraction a, Fraction b)
        {
            if (b.Numerator == 0)
            {
                throw new DivideByZeroException("Can't devide by zero!");
            }
            return new Fraction(a.Numerator * b.Denominator, a.Denominator * b.Numerator);
        }

        public static Fraction operator -(Fraction a)
        {
            return new Fraction(-a.Numerator, a.Denominator);
        }

        public static Fraction operator +(Fraction a)
        {
            return new Fraction(Math.Abs(a.Numerator), a.Denominator);
        }


        public static bool operator >(Fraction a, Fraction b) =>
            a.CompareTo(b) == 1;

        public static bool operator >=(Fraction a, Fraction b) =>
            a.CompareTo(b) >= 0;

        public static bool operator <=(Fraction a, Fraction b) =>
            a.CompareTo(b) <= 0;

        public static bool operator <(Fraction a, Fraction b) =>
            a.CompareTo(b) == -1;

        public static bool operator ==(Fraction a, Fraction b) =>
            a.CompareTo(b) == 0;

        public static bool operator !=(Fraction a, Fraction b) =>
            a.CompareTo(b) != 0;

        public static bool operator true(Fraction a) =>
            a.Numerator != 0;

        public static bool operator false(Fraction a) =>
            a.Numerator == 0;



        public static explicit operator sbyte(Fraction a) =>
            (sbyte)(double)a;

        public static explicit operator byte(Fraction a) =>
            (byte)(double)a;

        public static explicit operator short(Fraction a) =>
            (short)(double)a;

        public static explicit operator ushort(Fraction a) =>
            (ushort)(double)a;

        public static explicit operator int(Fraction a) =>
            (int)(double)a;

        public static explicit operator uint(Fraction a) =>
            (uint)(double)a;

        public static explicit operator long(Fraction a) =>
            a.Numerator / a.Denominator;

        public static explicit operator ulong(Fraction a) =>
            (ulong)(double)a;

        public static explicit operator double(Fraction a) =>
            (double)a.Numerator / a.Denominator;

        public static explicit operator float(Fraction a) =>
            (float)a.Numerator / a.Denominator;

        public static explicit operator decimal(Fraction a) =>
            (decimal)a.Numerator / a.Denominator;



        public override string ToString()
        {
            return ToString("FractionLike", CultureInfo.CurrentCulture);
        }

        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (String.IsNullOrEmpty(format)) format = "FractionLike";
            if (provider == null) provider = CultureInfo.CurrentCulture;

            switch (format)
            {
                case "FractionLike":
                    return Numerator.ToString() + "/" + Denominator.ToString();
                case "DoubleLike":
                    return ((double)this).ToString("e15", provider);
                case "DecimalLike":
                    return ((decimal)this).ToString(provider);
                default:
                    throw new FormatException(String.Format("The {0} format string is not supported.", format));
            }
        }

        public static bool TryParse(string number, out Fraction result)
        {
            int sign = 1;
            if (number[0] == '-')
            {
                sign = -1;
                number = number[1..];
            }

            Regex fractionLike = new Regex(@"^(\d+)/(\d+)$");
            Regex doubleLike = new Regex(@"^(\d)[\.|\,](\d+)['e'|'E']['+'|'\-'](\d+)$");
            Regex decimalLike = new Regex(@"^(\d+)[\.|\,](\d+)$");
            Regex longLike = new Regex(@"^(\d+)$");

            if (fractionLike.IsMatch(number))
            {
                int indexOfSlash = number.IndexOf("/");
                long num = long.Parse(number.Substring(0, indexOfSlash));
                long den = long.Parse(number.Substring(indexOfSlash + 1, number.Length - indexOfSlash - 1));
                if (den == 0)
                {
                    throw new DivideByZeroException("Can't devide by zero!");
                }
                result = new Fraction(sign * num, den);
                return true;
            }

            if (doubleLike.IsMatch(number))
            {
                int ePos = number.ToLowerInvariant().IndexOf("e");

                long num = long.Parse(number[0] + number[2..ePos]);
                long pow10 = -(ePos - 2) + (number[ePos + 1] == '-' ? -1 : +1) * long.Parse(number[(ePos + 2)..]);

                if (pow10 < 0)
                {
                    result = new Fraction(sign * num, BinPow(10, -pow10));
                } 
                else
                {
                    result = new Fraction(sign * num * BinPow(10, pow10), 1);
                }
                return true;
            }

            if (decimalLike.IsMatch(number))
            {
                int pointPos = number.IndexOf('.');
                if (pointPos == -1)
                    pointPos = number.IndexOf(',');
                long num = long.Parse(number.Substring(0, pointPos) + number[(pointPos + 1)..]);
                
                result = new Fraction(sign * num, BinPow(10, number.Length - pointPos - 1));
                return true;
            }

            if (longLike.IsMatch(number))
            {
                result = new Fraction(sign * long.Parse(number), 1);
                return true;
            }

            result = null;
            return false;
        }

        public int CompareTo(Fraction other)
        {
            long lcm = LeastCommonMultiple(Denominator, other.Denominator);
            if (lcm / Denominator * Numerator > lcm / other.Denominator * other.Numerator)
                return 1;
            else if (lcm / Denominator * Numerator < lcm / other.Denominator * other.Numerator)
                return -1;
            else return 0;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null || !(obj is Fraction))
                return false;
            else
                return CompareTo((Fraction)obj) == 0;
        }

        public override int GetHashCode()
        {
            return (int)((Numerator ^ Denominator) % (1000000000 + 7));
        }

        public bool Equals(Fraction other)
        {
            return CompareTo(other) == 0;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        static long GreatestCommonDivisor(long a, long b)
        {
            return b == 0 ? a : GreatestCommonDivisor(b, a % b);
        }

        static long LeastCommonMultiple(long a, long b)
        {
            return a / GreatestCommonDivisor(a, b) * b;
        }

        static long BinPow(long a, long b)
        {
            long res = 1;
            while (b > 0) {
                if ((b & 1) > 0)
                {
                    res *= a;
                    b--;
                }
                else
                {
                    a *= a;
                    b >>= 1;
                }
            }
            return res;
        }

    }
}
