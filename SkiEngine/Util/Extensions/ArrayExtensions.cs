using System;
using System.Collections.Generic;

namespace SkiEngine.Util.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] SetItems<T>(this T[] array, Func<T> func)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = func.Invoke();
            }

            return array;
        }

        public static T[] SetItems<T>(this T[] array, IEnumerable<T> enumerable)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                for (var i = 0; i < array.Length; i++)
                {
                    enumerator.MoveNext();
                    array[i] = enumerator.Current;
                }
            }

            return array;
        }

        public static T[,] SetItems<T>(this T[,] array, IEnumerable<T> enumerable)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                var width = array.GetLength(0);
                var height = array.GetLength(1);
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                {
                    enumerator.MoveNext();
                    array[x, y] = enumerator.Current;
                }

                return array;
            }
        }

        public static T[,,] SetItems<T>(this T[,,] array, IEnumerable<T> enumerable)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                var width = array.GetLength(0);
                var height = array.GetLength(1);
                var depth = array.GetLength(2);
                for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                for (var z = 0; z < depth; z++)
                {
                    enumerator.MoveNext();
                    array[x, y, z] = enumerator.Current;
                }

                return array;
            }
        }

        public static IEnumerable<T> Flatten<T>(this T[,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            {
                yield return array[x, y];
            }
        }

        public static IEnumerable<T> Flatten<T>(this T[,,] array)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);
            var depth = array.GetLength(2);
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
            for (var z = 0; z < depth; z++)
            {
                yield return array[x, y, z];
            }
        }
    }
}