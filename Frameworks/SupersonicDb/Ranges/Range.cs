using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Supersonic.Ranges;

public class Range
{
    #region Constructors
    public Range()
    {
        _simpleRanges = [];
    }
    public Range(params SimpleRange[] simpleRanges)
    {
        var list = simpleRanges.Where(x => !x.IsEmpty).ToList();
        var orderedList = list.OrderBy(x => x.StartIdx).ToList();
        var resultList = OrderAndRemoveIntersectingSections(orderedList);
        _simpleRanges = resultList.ToArray();
    }
    #endregion

    #region Overrides
    public override string ToString()
    {
        if (_simpleRanges.Length == 0) return "[empty]";

        var sb = new StringBuilder();

        bool first = true;
        foreach (var simpleRange in _simpleRanges)
        {
            if (first)
            {
                first = false;
                sb.Append(simpleRange);
            }
            else
            {
                sb.Append(", " + simpleRange);
            }
        }

        return sb.ToString();
    }
    #endregion

    #region Methods
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator +(Range a, SimpleRange b)
    {
        return a.Union(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator +(SimpleRange b, Range a)
    {
        return a.Union(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator *(Range a, SimpleRange b)
    {
        return a.Intersect(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator *(SimpleRange b, Range a)
    {
        return a.Intersect(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator + (Range a, Range b)
    {
        return a.Union(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator * (Range a, Range b)
    {
        return a.Intersect(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Range operator ! (Range r)
    {
        return r.Invert();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Range Union(SimpleRange other)
    {
        var list = _simpleRanges.ToList();
        list.Add(other);
        var orderedList = list.OrderBy(x => x.StartIdx).ToList();
        var resultList = OrderAndRemoveIntersectingSections(orderedList);
        return new Range(resultList.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Range Intersect(SimpleRange other)
    {
        //(A1 + A2) * B = A1*B + A2*B
        var totalRange = new Range();
        foreach (var simpleRange in _simpleRanges)
        {
            totalRange = totalRange + simpleRange*other;
        }
        return totalRange;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Range Union(Range other)
    {
        var list = _simpleRanges.ToList();
        list.AddRange(other._simpleRanges);
        var orderedList = list.OrderBy(x => x.StartIdx).ToList();
        var resultList = OrderAndRemoveIntersectingSections(orderedList);
        return new Range(resultList.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Range Intersect(Range other)
    {
        var list = new List<SimpleRange>();
        foreach (var mySimpleRange in _simpleRanges)
        {
            foreach (var otherSimpleRange in other._simpleRanges)
            {
                var intersect = mySimpleRange * otherSimpleRange;
                if (!intersect.IsEmpty) list.Add(intersect._simpleRanges.Single());
            }
        }
        var orderedList = list.OrderBy(x => x.StartIdx).ToList();
        var resultList = OrderAndRemoveIntersectingSections(orderedList);
        return new Range(resultList.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Range Invert()
    {
        var newSimpleRanges = new List<SimpleRange>();
        for (var i = 0; i < _simpleRanges.Length; i++)
        {
            var range = _simpleRanges[i];
            if (i == 0)
            {
                if (range.StartIdx != 0) newSimpleRanges.Add(new SimpleRange(0, range.StartIdx - 1));
            }
            else
            {
                var prevRange = _simpleRanges[i - 1];
                newSimpleRanges.Add(new SimpleRange(prevRange.EndIdx + 1, range.StartIdx - 1));
            }
        }

        //We use int.MaxValue here as imnfinity
        var lastRange = _simpleRanges[_simpleRanges.Length - 1];
        if (lastRange.EndIdx != int.MaxValue) newSimpleRanges.Add(new SimpleRange(lastRange.EndIdx + 1, int.MaxValue));

        return new Range(newSimpleRanges.ToArray());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Size(int listCount)
    {
        var count = 0;
        foreach (var simpleRange in SimpleRanges) count += simpleRange.Size(listCount);
        return count;
    }
    #endregion

    #region Helper Methods
    protected static List<SimpleRange> OrderAndRemoveIntersectingSections(List<SimpleRange> orderedList, int startAt = 0)
    {
        for (var i = startAt; i < orderedList.Count - 1; i++)
        {
            if (orderedList[i].EndIdx >= orderedList[i + 1].StartIdx - 1)
            {
                if (orderedList[i].EndIdx >= orderedList[i + 1].EndIdx)
                {
                    //if second is subset of first
                    orderedList.RemoveAt(i + 1);
                    return OrderAndRemoveIntersectingSections(orderedList, i);
                }
                else
                {
                    //if we need to combine them
                    orderedList[i] = new SimpleRange(orderedList[i].StartIdx, orderedList[i + 1].EndIdx);
                    orderedList.RemoveAt(i + 1);
                    return OrderAndRemoveIntersectingSections(orderedList, i);
                }
            }
        }
        return orderedList;
    }
    #endregion

    #region Properties
    public int SimpleRangesLength => _simpleRanges.Length;
    public bool IsEmpty => !_simpleRanges.Any();
    public SimpleRange[] SimpleRanges => _simpleRanges.ToArray();
    private readonly SimpleRange[] _simpleRanges;
    #endregion
}