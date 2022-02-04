using System;
using System.Collections.Generic;

namespace MonitorCommon;

public class ComparisonComparer<T> : Comparer<T>
{
    private readonly Comparison<T> comparison;

    public ComparisonComparer(Comparison<T> comparison) => this.comparison = comparison;

    public override int Compare(T? x, T? y) => comparison(x!, y!); // CLR does it like this...
}