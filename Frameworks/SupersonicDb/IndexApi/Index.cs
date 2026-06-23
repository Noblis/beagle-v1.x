using System;
using System.Collections;
using System.Collections.Generic;
// ReSharper disable once RedundantUsingDirective
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Supersonic.BNF;
using Supersonic.GC;
using Supersonic.Linq;
using Supersonic.Ranges;

namespace Supersonic.IndexApi;

internal class Index<TItem> where TItem : class
{
    #region EmbeddedTypes
    internal class IndexComparer<T> : IComparer<T> where T : TItem
    {
        #region Constructors
        public IndexComparer(Index<TItem> index)
        {
            IsIIndexedListItem = typeof(ISupersonicListItem).IsAssignableFrom(typeof(TItem));
            ParentIndex = index;
        }
        #endregion

        #region Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y)
        {
            return ParentIndex.CompareItemAandItemBUsingIndexProperties(x, y, true, out var _);
        }
        #endregion

        #region Properties
        protected bool IsIIndexedListItem { get; }
        protected Index<TItem> ParentIndex { get; }
        #endregion
    }
    #endregion

    #region Constructors
    protected Index(SupersonicList<TItem> parentSupersonicList)
    {
        ParentSupersonicList = parentSupersonicList;
    }
    internal static Index<TItem> CreateFromDefinition(SupersonicList<TItem> parentSupersonicList, IndexDefinition indexDef)
    {
        var indexProperties = new Expression<Func<TItem, object>>[indexDef.Props.Count];
        indexDef.Props = indexDef.Props.OrderBy(x => x.Order).ToList();
        for (var i = 0; i < indexDef.Props.Count; i++)
        {
            var x = Expression.Parameter(typeof(TItem), "x");
            var property = Expression.Property(x, indexDef.Props[i].Name);
            var indexProperty = Expression.Lambda<Func<TItem, object>>(Expression.Convert(property, typeof(object)), x);
            indexProperties[i] = indexProperty;
        }
        return CreateHelper(parentSupersonicList, indexDef.IsUnique, indexDef.Name, indexProperties);
    }
    public static Index<TItem> Create(SupersonicList<TItem> parentSupersonicList, string name, params Expression<Func<TItem, object>>[] indexProperties)
    {
        return CreateHelper(parentSupersonicList, false, name, indexProperties);
    }
    public static Index<TItem> CreateUnique(SupersonicList<TItem> parentSupersonicList, string name, params Expression<Func<TItem, object>>[] indexProperties)
    {
        return CreateHelper(parentSupersonicList, true, name, indexProperties);
    }
    protected static Index<TItem> CreateHelper(SupersonicList<TItem> parentSupersonicList, bool isUnique, string name, params Expression<Func<TItem, object>>[] indexProperties)
    {
        using (new SustainedLowLatencyGC())
        {
            if (indexProperties.Length < 1) throw new Exception("When creating an index must have at least one index property");

            var indexPropertiesCompiled = new Func<TItem, object>[indexProperties.Length];
            var propNames = new string[indexProperties.Length];

            Index<TItem> index;
            if (SupersonicListSettings.SortingMethod == SupersonicListSettings.SortMethod.Sort)
            {
                for (var i = 0; i < indexProperties.Length; i++)
                {
                    ProcessProperty(indexProperties, i, indexPropertiesCompiled, propNames);
                }

                index = new Index<TItem>(parentSupersonicList)
                {
                    Name = name,
                    IndexProperties = indexProperties,
                    IndexPropertiesCompiled = indexPropertiesCompiled,
                    PropNames = propNames,
                    IsUnique = isUnique,
                };

                var orderedList = new List<TItem>(parentSupersonicList.GetItems());
                orderedList.Sort(new IndexComparer<TItem>(index));

                index.OrderedList = orderedList;
            }
            else if (SupersonicListSettings.SortingMethod == SupersonicListSettings.SortMethod.ParallelOrderBy)
            {
                ProcessProperty(indexProperties, 0, indexPropertiesCompiled, propNames);

                var orderedList = parentSupersonicList.GetItems().AsParallel().OrderBy(indexPropertiesCompiled[0]);

                for (var i = 1; i < indexProperties.Length; i++)
                {
                    ProcessProperty(indexProperties, i, indexPropertiesCompiled, propNames);
                    var tmpI = i;
                    orderedList = orderedList.ThenBy(indexPropertiesCompiled[tmpI]);
                }

                index = new Index<TItem>(parentSupersonicList)
                {
                    Name = name,
                    OrderedList = orderedList.ToList(),
                    IndexProperties = indexProperties,
                    IndexPropertiesCompiled = indexPropertiesCompiled,
                    PropNames = propNames,
                    IsUnique = isUnique,
                };
            }
            else
            {
                ProcessProperty(indexProperties, 0, indexPropertiesCompiled, propNames);

                var orderedList = parentSupersonicList.GetItems().OrderBy(indexPropertiesCompiled[0]);

                for (var i = 1; i < indexProperties.Length; i++)
                {
                    ProcessProperty(indexProperties, i, indexPropertiesCompiled, propNames);
                    var tmpI = i;
                    orderedList = orderedList.ThenBy(indexPropertiesCompiled[tmpI]);
                }

                index = new Index<TItem>(parentSupersonicList)
                {
                    Name = name,
                    OrderedList = orderedList.ToList(),
                    IndexProperties = indexProperties,
                    IndexPropertiesCompiled = indexPropertiesCompiled,
                    PropNames = propNames,
                    IsUnique = isUnique,
                };
            }

            if (isUnique && !index.IsIndexUnique()) throw new ArgumentException($"List countains objects with duplicate unique index {name}", nameof(parentSupersonicList));

            return index;
        }
    }
    protected static void ProcessProperty(Expression<Func<TItem, object>>[] indexProperties, int i, Func<TItem, object>[] indexPropertiesCompiled, string[] propNames)
    {
        if (!(indexProperties[i].Body is MemberExpression memberExpression))
        {
            if (!(indexProperties[i].Body is UnaryExpression unaryExpression)) throw new Exception("You can only pick properties for index.");
            if (!(unaryExpression.Operand is MemberExpression memberExpressionThroughUnaryExpression)) throw new Exception("You can only pick properties for index.");
            memberExpression = memberExpressionThroughUnaryExpression;
        }
        if (!(memberExpression.Member is PropertyInfo propInfo)) throw new Exception("You can only pick properties for index.");

        if (propNames.Any(x => x == propInfo.Name)) throw new Exception($"{propInfo.Name} is a duplicate properties in index.");
        propNames[i] = propInfo.Name;

        indexPropertiesCompiled[i] = indexProperties[i].Compile();
    }
    #endregion

    #region Overrides
    public override string ToString()
    {
        return Name;
    }
    #endregion

    #region Methods
    //TODO: remove this method. It is only used for testing
    public void PrintOrderedList()
    {
        foreach (var item in OrderedList) Console.WriteLine(item);
    }

    public void DisableIndex()
    {
        if (IsDisabled) throw new InvalidOperationException($"Cannot disable index {Name}. Index is already disabled.");
        IsDisabled = true;
        PendingDeletes.Clear();
    }
    public void EnableIndex()
    {
        if (!IsDisabled) throw new InvalidOperationException($"Cannot enable index {Name}. Index is already enabled.");
        IsDisabled = false;
        Rebuild();
        foreach (var item in PendingDeletes) Remove(item);
    }

    public void Clear()
    {
        OrderedList.Clear();
        PendingDeletes.Clear();
    }
    public void InsertAt(int idx, TItem item)
    {
        if (IsDisabled) throw new InvalidOperationException($"InsertAt method cannot be called on a Disabled Index {Name}");
        OrderedList.Insert(idx, item);
    }
    public void Remove(TItem item)
    {
        if (!IsDisabled)
        {
            var idx = IndexOf(item, out var itemFound);
            if (!itemFound)
            {
                //var idx2 = IndexOf(item, out var itemFound2);
                throw new Exception($"Item with Guid {ParentSupersonicList.GetGuid(item)} cannot be removed becasue it does not exist in the Index {Name}");
            }
            RemoveAt(idx);
        }
        else
        {
            PendingDeletes.Add(item);
        }
    }
    public void RemoveAt(int idx)
    {
        if (IsDisabled) throw new InvalidOperationException($"RemoveAt method cannot be called on a Disabled Index {Name}");
        OrderedList.RemoveAt(idx);
    }
    public void Add(TItem item)
    {
        if (!IsDisabled)
        {
            var idx = IndexOf(item, out var itemFound);
            if (IsUnique && itemFound) throw new Exception($"Item cannot be added becasue it would violate unique index {Name} constraint");
            InsertAt(idx, item);
        }
        else
        {
            OrderedList.Add(item);
        }
    }

    public void Rebuild()
    {
        using (new SustainedLowLatencyGC())
        {
            if (IsDisabled) throw new InvalidOperationException($"Rebuild method cannot be called on a Disabled Index {Name}");

            if (SupersonicListSettings.SortingMethod == SupersonicListSettings.SortMethod.Sort)
            {
                OrderedList.Sort(new IndexComparer<TItem>(this));
            }
            else if (SupersonicListSettings.SortingMethod == SupersonicListSettings.SortMethod.ParallelOrderBy)
            {
                var orderedList = OrderedList.AsParallel().OrderBy(IndexPropertiesCompiled[0]);
                for (var i = 1; i < IndexProperties.Length; i++)
                {
                    var tmpI = i;
                    orderedList = orderedList.ThenBy(IndexPropertiesCompiled[tmpI]);
                }
                orderedList = orderedList.ThenBy(x => ParentSupersonicList.GetGuid(x));
                OrderedList = orderedList.ToList();
            }
            else
            {
                var orderedList = OrderedList.OrderBy(IndexPropertiesCompiled[0]);
                for (var i = 1; i < IndexProperties.Length; i++)
                {
                    var tmpI = i;
                    orderedList = orderedList.ThenBy(IndexPropertiesCompiled[tmpI]);
                }
                orderedList = orderedList.ThenBy(x => ParentSupersonicList.GetGuid(x));
                OrderedList = orderedList.ToList();
            }

            if (IsUnique && !IsIndexUnique()) throw new Exception($"Index countains objects with duplicate unique index {Name}");
        }
    }
    #endregion

    #region Query Methods
    public IEnumerable<TItem> WhereWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return WhereWithIndex(condition);
    }
    internal IEnumerable<TItem> WhereWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("WhereWithIndex method cannot be called on a Disabled Index");
        var range = ProcessCondition(condition);
        return new IndexEnumerable<TItem>(range, OrderedList);
    }

    public int CountWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return CountWithIndex(condition);
    }
    internal int CountWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("CountWithIndex method cannot be called on a Disabled Index");
        var range = ProcessCondition(condition);
        return range.Size(OrderedList.Count);
    }

    public bool AnyWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        if (IsDisabled) throw new InvalidOperationException("AnyWithIndex method cannot be called on a Disabled Index");

        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return AnyWithIndex(condition);
    }
    internal bool AnyWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("AnyWithIndex method cannot be called on a Disabled Index");
        var item = ProcessConditionForFirst(condition);
        return item != null;
    }

    public bool AllWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        if (IsDisabled) throw new InvalidOperationException("AnyWithIndex method cannot be called on a Disabled Index");

        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return AllWithIndex(condition);
    }
    internal bool AllWithIndex(Condition condition)
    {
        var range = ProcessCondition(condition);
        if (range.SimpleRangesLength != 1) return false;
        var simpleRange = range.SimpleRanges.Single();
        return simpleRange.StartIdx == 0 && simpleRange.EndIdx == OrderedList.Count - 1;
    }

    public TItem SingleWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        if (IsDisabled) throw new InvalidOperationException("SingleWithIndex method cannot be called on a Disabled Index");

        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return SingleWithIndex(condition);
    }
    internal TItem SingleWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("SingleWithIndex method cannot be called on a Disabled Index");
        var range = ProcessCondition(condition);
        return new IndexEnumerable<TItem>(range, OrderedList).Single();
    }

    public TItem SingleOrDefaultWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        if (IsDisabled) throw new InvalidOperationException("SingleOrDefaultWithIndex method cannot be called on a Disabled Index");

        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return SingleOrDefaultWithIndex(condition);
    }
    internal TItem SingleOrDefaultWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("SingleOrDefaultWithIndex method cannot be called on a Disabled Index");
        var range = ProcessCondition(condition);
        return new IndexEnumerable<TItem>(range, OrderedList).SingleOrDefault();
    }

    public TItem FirstWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        if (IsDisabled) throw new InvalidOperationException("FirstWithIndex method cannot be called on a Disabled Index");

        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return FirstWithIndex(condition);
    }
    internal TItem FirstWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("FirstWithIndex method cannot be called on a Disabled Index");
        var item = ProcessConditionForFirst(condition);
        if (item == null) throw new InvalidOperationException("Sequence contains no elements");
        return item;
    }

    public TItem FirstOrDefaultWithIndex(Expression<Func<TItem, bool>> predicate)
    {
        if (IsDisabled) throw new InvalidOperationException("FirstOrDefaultWithIndex method cannot be called on a Disabled Index");

        predicate = (Expression<Func<TItem, bool>>)Evaluator.PartialEval(predicate);
        var condition = ParseRootCondition(predicate.Body, out var errorMessage);
        if (errorMessage != null) throw new ArgumentException(errorMessage, nameof(predicate));
        return FirstOrDefaultWithIndex(condition);
    }
    internal TItem FirstOrDefaultWithIndex(Condition condition)
    {
        if (IsDisabled) throw new InvalidOperationException("FirstOrDefaultWithIndex method cannot be called on a Disabled Index");
        var item = ProcessConditionForFirst(condition);
        return item;
    }

    public int IndexOf(TItem item, out bool matchFound, int? startIdx = null, int? endIdx = null)
    {
        if (IsDisabled) throw new InvalidOperationException($"AnyIndexOf method cannot be called on a Disabled Index {Name}");

        if (OrderedList.Count == 0)
        {
            matchFound = false;
            return 0;
        }

        if (startIdx == null) startIdx = 0;
        if (endIdx == null) endIdx = OrderedList.Count - 1;

        var startResult = CompareItemAandItemBUsingIndexProperties(item, OrderedList[startIdx.Value], true, out matchFound);
        if (startResult == 0) return startIdx.Value;
        if (startResult < 0) return startIdx.Value;

        var endResult = CompareItemAandItemBUsingIndexProperties(item, OrderedList[endIdx.Value], true, out matchFound);
        if (endResult == 0) return endIdx.Value;
        if (endResult > 0) return endIdx.Value + 1;

        var midIdx = (int)(startIdx + endIdx) / 2;
        var midResult = CompareItemAandItemBUsingIndexProperties(item, OrderedList[midIdx], true, out matchFound);
        if (midResult == 0) return midIdx;

        if (midIdx == startIdx) return midIdx + 1;

        if (midResult > 0) return IndexOf(item, out matchFound, midIdx + 1, endIdx - 1);
        else return IndexOf(item, out matchFound, startIdx + 1, midIdx - 1);
    }
    #endregion

    #region Helper Methods
    internal Condition ParseRootCondition(Expression expression, out string errorMessage)
    {
        var condition = ParseCondition(expression, out errorMessage);
        if (errorMessage != null) return null;

        //Sort and Validate factors in terms
        foreach (var term in condition.Terms)
        {
            term.OrderByProperyIndexAndValidate(out errorMessage);
            if (errorMessage != null) return null;
        }
        return condition;
    }
    internal Condition ParseCondition(Expression expression, out string errorMessage)
    {
        var condition = new Condition();
        if (expression is BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                var leftSubCondition = ParseCondition(binaryExpression.Left, out errorMessage);
                if (errorMessage != null) return null;

                var rightSubCondition = ParseCondition(binaryExpression.Right, out errorMessage);
                if (errorMessage != null) return null;

                condition.Terms.AddRange(leftSubCondition.Terms);
                condition.Terms.AddRange(rightSubCondition.Terms);
            }
            else
            {
                var terms = ParseTerm(expression, out errorMessage);
                if (errorMessage != null) return null;
                condition.Terms.AddRange(terms);
            }
        }
        else
        {
            var terms = ParseTerm(expression, out errorMessage);
            if (errorMessage != null) return null;
            condition.Terms.AddRange(terms);
            return condition;
        }

        errorMessage = null;
        return condition;
    }
    internal List<Term> ParseTerm(Expression expression, out string errorMessage)
    {
        var terms = new List<Term> { new() };

        if (expression is BinaryExpression binaryExpression)
        {
            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                var leftSubTerms = ParseTerm(binaryExpression.Left, out errorMessage);
                if (errorMessage != null) return null;
                terms = Term.MultiplyTerms(terms, leftSubTerms);

                var rightSubTerms = ParseTerm(binaryExpression.Right, out errorMessage);
                if (errorMessage != null) return null;
                terms = Term.MultiplyTerms(terms, rightSubTerms);
            }
            else if (binaryExpression.NodeType == ExpressionType.OrElse)
            {
                var condition = ParseCondition(expression, out errorMessage);
                if (errorMessage != null) return null;

                terms = Term.MultiplyTerms(terms, condition.Terms);
            }
            else
            {
                var factor = ParseBinaryFactor(expression, out errorMessage);
                if (errorMessage != null) return null;
                terms = Term.MultiplyTerms(terms, [new() { Factors = [factor] }]);
            }
        }
        else
        {
            var factor = ParseUnaryFactor(expression, out errorMessage);
            if (errorMessage != null) return null;
            terms = Term.MultiplyTerms(terms, [new() { Factors = [factor] }]);
        }
        return terms;
    }
    internal Factor ParseBinaryFactor(Expression expression, out string errorMessage)
    {
        var factor = new Factor();
        if (!(expression is BinaryExpression binaryExpression))
        {
            errorMessage = "Invalid predicate";
            return null;
        }

        MemberExpression memberExpression;
        ConstantExpression constantExpression;
        Factor.OperationEnum operation;

        // ReSharper disable once MergeCastWithTypeCheck
        if (binaryExpression.Left is MemberExpression && binaryExpression.Right is ConstantExpression)
        {
            memberExpression = (MemberExpression)binaryExpression.Left;
            constantExpression = (ConstantExpression)binaryExpression.Right;
            operation = TryConvertToTermOperation(binaryExpression.NodeType, false, out errorMessage);
            if (errorMessage != null) return null;
        }
        else if (binaryExpression.Left is ConstantExpression && binaryExpression.Right is MemberExpression)
        {
            memberExpression = (MemberExpression)binaryExpression.Right;
            constantExpression = (ConstantExpression)binaryExpression.Left;
            operation = TryConvertToTermOperation(binaryExpression.NodeType, true, out errorMessage);
            if (errorMessage != null) return null;
        }
        else
        {
            errorMessage = "Expression must contain a member expression and a constant operands";
            return null;
        }

        var idx = PropNames.IndexOf(memberExpression.Member.Name);
        if (idx == -1)
        {
            errorMessage = $"Predicate not valid for selected Index because it contains {memberExpression.Member.Name}";
            return null;
        }

        factor.PropertyIndex = idx;
        factor.Operation = operation;
        factor.Constant = constantExpression.Value;

        errorMessage = null;
        return factor;
    }
    internal Factor ParseUnaryFactor(Expression expression, out string errorMessage)
    {
        var factor = new Factor();

        MemberExpression memberExpression;
        ConstantExpression constantExpression;
        Factor.OperationEnum operation;

        // ReSharper disable once MergeCastWithTypeCheck
        if (expression is MemberExpression)
        {
            memberExpression = (MemberExpression)expression;
            constantExpression = Expression.Constant(true);
            operation = TryConvertToTermOperation(ExpressionType.Equal, false, out errorMessage);
            if (errorMessage != null) return null;
        }
        else if (expression is UnaryExpression unaryExpression)
        {
            if (unaryExpression.NodeType != ExpressionType.Not || !(unaryExpression.Operand is MemberExpression)) throw new ArgumentException("Invalid predicate");

            memberExpression = (MemberExpression)unaryExpression.Operand;
            constantExpression = Expression.Constant(false);
            operation = TryConvertToTermOperation(ExpressionType.Equal, false, out errorMessage);
            if (errorMessage != null) return null;
        }
        else
        {
            errorMessage = "Invalid predicate";
            return null;
        }

        var idx = PropNames.IndexOf(memberExpression.Member.Name);
        if (idx == -1)
        {
            errorMessage = $"Pedicate not valid for selcted Index because it contains {memberExpression.Member.Name}";
            return null;
        }

        factor.PropertyIndex = idx;
        factor.Operation = operation;
        factor.Constant = constantExpression.Value;

        errorMessage = null;
        return factor;
    }

    internal Ranges.Range ProcessCondition(Condition condition)
    {
        var range = new Ranges.Range();
        foreach (var term in condition.Terms) range += ProcessTerm(term);
        return range;
    }
    internal Ranges.Range ProcessTerm(Term term)
    {
        var equalityRange = ProcessEqualityFactors(term.EqualityFactors);
        var inequalityRange = ProcessInequalityFactors(term.InequalityFactors, equalityRange);
        return new Ranges.Range(inequalityRange);
    }

    internal TItem ProcessConditionForFirst(Condition condition)
    {
        foreach (var term in condition.Terms)
        {
            var item = ProcessTermForFirst(term);
            if (item != null) return item;
        }
        return null;
    }
    internal TItem ProcessTermForFirst(Term term)
    {
        if (OrderedList.Count == 0) return null;

        var equalityRange = ProcessEqualityFactors(term.EqualityFactors);
        var inequalityRange = ProcessInequalityFactors(term.InequalityFactors, equalityRange);
        if (inequalityRange.IsEmpty) return null;
        return OrderedList[inequalityRange.StartIdx];
    }

    internal SimpleRange ProcessEqualityFactors(List<Factor> factors)
    {
        if (OrderedList.Count == 0) return new SimpleRange();

        var startIndexOf = FindStartForFactorsForEqualities(factors);
        if (startIndexOf == -1) return new SimpleRange();

        var endIndexOf = FindEndForFactorsForEqualities(factors);
        if (endIndexOf == -1) return new SimpleRange();

        return new SimpleRange(startIndexOf, endIndexOf);
    }
    internal SimpleRange ProcessInequalityFactors(List<Factor> factors, SimpleRange currentRange)
    {
        if (currentRange.IsEmpty) return currentRange;
        foreach (var factor in factors) currentRange = ProcessInequalityFactor(factor, currentRange);
        return currentRange;
    }
    internal SimpleRange ProcessInequalityFactor(Factor factor, SimpleRange currentRange)
    {
        if (factor == null) return currentRange;
        if (currentRange.IsEmpty) return currentRange;

        var factors = new List<Factor> { factor };
        switch (factor.Operation)
        {
            case Factor.OperationEnum.LessThan:
            {
                var startIndexOf = FindStartForFactorsForInequalities(factors, out var _, currentRange.StartIdx, currentRange.EndIdx);
                var rangeEnd = startIndexOf - 1;
                if (rangeEnd < currentRange.StartIdx) return new SimpleRange();
                return new SimpleRange(currentRange.StartIdx, rangeEnd);
            }
            case Factor.OperationEnum.GreaterThan:
            {
                var endIndexOf = FindEndForFactorsForInequalities(factors, out var matchFound, currentRange.StartIdx, currentRange.EndIdx);
                if (matchFound)
                {
                    var rangeStart = endIndexOf + 1;
                    if (rangeStart > currentRange.EndIdx) return new SimpleRange();
                    return new SimpleRange(rangeStart, currentRange.EndIdx);
                }
                else
                {
                    return new SimpleRange(endIndexOf, currentRange.EndIdx);
                }
            }
            case Factor.OperationEnum.LessThanOrEqual:
            {
                var endIndexOf = FindEndForFactorsForInequalities(factors, out var matchFound, currentRange.StartIdx, currentRange.EndIdx);

                if (matchFound)
                {
                    return new SimpleRange(currentRange.StartIdx, endIndexOf);
                }
                else
                {
                    var rangeEnd = endIndexOf - 1;
                    if (rangeEnd < currentRange.StartIdx) return new SimpleRange();
                    return new SimpleRange(currentRange.StartIdx, rangeEnd);
                }
            }
            case Factor.OperationEnum.GreaterThanOrEqual:
            {
                var startIndexOf = FindStartForFactorsForInequalities(factors, out var _, currentRange.StartIdx, currentRange.EndIdx);
                return new SimpleRange(startIndexOf, currentRange.EndIdx);
            }
            default:
            {
                throw new InvalidProgramException($"Unsupported operation {factor.Operation}");
            }
        }
    }

    internal int FindStartForFactorsForEqualities(List<Factor> factors, int? startIdx = null, int? endIdx = null)
    {
        if (OrderedList.Count == 0) return -1;

        if (startIdx == null) startIdx = 0;
        if (endIdx == null) endIdx = OrderedList.Count - 1;

#if DEBUG
        if (startIdx > endIdx) Debugger.Break();
#endif

        if (!factors.Any()) return startIdx.Value;

        var startResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value);
        if (startResult < 0) return -1;

        //var preStartResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value - 1);
        if (startResult == 0 /*&& preStartResult != 0*/) return startIdx.Value;

        var endResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value);
        if (endResult > 0) return -1;

        var preEndResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value - 1);
        if (endResult == 0 && preEndResult != 0) return endIdx.Value;

        var midIdx = (int)(startIdx + endIdx) / 2;
        var midResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx);
        var preMidResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx - 1);
        if (midResult == 0 && preMidResult != 0) return midIdx;

        if (midIdx == startIdx) return -1;

        if (midResult <= 0)
        {
            if (startIdx + 1 > midIdx - 1) return -1;
            return FindStartForFactorsForEqualities(factors, startIdx + 1, midIdx - 1);
        }
        else
        {
            if (midIdx + 1 > endIdx - 1) return -1;
            return FindStartForFactorsForEqualities(factors, midIdx + 1, endIdx - 1);
        }
    }
    internal int FindEndForFactorsForEqualities(List<Factor> factors, int? startIdx = null, int? endIdx = null)
    {
        if (OrderedList.Count == 0) return -1;

        if (startIdx == null) startIdx = 0;
        if (endIdx == null) endIdx = OrderedList.Count - 1;

#if DEBUG
        if (startIdx > endIdx) Debugger.Break();
#endif

        if (!factors.Any()) return endIdx.Value;

        var startResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value);
        if (startResult < 0) return -1;

        var postStartResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value + 1);
        if (startResult == 0 && postStartResult != 0) return startIdx.Value;

        var endResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value);
        if (endResult > 0) return -1;

        //var postEndResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value + 1);
        if (endResult == 0 /*&& postEndResult != 0*/) return endIdx.Value;

        var midIdx = (int)(startIdx + endIdx) / 2;
        var midResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx);
        var postMidResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx + 1);
        if (midResult == 0 && postMidResult != 0) return midIdx;

        if (midIdx == startIdx) return -1;

        if (midResult >= 0)
        {
            if (midIdx + 1 > endIdx - 1) return -1;
            return FindEndForFactorsForEqualities(factors, midIdx + 1, endIdx - 1);
        }
        else
        {
            if (startIdx + 1 > midIdx - 1) return -1;
            return FindEndForFactorsForEqualities(factors, startIdx + 1, midIdx - 1);
        }
    }

    internal int FindStartForFactorsForInequalities(List<Factor> factors, out bool matchFound, int? startIdx = null, int? endIdx = null)
    {
        if (IsUnique) return FindAnyForFactorsForInequalities(factors, out matchFound, startIdx, endIdx);

        if (OrderedList.Count == 0)
        {
            matchFound = false;
            return 0;
        }

        if (startIdx == null) startIdx = 0;
        if (endIdx == null) endIdx = OrderedList.Count - 1;

#if DEBUG
        if (startIdx > endIdx) Debugger.Break();
#endif

        var startResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value);
        if (startResult < 0)
        {
            matchFound = false;
            return startIdx.Value;
        }

        //var preStartResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value - 1);
        if (startResult == 0 /*&& preStartResult != 0*/)
        {
            matchFound = true;
            return startIdx.Value;
        }

        var endResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value);
        if (endResult > 0)
        {
            matchFound = false;
            return endIdx.Value + 1;
        }

        var preEndResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value - 1);
        if (endResult == 0 && preEndResult != 0)
        {
            matchFound = true;
            return endIdx.Value;
        }

        var midIdx = (int)(startIdx + endIdx) / 2;
        var midResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx);
        var preMidResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx - 1);
        if (midResult == 0 && preMidResult != 0)
        {
            matchFound = true;
            return midIdx;
        }

        if (midIdx == startIdx)
        {
            matchFound = false;
            return midIdx + 1;
        }

        if (midResult <= 0)
        {
            if (startIdx + 1 > midIdx - 1)
            {
                matchFound = false;
                return midIdx + 1;
            }
            return FindStartForFactorsForInequalities(factors, out matchFound, startIdx + 1, midIdx - 1);
        }
        else
        {
            if (midIdx + 1 > endIdx - 1)
            {
                matchFound = false;
                return midIdx + 1;
            }
            return FindStartForFactorsForInequalities(factors, out matchFound, midIdx + 1, endIdx - 1);
        }
    }
    internal int FindEndForFactorsForInequalities(List<Factor> factors, out bool matchFound, int? startIdx = null, int? endIdx = null)
    {
        if (IsUnique) return FindAnyForFactorsForInequalities(factors, out matchFound, startIdx, endIdx);

        if (OrderedList.Count == 0)
        {
            matchFound = false;
            return 0;
        }

        if (startIdx == null) startIdx = 0;
        if (endIdx == null) endIdx = OrderedList.Count - 1;

#if DEBUG
        if (startIdx > endIdx) Debugger.Break();
#endif

        var startResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value);
        if (startResult < 0)
        {
            matchFound = false;
            return startIdx.Value;
        }

        var postStartResult = CompareTermAndItemBUsingIndexProperties(factors, startIdx.Value + 1);
        if (startResult == 0 && postStartResult != 0)
        {
            matchFound = true;
            return startIdx.Value;
        }

        var endResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value);
        if (endResult > 0)
        {
            matchFound = false;
            return endIdx.Value + 1;
        }

        //var postEndResult = CompareTermAndItemBUsingIndexProperties(factors, endIdx.Value + 1);
        if (endResult == 0 /*&& postEndResult != 0*/)
        {
            matchFound = true;
            return endIdx.Value;
        }

        var midIdx = (int)(startIdx + endIdx) / 2;
        var midResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx);
        var postMidResult = CompareTermAndItemBUsingIndexProperties(factors, midIdx + 1);
        if (midResult == 0 && postMidResult != 0)
        {
            matchFound = true;
            return midIdx;
        }
        if (midIdx == startIdx)
        {
            matchFound = false;
            return midIdx + 1;
        }

        if (midResult >= 0)
        {
            if (startIdx + 1 > midIdx - 1)
            {
                matchFound = false;
                return midIdx + 1;
            }
            return FindEndForFactorsForInequalities(factors, out matchFound, midIdx + 1, endIdx - 1);
        }
        else
        {
            if (midIdx + 1 > endIdx - 1)
            {
                matchFound = false;
                return midIdx + 1;
            }
            return FindEndForFactorsForInequalities(factors, out matchFound, startIdx + 1, midIdx - 1);
        }
    }
    internal int FindAnyForFactorsForInequalities(List<Factor> factors, out bool matchFound, int? startIdx = null, int? endIdx = null)
    {
        if (OrderedList.Count == 0)
        {
            matchFound = false;
            return 0;
        }

        if (startIdx == null) startIdx = 0;
        if (endIdx == null) endIdx = OrderedList.Count - 1;

#if DEBUG
        if (startIdx > endIdx) Debugger.Break();
#endif

        var startResult = CompareTermAndItemBUsingIndexProperties(factors, OrderedList[startIdx.Value]);
        if (startResult == 0)
        {
            matchFound = true;
            //Look for item
            return startIdx.Value;
        }
        if (startResult < 0)
        {
            matchFound = false;
            return startIdx.Value;
        }

        var endResult = CompareTermAndItemBUsingIndexProperties(factors, OrderedList[endIdx.Value]);
        if (endResult == 0)
        {
            matchFound = true;
            //Look for item 
            return endIdx.Value;
        }
        if (endResult > 0)
        {
            matchFound = false;
            return endIdx.Value + 1;
        }

        var midIdx = (int)(startIdx + endIdx) / 2;
        var midResult = CompareTermAndItemBUsingIndexProperties(factors, OrderedList[midIdx]);
        if (midResult == 0)
        {
            matchFound = true;
            //Look for item 
            return midIdx;
        }
        if (midIdx == startIdx)
        {
            matchFound = false;
            return midIdx + 1;
        }

        if (midResult > 0) return FindAnyForFactorsForInequalities(factors, out matchFound, midIdx + 1, endIdx - 1);
        else return FindAnyForFactorsForInequalities(factors, out matchFound, startIdx + 1, midIdx - 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int CompareItemAandItemBUsingIndexProperties(TItem item, int idx, bool useGuid, out bool indexMatchFound)
    {
        if (idx < 0)
        {
            indexMatchFound = false;
            return -1;
        }

        if (idx >= OrderedList.Count)
        {
            indexMatchFound = false;
            return 1;
        }

        return CompareItemAandItemBUsingIndexProperties(item, OrderedList[idx], useGuid, out indexMatchFound);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int CompareItemAandItemBUsingIndexProperties(TItem listItemA, TItem listItemB, bool useGuid, out bool indexMatchFound)
    {
        foreach (var indexPropertyCompiled in IndexPropertiesCompiled)
        {
            var a = indexPropertyCompiled.Invoke(listItemA);
            var b = indexPropertyCompiled.Invoke(listItemB);
            var result = Comparer.Default.Compare(a, b);
            if (result > 0)
            {
                indexMatchFound = false;
                return 1;
            }
            if (result < 0)
            {
                indexMatchFound = false;
                return -1;
            }
            //if result == 0, we continue to next index
        }

        //if we found the objects to be equal for the purposes of the Index and it is required, compare guids;
        indexMatchFound = true;

        if (!useGuid) return 0;
        return Comparer.Default.Compare(ParentSupersonicList.GetGuid(listItemA), ParentSupersonicList.GetGuid(listItemB));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int CompareTermAndItemBUsingIndexProperties(List<Factor> factors, int idx)
    {
        if (idx < 0) return -1;
        if (idx >= OrderedList.Count) return 1;
        return CompareTermAndItemBUsingIndexProperties(factors, OrderedList[idx]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int CompareTermAndItemBUsingIndexProperties(List<Factor> factors, TItem listItemB)
    {
        foreach (var factor in factors)
        {
            var result = CompareResultAndItemBUsingIndexProperty(factor.PropertyIndex, factor.Constant, listItemB);
            if (result != 0) return result;
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int CompareResultAndItemBUsingIndexProperty(int propertyIdx, object a, int idx)
    {
        if (idx < 0) return -1;
        if (idx >= OrderedList.Count) return 1;
        return CompareResultAndItemBUsingIndexProperty(propertyIdx, a, OrderedList[idx]);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected int CompareResultAndItemBUsingIndexProperty(int propertyIdx, object a, TItem listItemB)
    {
        var indexPropertyCompiled = IndexPropertiesCompiled[propertyIdx];
        var b = indexPropertyCompiled.Invoke(listItemB);
        var result = Comparer.Default.Compare(a, b);
        return result;
    }

    protected bool IsIndexUnique()
    {
        using (new SustainedLowLatencyGC())
        {
            if (OrderedList.Count == 0) return true;

            var previousItem = OrderedList[0];
            for (var i = 1; i < OrderedList.Count; i++)
            {
                if (CompareItemAandItemBUsingIndexProperties(OrderedList[i], previousItem, false, out var _) == 0) return false;
            }
            return true;
        }
    }
    internal Factor.OperationEnum TryConvertToTermOperation(ExpressionType nodeType, bool invert, out string errorMessage)
    {
        errorMessage = null;

        if (invert)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal: return Factor.OperationEnum.Equal;
                case ExpressionType.LessThan: return Factor.OperationEnum.GreaterThan;
                case ExpressionType.GreaterThan: return Factor.OperationEnum.LessThan;
                case ExpressionType.LessThanOrEqual: return Factor.OperationEnum.GreaterThanOrEqual;
                case ExpressionType.GreaterThanOrEqual: return Factor.OperationEnum.LessThanOrEqual;
                default: errorMessage = $"{nodeType} is an unsupported operation"; return Factor.OperationEnum.Equal;
            }
        }
        else
        {
            switch (nodeType)
            {
                case ExpressionType.Equal: return Factor.OperationEnum.Equal;
                case ExpressionType.LessThan: return Factor.OperationEnum.LessThan;
                case ExpressionType.GreaterThan: return Factor.OperationEnum.GreaterThan;
                case ExpressionType.LessThanOrEqual: return Factor.OperationEnum.LessThanOrEqual;
                case ExpressionType.GreaterThanOrEqual: return Factor.OperationEnum.GreaterThanOrEqual;
                default: errorMessage = $"{nodeType} is an unsupported operation"; return Factor.OperationEnum.Equal;
            }
        }
    }
    #endregion

    #region Properties
    public string Name { get; protected set; }
    protected Expression<Func<TItem, object>>[] IndexProperties { get; set; }
    protected Func<TItem, object>[] IndexPropertiesCompiled { get; set; }
    protected string[] PropNames { get; set; }
    protected List<TItem> OrderedList { get; set; }
    protected bool IsUnique { get; set; }

    protected List<TItem> PendingDeletes { get; set; } = [];

    public bool IsDisabled { get; protected set; }
    public IEnumerable<TItem> Items => OrderedList.AsEnumerable();

    protected SupersonicList<TItem> ParentSupersonicList { get; set; }
    #endregion
}