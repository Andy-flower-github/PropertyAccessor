using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Andy.Utilities
{
    /// <summary>
    /// Andy -> 簡化型態方便處理 Character, Numeric, Logic, Date, Class
    /// </summary>
    public enum MySimpleTypes { String, Numeric, Logic, Date, Object }

    public static class PrimitiveTypeExtras
    {
        /// <summary>
        /// Andy -> 針對 Null 值, 取出真正的型別
        /// </summary>
        public static Type GetUnderlyingType(this Type sender)
            => Nullable.GetUnderlyingType(sender) ?? sender;

        private static IList<Type> NumericTypes = new Type[] { typeof(int), typeof(long), typeof(float), typeof(decimal), typeof(double) };

        /// <summary>
        /// Andy =>判斷 Type 是不是數值的型態. int,long,float,decimal,double
        /// </summary>
        public static bool IsNumericType(this Type sender) => NumericTypes.Contains(sender);

        /// <summary>
        /// Andy -> 簡化型態方便處理
        /// </summary>
        public static MySimpleTypes GetMySimpleType(this object sender)
        {
            switch (Type.GetTypeCode(sender.GetType()))
            {
                case TypeCode.String:
                    return MySimpleTypes.String;
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return MySimpleTypes.Numeric;
                case TypeCode.DateTime:
                    return MySimpleTypes.Date;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                case TypeCode.Object:
                    return MySimpleTypes.Object;
                case TypeCode.Boolean:
                    return MySimpleTypes.Logic;
                default:
                    throw new Exception($"未定義的 TypeCode: <{Type.GetTypeCode(sender.GetType())}>!!! 請通知MIS加入判斷...");
            }
        }

        /// <summary>
        /// Andy -> 把日期變成字串傳回 {2020/12/31} => "1091231" 
        /// </summary>
        public static string ToShortTaiwanDateString(this DateTime sender)
            => (sender.Year - 1911).ToString("D3") + sender.ToString("MMdd");

        /// <summary>
        /// Andy -> 把日期變成字串傳回 {2020/12/31} => "109/12/31" 
        /// </summary>
        public static string ToTaiwanDateString(this DateTime sender)
            => (sender.Year - 1911).ToString("D3") + sender.ToString("/MM/dd");

        /// <summary>
        /// Andy -> 傳回日期區間格式. 2020/01/01 to 2020/01/31
        /// </summary>
        public static string ParseToRangeText(this DateTime? sender, DateTime? enddate, string dateformat = "yyyy/MM/dd")
        {
            string date1 = sender == null ? "" : sender.Value.ToString(dateformat);
            string date2 = enddate == null ? "" : enddate.Value.ToString(dateformat);
            return $"{date1} to {date2}";
        }

        /// <summary>
        /// Andy -> 傳回文字日期區間格式. 1/1 ~ 1/31
        /// </summary>
        public static string ParseToPeriodDate(this DateTime? sender, DateTime? enddate, string dateformat = "yyyy/MM/dd")
        {
            string date1 = sender == null ? "" : sender.Value.ToString(dateformat);
            string date2 = enddate == null ? "" : enddate.Value.ToString(dateformat);
            return date1 == date2 ? date1 : $"{date1} ~ {date2}";
        }

        /// <summary>
        /// Andy -> 傳回文字日期區間格式. 1/1 ~ 1/31
        /// </summary>
        public static string ParseToPeriodDate(this DateTime sender, DateTime? enddate, string dateformat = "yyyy/MM/dd")
        {
            string date1 = sender.ToString(dateformat);
            string date2 = enddate == null ? "" : enddate.Value.ToString(dateformat);
            return date1 == date2 ? date1 : $"{date1} ~ {date2}";
        }
        /// <summary>
        /// Andy -> 傳回字串區間格式. A001 to A005
        /// </summary>
        public static string ParseToRangeText(this string sender, string endtext)
        {
            if (string.IsNullOrWhiteSpace(sender) && string.IsNullOrWhiteSpace(endtext))
                return "(All)";

            return $"{sender ?? ""} to {endtext ?? ""}";
        }

        /// <summary>
        /// Andy -> 把 Enum 變成 IDictionary
        /// </summary>
        public static IDictionary<int, string> ParseToDictionary<T>(this Enum sender)
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(x => (int)(object) x, x => x.ToString());
        }

        /// <summary>
        /// Andy -> "5.1MM * 6.1MM" => "5.1*6.1"
        /// </summary>
        public static string GetDigitString(this string sender)
        {
            string regex = @"[\d+\-*/.,]";
            return Regex.Matches(sender, regex).Cast<Match>().Select(x => x.Value).JoinWith("");
        }

        public static string Replicate(this string sender, int times)
            => Enumerable.Range(1, times).Select(x => sender).JoinWith("");

        /// <summary>
        /// Andy ->  "1+2" = 3, "3*2+1" = 7 , 失敗回傳  0, 'aa'>='bb' = FALSE
        /// </summary>
        public static object ComputeA(this string sender)
        {
            if (sender.NoValue()) return 0;
            object resultVar;
            try
            {
                resultVar = (new DataTable()).Compute(sender, "");
            }
            catch
            {
                resultVar = 0;
            }
            return resultVar;
        }

        /// <summary>
        /// Andy ->  "1+2" = 3, "3*2+1" = 7 , 失敗回傳  0
        /// </summary>
        public static decimal ComputeToDecimal(this string sender)
        {
            if (sender.NoValue()) return 0;
            var obj = sender.ComputeA() ?? "";
            decimal value = 0;
            decimal.TryParse(obj.ToString(), out value);
            return value;
        }

        /// <summary>
        /// Andy -> 傳回指定的四捨五入. decimal 型別
        /// </summary>
        public static decimal ToRound(this decimal sender, int decimals)
            => decimal.Round(sender, decimals, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Andy -> 傳回指定的四捨五入. decimal 型別
        /// </summary>
        public static decimal ToRound(this decimal? sender, int decimals)
            => decimal.Round(sender ?? 0, decimals, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Andy -> 傳回字串是否介於指定的字串區間
        /// </summary>
        public static bool Between(this string sender, string start, string end)
        {
            string expression = $"'{sender}'>='{start}' and '{sender}'<='{end}'";
            return expression.ComputeA().ParseToBool();
        }

        /// <summary>
        /// Andy -> 傳回日期是否介於指定的日期區間
        /// </summary>
        public static bool Between(this DateTime sender, DateTime? start, DateTime? end)
            => sender >= start && sender <= end;

        /// <summary>
        /// Andy -> 傳回日期是否介於指定的日期區間
        /// </summary>
        public static bool Between(this DateTime? sender, DateTime? start, DateTime? end)
            => sender >= start && sender <= end;

        /// <summary>
        /// Andy -> 傳回 int 是否介於指定的 int 區間
        /// </summary>
        public static bool Between(this int sender, int start, int end)
            => sender >= start && sender <= end;

        /// <summary>
        /// Andy -> 傳回 long 是否介於指定的 long 區間
        /// </summary>
        public static bool Between(this long sender, long start, long end)
            => sender >= start && sender <= end;

        /// <summary>
        /// Andy -> 傳回 decimal 是否介於指定的 decimal 區間
        /// </summary>
        public static bool Between(this decimal sender, decimal start, decimal end)
            => sender >= start && sender <= end;


        /// <summary>
        /// Andy -> 將數字變成字串 1->A, 2->B, 26->Z, 27->AA, 28->AB
        /// </summary>
        public static string ToExcelColumnName(this int sender)
        {
            if (sender <= 26)
                return "" + (char)(64 + sender);
            int intNum = (sender - 1) / 26;                  // 求整數
            int modNum = (sender - 1) % 26 + 1;               // 求餘數
            return "" + (char)(intNum + 64) + (char)(modNum + 64);
        }

        /// <summary>
        /// Andy -> 求這個月有幾天 DateTime.DaysInMonth()
        /// </summary>
        public static int DaysInMonth(this DateTime sender)
            => DateTime.DaysInMonth(sender.Year, sender.Month);

        /// <summary>
        /// Andy -> 求這個月的第一天
        /// </summary>
        public static DateTime FirstDateInMonth(this DateTime sender)
            => new DateTime(sender.Year, sender.Month, 1);

        /// <summary>
        /// Andy -> 求這個月的第一天
        /// </summary>
        public static DateTime FirstDateInLastMonth(this DateTime sender)
            => sender.AddMonths(-1).FirstDateInMonth();

        /// <summary>
        /// Andy -> 求今年的第一天
        /// </summary>
        public static DateTime FirstDateInYear(this DateTime sender)
            => new DateTime(sender.Year, 1, 1);

        /// <summary>
        /// Andy -> 求這個月的最後一天
        /// </summary>
        public static DateTime LastDateInMonth(this DateTime sender)
            => new DateTime(sender.Year, sender.Month, DateTime.DaysInMonth(sender.Year, sender.Month));

        /// <summary>
        /// Andy -> 求上個月的最後一天
        /// </summary>
        public static DateTime LastDateInLastMonth(this DateTime sender)
            => sender.AddMonths(-1).LastDateInMonth();

        /// <summary>
        /// Andy -> 簡化 Split 的語法, 會傳回 sender.Split(new string[] {splitString}, StringSplitOptions.None)
        /// </summary>
        public static string[] Split(this string sender, string splitString)
            => sender.Split(new string[] { splitString }, StringSplitOptions.None);

        /// <summary>
        /// Andy -> int to string => 1 => "0001", -1 => "-001". 常用在媒體申報的數字轉文字
        /// </summary>
        public static string ToMediaNumber(this int num, int totalWidth)
            => num >= 0 ? num.ToString($"D{totalWidth}") : "-" + (num * -1).ToString($"D{totalWidth - 1}");

        /// <summary>
        /// Andy -> int to string => 1.=> "000100", -1 => "-00100". 常用在媒體申報的數字轉文字(含小數點)
        /// </summary>
        public static string ToMediaNumber(this decimal num, int intWidth, int decimalWidth)
        {
            int integer = (int)Math.Truncate(num);
            string intString = integer.ToMediaNumber(intWidth);
            decimal deci = Math.Abs(num - integer);
            var decitoint = deci * Math.Pow(10, decimalWidth).ParseToDecimal();
            string decimamlString = decitoint.ParseToString().PadRightBytes(decimalWidth, '0');
            return intString + decimamlString;
        }

        public static string GetPrefixNumberString(this string text)
        {
            string result = "";
            foreach (char item in text)
            {
                if (Char.IsDigit(item))
                    result += item;
                else
                    break;
            }
            return result;
        }

        /// <summary>
        /// Andy -> 求字串的長度. 以bytes 為單位 , 中文二個, 英文一個
        /// </summary>
        public static int GetBytesLength(this string sender)
            => Encoding.GetEncoding(950).GetBytes(sender).Length;

        /// <summary>
        /// Andy -> 將字串(右方)補滿指定的長度. 以bytes 為單位 , 中文二個, 英文一個, 一般用於製作媒體檔
        /// </summary>
        public static string PadRightBytes(this string sender, int maxlength, Char padchar = ' ')
        {
            string padstring = string.Empty;
            for (int i = 0; i < sender.Length; i++)
            {
                string newstring = padstring + sender.Substring(i, 1);
                if (newstring.GetBytesLength() == maxlength)
                {
                    padstring = newstring;
                    break;
                }
                else if (newstring.GetBytesLength() > maxlength)
                {
                    break;
                }
                else
                {
                    padstring = newstring;
                }
            }

            if (padstring.GetBytesLength() < maxlength)
            {
                padstring += string.Empty.PadRight(maxlength - padstring.GetBytesLength(), padchar);
            }
            return padstring;
        }

        /// <summary>
        /// Andy -> 將字串(左方)補滿指定的長度. 以bytes 為單位 , 中文二個, 英文一個. 一般用於製作媒體檔
        /// </summary>
        public static string PadLeftBytes(this string sender, int maxlength, Char padchar = ' ')
        {
            string padstring = string.Empty;
            for (int i = 0; i < sender.Length; i++)
            {
                string newstring = padstring + sender.Substring(i, 1);
                if (newstring.GetBytesLength() == maxlength)
                {
                    padstring = newstring;
                    break;
                }
                else if (newstring.GetBytesLength() > maxlength)
                {
                    break;
                }
                else
                {
                    padstring = newstring;
                }
            }

            if (padstring.GetBytesLength() < maxlength)
            {
                padstring = string.Empty.PadRight(maxlength - padstring.GetBytesLength(), padchar) + padstring; 
            }
            return padstring;
        }

        /// <summary>
        /// Andy -> 取字串左邊的前 xx 碼. 若字串長度不足. 則傳回整個字串
        /// </summary>
        public static string Left(this string sender, int getlength)
            => sender.Length > getlength ? sender.Substring(0, getlength) : sender;

        /// <summary>
        /// Andy -> 取字串右邊的前 xx 碼, 若字串長度不足. 則傳回整個字串
        /// </summary>
        public static string Right(this string sender, int getlength)
            => sender.Length > getlength ? sender.Substring(sender.Length - getlength, getlength) : sender;

        /// <summary>
        /// Andy -> 由兩個值中. 取不是空白或是Null的值.
        /// </summary>
        public static string Evl(this string text1, string text2)
            => text1.HasValue() ? text1 : text2.Evl("");

        /// <summary>
        /// Andy -> 由兩個值中. 取不是Null的值.
        /// </summary>
        public static DateTime? Evl(this DateTime? date1, DateTime? date2)
            => date1 ?? date2;

        /// <summary>
        /// Andy -> 判斷字串是否存在, 後面指定的字串
        /// </summary>
        public static bool Inlist(this string sender, params string[] strings)
            => strings.Contains(sender);

        /// <summary>
        /// Andy -> 求字串中. 指定文字出現的次數
        /// </summary>
        public static int RepeatCount(this string text, string search)
            => Regex.Matches(text, search).Count;

        public static string ExceptInvalidChar(this string text)
            => ExceptInvalidChar(text, Path.GetInvalidFileNameChars()).Replace("$", "");

        public static string ExceptInvalidChar(this string text, params char[] exceptchars)
            => text.AsEnumerable().Except(exceptchars).JoinWith("");

        /// <summary>
        /// Andy -> string.IsNullOrWhiteSpace(text)
        /// </summary>
        public static bool NoValue(this string text) => string.IsNullOrWhiteSpace(text);

        /// <summary>
        /// Andy -> !string.IsNullOrWhiteSpace(text)
        /// </summary>
        public static bool HasValue(this string text) => !string.IsNullOrWhiteSpace(text);

        public static bool HaveInvalidChar(this string text)
            => text.AsEnumerable().Intersect(Path.GetInvalidFileNameChars()).Count() > 0;

        /// <summary>
        /// Andy -> 判斷整數是否存在, 後面指定的整數
        /// </summary>
        public static bool Inlist(this int sender, params int[] ints)
            => ints.Contains(sender);

        /// <summary>
        /// Andy -> 將 object 還原成原型別. 若轉型失敗則傳回 該型別的 default值 
        /// </summary>
        public static T Unboxing<T>(this object sender)
        {
            try
            {
                return (T)sender;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Andy => 根據型別回傳初值
        /// </summary>
        public static object MyDefault(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Char:
                case TypeCode.String:
                case TypeCode.Empty:
                    return string.Empty;
                case TypeCode.Object:
                    return null;
                case TypeCode.DBNull:
                    return DBNull.Value;
                case TypeCode.Boolean:
                    return false;
                case TypeCode.SByte:
                    return (sbyte)0;
                case TypeCode.Byte:
                    return (byte)0;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return (int)0;
                case TypeCode.Double:
                case TypeCode.Single:
                    return (float)0;
                case TypeCode.Decimal:
                    return (decimal)0;
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return (uint)0;
                case TypeCode.DateTime:
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Andy -> 判斷物件值是為 Primitive, 若物件值本身為 Nullable , 則判斷物其 UnderlyingType
        /// </summary>
        public static bool IsNullablePrimitive(this Type sender)
        {
            var ti = sender.GetTypeInfo();
            var isNullable = ti.IsGenericType && sender.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(sender);
                return IsPrimitiveType(underlyingType);
            }
            return isNullable;
        }

        /// <summary>
        /// Andy -> 判斷物件是不是基本型別物件
        /// </summary>
        public static bool IsPrimitiveType(this Type sender)
        {
            var ti = sender.GetTypeInfo();
            var isPrimitive = ti.IsPrimitive || ti.IsValueType || sender == typeof(string) || sender == typeof(decimal);
            return isPrimitive;
        }

        /// <summary>
        /// Andy -> 將一維陣列的值, 寫入二維陣列的指定AXIS
        /// </summary>
        public static void SetValues<T>(this T[,] _2darray, T[] _1darray, int axis0)
        {
            for (int i = 0; i < _1darray.Length; i++)
            {
                _2darray[axis0, i] = _1darray[i];
            }
        }

        public static R SafeGetValue<T, R>(this IDictionary<T, R> dic, T key)
        {
            if (dic.TryGetValue(key, out R value))
            {
                return value;
            }
            else
            {
                return default(R);
            }
        }
    }
}
