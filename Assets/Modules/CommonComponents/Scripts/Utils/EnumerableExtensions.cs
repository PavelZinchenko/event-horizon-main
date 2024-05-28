using System;
using System.Linq;
using System.Collections.Generic;

public static class EnumerableExtension
{
	public static T MinValue<T,TResult>(this IEnumerable<T> collection, Func<T, TResult> predicate)
		where TResult: IComparable
	{
		TResult minValue = default(TResult);
		T currentItem = default(T);
		bool inited = false;
		foreach (var item in collection)
		{
			var value = predicate(item);
			if (!inited || value.CompareTo(minValue) < 0)
			{
				inited = true;
				minValue = value;
				currentItem = item;
			}
		}

		return currentItem;
	}

    public static int FindIndex<T>(this IList<T> source, Predicate<T> match)
    {
        var count = source.Count;
        for (int i = 0; i < count; i++)
        {
            if (match(source[i]))
            {
                return i;
            }
        }
        return -1;
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T value)
    {
        foreach (var item in enumerable)
            yield return item;

        yield return value;
    }

    public static IEnumerable<T> Concat<T>(this T value, IEnumerable<T> enumerable)
    {
        yield return value;

        foreach (var item in enumerable)
            yield return item;
    }

    public static bool Empty<T>(this IList<T> list)
    {
        return list.Count == 0;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T value)
    {
        return IndexOf(list, value, EqualityComparer<T>.Default);
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T value, IEqualityComparer<T> comparer)
    {
        for (int i = 0; i < list.Count; i++)
            if (comparer.Equals(list[i], value))
                return i;

        return -1;
    }

    public static IList<T> Shuffle<T>(this IList<T> list, System.Random random)
	{
		for (int i = 0; i < list.Count; ++i)
		{
			var j = random.Next(list.Count);
			if (i == j)
				continue;

			var temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}

        return list;
    }

    public static IEnumerable<T> OddElements<T>(this IEnumerable<T> collection)
	{
		int index = 0;
		foreach (var item in collection)
			if ((++index & 1) == 0)
				yield return item;
	}		

	public static T RandomElement<T>(this IList<T> collection, System.Random random)
	{
	    var count = collection.Count;
		return count > 0 ? collection[random.Next(count)] : default(T);
	}

    public static T RandomElement<T>(this IEnumerable<T> collection, System.Random random)
    {
        var count = collection.Count();
        return count > 0 ? collection.ElementAt(random.Next(count)) : default(T);
    }

    public static IEnumerable<T> NotNull<T>(this IEnumerable<T> collection) => collection.Where(item => item != null);

    public static IEnumerable<T> IfEmptyThen<T>(this IEnumerable<T> collection, IEnumerable<T> other)
    {
        int count = 0;
        foreach (var item in collection)
        {
            count++;
            yield return item;
        }

        if (count > 0) yield break;

        foreach (var item in other)
            yield return item;
    }

    public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> collection, int count, System.Random random)
	{
        if (count <= 0) return Enumerable.Empty<T>();
        return collection.RandomElements(count, collection.Count(), random);
	}

    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> collection, int requiredElements)
    {
        if (requiredElements <= 0) yield break;
        int count = 0;

        while (true)
        {
            foreach (var item in collection)
            {
                yield return item;

                if (++count == requiredElements) 
                    yield break;
            }

            if (count == 0) // collection empty
                yield break;
        }
    }

    public static IEnumerable<T> RandomElements<T>(this IEnumerable<T> collection, int count, int size, System.Random random)
	{
        if (count <= 0) yield break;

		var itemsLeft = count;
		foreach (var item in collection)
		{
			if (itemsLeft <= 0)
				yield break;

			var itemsCount = itemsLeft;
			for (int i = 0; i < itemsCount; ++i)
			{
				if (random.Next(size) == 0)
				{
					itemsLeft--;
					yield return item;
				}
			}
            
			size--;			
        }
    }
    
    public static IEnumerable<T> RandomUniqueElements<T>(this IEnumerable<T> collection, int count, System.Random random)
	{
		return collection.RandomUniqueElements(count, collection.Count(), random);
	}

	public static IEnumerable<T> RandomUniqueElements<T>(this IEnumerable<T> collection, int count, int size, System.Random random)
	{
		var itemsLeft = count;
		foreach (var item in collection)
		{
			if (itemsLeft <= 0)
				yield break;
			
			if (random.Next(size) < itemsLeft)
			{
				itemsLeft--;
				yield return item;
			}
			size--;
		}
	}

    public static IEnumerable<int> RandomUniqueNumbers(int min, int max, int count, System.Random random)
    {
        var itemsLeft = count;
        var size = max - min + 1;

        for (var i = min; i <= max; ++i)
        {
            if (itemsLeft <= 0)
                yield break;

            if (random.Next(size) < itemsLeft)
            {
                itemsLeft--;
                yield return i;
            }
            size--;
        }
    }

    public static IEnumerable<T> ToEnumerable<T>(this T item)
	{
		yield return item;
	}

    public static T[] Add<T>(this T[] target, T item)
    {
        if (target == null)
        {
            return new[] { item };
        }

        var result = new T[target.Length + 1];
        target.CopyTo(result, 0);
        result[target.Length] = item;
        return result;
    }

    public static T[] AddIfNotExists<T>(this T[] target, T item)
    {
        if (target == null)
        {
            return new[] { item };
        }

        if (Array.IndexOf(target, item) >= 0)
            return target;

        var result = new T[target.Length + 1];
        target.CopyTo(result, 0);
        result[target.Length] = item;
        return result;
    }

    public static T[] RemoveAll<T>(this T[] target, T item) where T : class
    {
        if (target == null)
            return null;

        var index = 0;
        for (var i = 0; i < target.Length; ++i)
        {
            target[index] = target[i];
            if (target[i] != item)
                index++;
        }

        if (index == 0)
            return new T[] {};

        var result = new T[index];
        Array.Copy(target, result, index);
        return result;
    }

    public static void ReplaceAll<T>(this IList<T> list, IEnumerable<T> newValues)
    {
        list.Clear();
        foreach (var value in newValues)
            list.Add(value);
    }

    public static void SetValue<T>(this IList<T> list, T value, int index)
    {
        while (list.Count <= index)
            list.Add(default(T));
        list[index] = value;
    }

    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, Func<TValue,TValue,TValue> mergeFunc)
    {
        TValue oldValue;
        if (!dictionary.TryGetValue(key, out oldValue))
        {
            dictionary.Add(key, value);
            return;
        }

        dictionary[key] = mergeFunc(oldValue, value);
    }

    public static int BoundedSum(this IEnumerable<int> values, int min, int max)
    {
        var sum = 0;
        foreach (var value in values)
        {
            if (value > 0 && sum + value < sum)
                return max;
            if (value < 0 && sum + value > sum)
                return min;

            sum += value;
        }

        return sum;
    }

    public static void QuickRemove<T>(this IList<T> list, int index)
    {
        var count = list.Count;
        if (index < 0 || index >= count)
            return;

        if (index + 1 < count)
            list[index] = list[count - 1];

        list.RemoveAt(count - 1);
    }
}
