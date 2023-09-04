using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cslox
{
    internal class LoxClass : LoxInstance, ILoxCallable
    {
        readonly string name;

        readonly Dictionary<string, LoxCallable_Function> methods;

        internal LoxClass(LoxClass metaClass, string name, Dictionary<string, LoxCallable_Function> methods) : base(metaClass)
        {
            this.name = name;
            this.methods = methods;
        }

        public int Arity()
        {
            LoxCallable_Function? initializer = FindMethod("init");
            if (initializer != null)
                return initializer.Arity();
            return 0;
        }

        public object? Call(Interpreter interpreter, List<object?> arguments)
        {
            LoxInstance instance = new(this);
            LoxCallable_Function? initializer = FindMethod("init");
            if (initializer != null)
                initializer.Bind(instance).Call(interpreter, arguments);
            return instance;
        }

        internal LoxCallable_Function? FindMethod(string name)
        {
            if (methods.TryGetValue(name, out LoxCallable_Function? initializer)) return initializer;
            return null;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
