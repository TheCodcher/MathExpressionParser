using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

namespace SomeParseSolution
{
    class ParseExpressionResult<TKey, TArg>
    {
        IDictionary<TKey, TArg> _values;
        ExpressionNode<TKey, TArg> _upperNode;
        public ParseExpressionResult() { }
        public TArg Execute(IDictionary<TKey, TArg> values)
        {
            _values = values;
            return _upperNode.Execute();
        }
        public void Set(ExpressionNode<TKey, TArg> upperNode)
        {
            _upperNode = upperNode;
        }
        public TArg GetDictionaryValue(TKey key)
        {
            return _values[key];
        }
    }
    class ExpressionNode<TKey, TArg>
    {
        Func<TArg> _execute;
        //для функции от одного аргумента
        public ExpressionNode(ExpressionNode<TKey, TArg> node, Func<TArg,TArg> func)
        {
            _execute = () => func(node.Execute());
        }
        //для функции от двух аргументов
        public ExpressionNode(ExpressionNode<TKey, TArg> nodeFirstArg, ExpressionNode<TKey, TArg> nodeSecondArg, Func<TArg,TArg,TArg> func)
        {
            _execute = () => func(nodeFirstArg.Execute(), nodeSecondArg.Execute());
        }
        //для параметра
        public ExpressionNode(TKey paramName, Func<TKey, TArg> getValue)
        {
            _execute = () => getValue(paramName);
        }
        public TArg Execute()
        {
            return _execute();
        }
    }
}
