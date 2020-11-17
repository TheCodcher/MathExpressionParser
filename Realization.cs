using System;
using System.Collections.Generic;
using System.Text;

namespace SomeParseSolution
{
    class RealNumbersExpressionCalculator : ExpressionCalculator<string, double>
    {
        protected override MathOperation<string, double>[] InicializeOperations()
        {
            return new[]
            {
               new MathOperation<string, double>("+",(a,b)=>a+b),
               new MathOperation<string, double>("-",(a)=>-a,2),
               new MathOperation<string, double>("*",(a,b)=>a*b,3),
               new MathOperation<string, double>("/",(a,b)=>a/b,3),
               new MathOperation<string, double>("cos",(a)=>Math.Cos(a),5),
               new MathOperation<string, double>("sin",(a)=>Math.Sin(a),5),
               new MathOperation<string, double>("^",(a,b)=>Math.Pow(a,b),4)
           };
        }
    }
    class RealNumbersExpressionParser : ExpressionParser<double>
    {
        static Dictionary<string, int> defaultDividers = new Dictionary<string, int>()
        {
            {"(",6 },
            { ")",-6}
        };
        public RealNumbersExpressionParser() : base(new RealNumbersExpressionCalculator(), defaultDividers) { }
        public override ParseExpressionResult<string,double> Parse(string source)
        {
            source = source[0] + source.Substring(1).Replace("-", "+-");
            return base.Parse(source);
        }
    }
}
