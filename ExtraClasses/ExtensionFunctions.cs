//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace OSS_Domain
//{
//    public static class ExtensionFunctions
//    {
//        public static string SetStringValue(this string value,string NewValue,int Index,int Length)
//        {
//            if (value == null)
//                value = string.Empty;

//            string result = string.Empty;
//            NewValue = NewValue.PadRight(Length, ' ');
//            for (int i = 0; i < Math.Max(value.Length,Index+Length); i++)
//            {
//                if(i>=Index && i < Index + Length)
//                {
//                    result += NewValue[i-Index];
//                }
//                else
//                {
//                    result+= i<value.Length?value[i]:' ';
//                }
//            }

//            return result;
//        }
//        public static string GetStringValue(this string value, int Index, int Length)
//        {
//            if (value == null)
//                value = string.Empty;
//            StringBuilder SB = new StringBuilder();
//            for (int i = Index; i < Index + Length && i < value.Length; i++)
//            {
//                SB.Append(value[i]);
//            }
//            return SB.ToString();
//        }

//        //public static RT GetValue<IT,RT>(this IT value, int Index, int Length, bool Signed)
//        //{
//        //    StringBuilder SBValue = new StringBuilder();

//        //    if (Signed)
//        //    {
//        //        if (value.ToString().StartsWith("-"))
//        //            SBValue.Append('-');
//        //        else
//        //            SBValue.Append('+');
//        //        Length = Length - 1;
//        //    }

//        //    if (typeof(IT) == typeof(string))
//        //        SBValue.Append(value?.ToString().PadRight(Length, ' '));
//        //    else
//        //        SBValue.Append(value?.ToString().PadLeft(Length, '0'));
//        //    string NewValue = SBValue.ToString().GetStringValue(Index, Length);
//        //    if (string.IsNullOrWhiteSpace(NewValue) && typeof(RT)!=typeof(string))
//        //    {
//        //        NewValue = "0";
//        //    }
//        //    return (RT)Convert.ChangeType(NewValue, typeof(RT));
//        //}
//        //public static double GetValue<IT>(this IT value, int Index, int LPrePoint,int LPostPoint, bool Signed)
//        //{
//        //    int Length = LPrePoint + LPostPoint;
//        //    string StrValue = (value == null ? string.Empty : value.ToString()).GetValue<string,string>(Index, Length, Signed);
//        //    double Value = Convert.ToDouble($"{StrValue.Substring(0, LPrePoint)}.{StrValue.Substring(LPostPoint)}");
//        //    return Signed ? Value : Math.Abs(Value);
//        //}
//        //public static RT SetValue<IT,RT>(this IT source,double value, int Index, int LPrePoint, int LPostPoint, bool Signed)
//        //{
//        //    int Length = LPrePoint + LPostPoint;
//        //    string[] Tokens = value.ToString().Split('.');
//        //    long PrePoint = Convert.ToInt64(Tokens[0]);
//        //    long PostPoint = Tokens.Length > 1 ? Convert.ToInt64(Tokens[1]) : 0;

//        //    StringBuilder SB = new StringBuilder();
//        //    if (Signed)
//        //    {
//        //        SB.Append($"{(PrePoint < 0 ? "-" : "+")}");
//        //        SB.Append(Math.Abs(PrePoint).ToString().PadLeft(LPrePoint - 1, '0'));
//        //    }
//        //    SB.Append(PrePoint.ToString().PadLeft(LPrePoint, '0'));
//        //    SB.Append(PostPoint.ToString().PadLeft(LPostPoint, '0'));

//        //    return (RT)Convert.ChangeType((source == null ? string.Empty : source.ToString()).SetStringValue(SB.ToString(), Index, Length), typeof(RT));
//        //}

//        //public static RT SetValue<ST, IT, RT>(this ST lvalue, IT value, int Index, int Length,bool Signed)
//        //{
//        //    StringBuilder SBValue = new StringBuilder();

//        //    if (Signed)
//        //    {
//        //        if (value.ToString().StartsWith("-"))
//        //            SBValue.Append('-');
//        //        else
//        //            SBValue.Append('+');
//        //        Length = Length - 1;
//        //    }

//        //    if (typeof(IT) == typeof(string))
//        //        SBValue.Append(value?.ToString().PadRight(Length, ' '));
//        //    else
//        //        SBValue.Append(value?.ToString().PadLeft(Length, '0'));
//        //    string source = lvalue == null ? string.Empty : lvalue.ToString();
//        //    return (RT)Convert.ChangeType(source.SetStringValue(SBValue.ToString(), Index, Length), typeof(RT));
//        //}

//        public static long GetLongValue(this string value, int Index, int Length, bool IsSigned)
//        {
//            string StrValue = value.GetStringValue(Index, Length);
//            long Value = string.IsNullOrWhiteSpace(StrValue) ?0:Convert.ToInt64(StrValue);
//            return IsSigned ? Value : Math.Abs(Value);
//        }
//        public static double GetDoubleValue(this string value, int Index, int LengthPrePoint, int LengthPostPoint, bool IsSigned)
//        {
//            int Length = LengthPrePoint + LengthPostPoint;
//            string StrValue = value.GetStringValue(Index, Length);
//            double Value = Convert.ToDouble($"{StrValue.Substring(0, LengthPrePoint)}.{StrValue.Substring(LengthPrePoint)}");
//            return IsSigned ? Value : Math.Abs(Value);
//        }
//        public static string SetDoubleValue(this string source, double value, int Index, int LengthPrePoint, int LengthPostPoint, bool IsSigned)
//        {
//            string[] Tokens = value.ToString().Split('.');
//            long PrePoint = Convert.ToInt64(Tokens[0]);
//            long PostPoint = Tokens.Length > 1 ? Convert.ToInt64(Tokens[1]) : 0;

//            StringBuilder SB = new StringBuilder();
//            if (IsSigned)
//            {
//                SB.Append($"{(PrePoint < 0 ? "-" : "+")}");
//                SB.Append(Math.Abs(PrePoint).ToString().PadLeft(LengthPrePoint - 1, '0'));
//            }
//            SB.Append(PrePoint.ToString().PadLeft(LengthPrePoint, '0'));
//            SB.Append(PostPoint.ToString().PadLeft(LengthPostPoint, '0'));
//            return source.SetStringValue(SB.ToString(), Index, LengthPrePoint + LengthPostPoint);
//        }
//        public static string SetLongValue(this string source, long value, int Index, int Length, bool IsSigned)
//        {         
//            StringBuilder SB = new StringBuilder();
//            if (IsSigned)
//            {
//                SB.Append($"{(value < 0 ? "-" : "+")}");
//                SB.Append(Math.Abs(value).ToString().PadLeft(Length - 1, '0'));
//            }
//            else
//                SB.Append(value.ToString().PadLeft(Length, '0'));
//            return source.SetStringValue(SB.ToString(), Index, Length);
//        }
//        //public static long Abs(this long value)
//        //{
//        //    return Math.Abs(value);
//        //}
//        //public static double Abs(this double value)
//        //{
//        //    return Math.Abs(value);
//        //}


//        //public static string SetStringValue(this string value, int StartIndex, int Count)
//        //{
//        //    StringBuilder SB = new StringBuilder();
//        //    for (int i = StartIndex; i < StartIndex + Count && i < value.Length; i++)
//        //    {
//        //        SB.Append(value[i]);
//        //    }
//        //    return SB.ToString();
//        //}
//        //public static long GetNumericValue(this string value,int StartIndex,int Count)
//        //{
//        //    string StrValue = value.GetStringValue(StartIndex, Count);
//        //    StrValue = string.IsNullOrEmpty(StrValue) ? "0" : StrValue;
//        //    return long.Parse(StrValue);
//        //}
//        //public static double GetDoubleValue(this string value, int StartIndex, int SizePrePoint, int SizePostPoint)
//        //{
//        //    string PrePoint = value.GetStringValue(StartIndex, SizePrePoint);
//        //    string PostPoint = value.GetStringValue(StartIndex+ SizePrePoint, SizePostPoint);

//        //    PrePoint = string.IsNullOrEmpty(PrePoint) ? "0" : PrePoint;
//        //    PostPoint = string.IsNullOrEmpty(PostPoint) ? "0" : PostPoint;

//        //    return double.Parse($"{PrePoint.PadLeft(SizePrePoint, '0')}.{PostPoint.PadLeft(SizePostPoint, '0')}");
//        //}
//    }
//}
