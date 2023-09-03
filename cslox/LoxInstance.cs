using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class LoxInstance
    {
        LoxClass klass;

        readonly Dictionary<string, object?> fields = new();
        
        internal LoxInstance( LoxClass klass )
        {
            this.klass = klass;
        }

        // 字段是直接保存在实例中的命名状态
        internal object? Get(Token name)
        {
            if(fields.TryGetValue(name.lexeme, out var value)) return value;

            LoxCallable_Function? method = klass.FindMethod(name.lexeme);
            if (method != null)
                return method.Bind(this);

            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'");
        }

        internal void Set(Token name, object? value)
        {
            fields[name.lexeme] = value;
        }

        public override string ToString()
        {
            return klass.ToString() + " instance";
        }
    }
}
