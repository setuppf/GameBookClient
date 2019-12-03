using System;

namespace GEngine
{
    public class Util
    {
        public static T ChangeType<T>(object obj, T t)
        {
            return (T)obj;
        }

        public static T ChangeToEnum<T>(string value) where T : struct
        {
            if (!Enum.TryParse(value, out T colorValue))
            {
                return default(T);
            }

            if (!Enum.IsDefined(typeof(T), colorValue))
                return default(T);

            return colorValue;
        }

        public static T ChangeToEnum<T>(int value) where T : struct
        {
            return ChangeToEnum<T>(value.ToString());
        }
    }
}

