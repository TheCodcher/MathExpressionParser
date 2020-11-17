using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SomeParseSolution
{
    class ExpressionParser<TArg>
    {
        public const string RequiredSymbol = ";";
        Dictionary<string, MathSymbolProperty> WordDict;
        string[] WordMass;
        IExpressionCalculator<string, TArg> Calc;
        public ExpressionParser(IExpressionCalculator<string, TArg> calc, Dictionary<string, int> devidedSymbolsWeight)
        {
            Calc = calc;
            var temp = devidedSymbolsWeight.Select(i => new KeyValuePair<string, MathSymbolProperty>(i.Key, new MathSymbolProperty(i.Value, true)))
                .Concat(calc.GetOperations().Select(i => new KeyValuePair<string, MathSymbolProperty>(i.Symbol, new MathSymbolProperty(i.Weight, false))));
            WordDict = new Dictionary<string, MathSymbolProperty>(temp);
            WordMass = WordDict.Select(i => i.Key).ToArray();
        }
        public virtual ParseExpressionResult<string, TArg> Parse(string source)
        {
            foreach (var i in WordMass)
            {
                source = source.Replace(i, RequiredSymbol + i + RequiredSymbol);
            }
            var mass = source.Split(RequiredSymbol, StringSplitOptions.RemoveEmptyEntries);
            int weightK = 0;
            Queue<object> values = new Queue<object>();
            List<KeyValuePair<int, int>> WeightIndx = new List<KeyValuePair<int, int>>();
            var result = new ParseExpressionResult<string, TArg>();
            foreach(var i in mass)
            {
                if (!WordDict.ContainsKey(i))
                {
                    values.Enqueue(new ExpressionNode<string, TArg>(i, result.GetDictionaryValue));
                }
                else
                {
                    var now = WordDict[i];
                    if (now.isDevided)
                    {
                        weightK += now.Weight;
                    }
                    else
                    {
                        WeightIndx.Add(new KeyValuePair<int, int>(now.Weight + weightK, values.Count));
                        values.Enqueue(i);
                    }
                }
            }
            WeightIndx.Sort((y, x) => (x.Key - y.Key) == 0 ? y.Value - x.Value : x.Key - y.Key);

            List<MassReplacer> replacers = new List<MassReplacer>();
            var tempStart = 0;
            for (int i = WeightIndx.IndexOf(WeightIndx.First(i => i.Key == WeightIndx.Last().Key)); i < WeightIndx.Count; i++)
            {
                replacers.Add(new MassReplacer(tempStart, (WeightIndx[i].Value - 1) < 0 ? 0 : (WeightIndx[i].Value - 1)));
                tempStart = WeightIndx[i].Value + 1;
            }
            replacers.Add(new MassReplacer(tempStart, values.Count - 1));
            var generalreplacer = new MassReplacer(0, values.Count - 1);
            var operations = WeightIndx.Select(i => i.Value).ToArray();
            var resultMass = values.ToArray();
            Dictionary<string, MathOperation<string, TArg>> comparer = new Dictionary<string, MathOperation<string, TArg>>(Calc.GetOperations().Select(i => new KeyValuePair<string, MathOperation<string, TArg>>(i.Symbol, i)));
            foreach (var opIndx in operations)
            {
                var now = comparer[(string)resultMass[opIndx]];
                var nowRepl = replacers.Find(i => i.Incude(opIndx));
                if (nowRepl == null)
                {
                    generalreplacer.SetExpansionScope(replacers.Find(i => i.Incude(opIndx - 1)).NowBottom, replacers.Find(i => i.Incude(opIndx + 1)).NowTop);
                    nowRepl = generalreplacer;
                }
                ExpressionNode<string, TArg> temp;
                if (now.OneArgFunc)
                {
                    temp = new ExpressionNode<string, TArg>((ExpressionNode<string, TArg>)resultMass[opIndx + 1], a => Calc.Calculate(now.Symbol, a));
                    nowRepl.SetExpansionScope(opIndx, opIndx + 1);
                }
                else
                {
                    temp = new ExpressionNode<string, TArg>((ExpressionNode<string, TArg>)resultMass[opIndx - 1], (ExpressionNode<string, TArg>)resultMass[opIndx + 1], (a, b) => Calc.Calculate(now.Symbol, a, b));
                    nowRepl.SetExpansionScope(opIndx - 1, opIndx + 1);
                }
                nowRepl.Replace(resultMass, temp);
            }
            result.Set((ExpressionNode<string, TArg>)generalreplacer.GetResult(resultMass));
            return result;
        }
        class MassReplacer
        {
            int StartIndx;
            int EndIndx;
            public int NowBottom { get; private set; }
            public int NowTop { get; private set; }
            public MassReplacer(int start, int end)
            {
                StartIndx = start;
                EndIndx = end;
                NowBottom = end;
                NowTop = start;
            }
            public void SetExpansionScope(int bottom, int top)
            {
                if (bottom >= StartIndx && bottom < NowBottom) NowBottom = bottom;
                if (top <= EndIndx && top > NowTop) NowTop = top;
            }
            public void Replace(object[] mass, object obj)
            {
                mass[NowBottom] = obj;
                mass[NowTop] = obj;
            }
            public bool Incude(int indx) => indx >= StartIndx && indx <= EndIndx;
            public object GetResult(object[] mass) => mass[StartIndx];
        }
        struct MathSymbolProperty
        {
            public readonly int Weight;
            public readonly bool isDevided;
            public MathSymbolProperty(int w, bool isDiv)
            {
                Weight = w;
                isDevided = isDiv;
            }
        }
    }
    class ExpressionParser<T, TArg> : ExpressionParser<TArg>
    {
        public ExpressionParser(MathOperation<T, TArg>[] calcOprs, Dictionary<T, int> devidedSymbolsWeight) :
            base(new RealizeExpressionCalculator(calcOprs),
                new Dictionary<string, int>(devidedSymbolsWeight.Select(i => new KeyValuePair<string, int>(i.Key.ToString(), i.Value))))
        { }
        class RealizeExpressionCalculator : ExpressionCalculator<string, TArg>
        {
            MathOperation<string, TArg>[] mathOperations;
            public RealizeExpressionCalculator(MathOperation<T, TArg>[] calcOprs)
            {
                mathOperations = calcOprs.Select(i => new MathOperation<string, TArg>(i.Symbol.ToString(), i.Operation, i.OneArgFunc, i.Weight)).ToArray();
            }
            protected override MathOperation<string, TArg>[] InicializeOperations()
            {
                return mathOperations;
            }
        }
        public virtual ParseExpressionResult<string, TArg> Parse(string source, Dictionary<T, TArg> valueDict)
        {
            return Parse(source/*,new Dictionary<string, TArg>(valueDict.Select(i => new KeyValuePair<string, TArg>(i.Key.ToString(), i.Value)))*/);
        }
    }
}
