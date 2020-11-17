using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SomeParseSolution
{
    interface IExpressionCalculator<TKey, TArg>
    {
        MathOperation<TKey, TArg>[] GetOperations();
        TArg Calculate(TKey op, TArg arg1, TArg arg2);
        TArg Calculate(TKey op, TArg arg);
    }
    struct MathOperation<TSymbol, TArg>
    {
        public readonly TSymbol Symbol;
        public readonly Delegate Operation;
        public readonly int Weight;
        public readonly bool OneArgFunc;
        public MathOperation(TSymbol symbol, Func<TArg, TArg, TArg> func, int weigth = 1)
            : this(symbol, func, false, weigth) { }
        public MathOperation(TSymbol symbol, Func<TArg, TArg> func, int weigth = 1)
            : this(symbol, func, true, weigth) { }
        public MathOperation(TSymbol symbol, Delegate func, bool isOneArgFunc, int weigth = 1)
        {
            Operation = func;
            Weight = weigth;
            Symbol = symbol;
            OneArgFunc = isOneArgFunc;
        }
    }
    abstract class Calculator<TKey, TArg>
    {
        Dictionary<TKey, Func<TArg, TArg, TArg>> Dict;
        Dictionary<TKey, Func<TArg, TArg>> ChangerDict;
        public Calculator()
        {
            Dict = InicializeTwoArgFuncs();
            ChangerDict = InicializeOneArgFuncs();
        }
        public TArg Calculate(TKey op, TArg arg1, TArg arg2)
        {
            return Dict[op](arg1, arg2);
        }
        public TArg Calculate(TKey op, TArg arg)
        {
            return ChangerDict[op](arg);
        }
        protected abstract Dictionary<TKey, Func<TArg, TArg>> InicializeOneArgFuncs();
        protected abstract Dictionary<TKey, Func<TArg, TArg, TArg>> InicializeTwoArgFuncs();
    }
    abstract class ExpressionCalculator<TSymbol, TArg> : Calculator<TSymbol, TArg>, IExpressionCalculator<TSymbol, TArg>
    {
        MathOperation<TSymbol, TArg>[] mathOperations;
        public ExpressionCalculator()
        {
            Inicialize();
        }
        protected override Dictionary<TSymbol, Func<TArg, TArg, TArg>> InicializeTwoArgFuncs()
        {
            Inicialize();
            return new Dictionary<TSymbol, Func<TArg, TArg, TArg>>(mathOperations.Where(i => !i.OneArgFunc).Select(i => new KeyValuePair<TSymbol, Func<TArg, TArg, TArg>>(i.Symbol, (Func<TArg, TArg, TArg>)i.Operation)));
        }
        protected override Dictionary<TSymbol, Func<TArg, TArg>> InicializeOneArgFuncs()
        {
            Inicialize();
            return new Dictionary<TSymbol, Func<TArg, TArg>>(mathOperations.Where(i => i.OneArgFunc).Select(i => new KeyValuePair<TSymbol, Func<TArg, TArg>>(i.Symbol, (Func<TArg, TArg>)i.Operation)));
        }
        protected abstract MathOperation<TSymbol, TArg>[] InicializeOperations();
        public MathOperation<TSymbol, TArg>[] GetOperations()
        {
            return mathOperations;
        }
        void Inicialize()
        {
            if (mathOperations == null) mathOperations = InicializeOperations();
        }
    }
}
