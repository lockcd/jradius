using JRadius.Core.Packet.Attribute.Value;
using System;

namespace JRadius.Core.Packet.Attribute
{
    public abstract class RadiusAttribute
    {
        protected long _attributeType = 0;
        protected int _attributeOp = Operator.SET;
        protected IAttributeValue _attributeValue = null;
        protected string _attributeName = "Unknown Attribute";

        protected bool _recyclable;
        protected bool _recycled;

        protected bool _overflow;

        public RadiusAttribute()
        {
        }

        public abstract void Setup();

        protected void Setup(object value)
        {
            Setup(value, Operator.SET);
        }

        protected void Setup(object o, int op)
        {
            Setup();
            _attributeOp = op;
            if (o == null) return;

            if (o is IAttributeValue value)
            {
                _attributeValue = value;
            }
            else
            {
                _attributeValue.SetValueObject(o);
            }
        }

        public long GetType()
        {
            return _attributeType;
        }

        public virtual long GetFormattedType()
        {
            return _attributeType;
        }

        public IAttributeValue GetValue()
        {
            return _attributeValue;
        }

        public string GetAttributeName()
        {
            return _attributeName;
        }

        public int GetAttributeOp()
        {
            return _attributeOp;
        }

        public void SetAttributeOp(int attributeOp)
        {
            _attributeOp = attributeOp;
        }

        public void SetAttributeOp(string attributeOp)
        {
            _attributeOp = Operator.OperatorFromString(attributeOp);
        }

        public void SetValue(byte[] b)
        {
            _attributeValue.SetValue(b);
        }

        public void SetValue(byte[] b, int off, int len)
        {
            _attributeValue.SetValue(b, off, len);
        }

        public void SetValue(string value)
        {
            _attributeValue.SetValue(value);
        }

        public override string ToString()
        {
            return $"{_attributeName} {Operator.OperatorToString(_attributeOp)} {_attributeValue}";
        }

        public sealed class Operator
        {
            public const int ADD = 8;   /* += */
            public const int SUB = 9;   /* -= */
            public const int SET = 10;  /* := */
            public const int EQ = 11;  /* = */
            public const int NE = 12;  /* != */
            public const int GE = 13;  /* >= */
            public const int GT = 14;  /* > */
            public const int LE = 15;  /* <= */
            public const int LT = 16;  /* < */
            public const int REG_EQ = 17;  /* =~ */
            public const int REG_NE = 18;  /* !~ */
            public const int CMP_TRUE = 19;  /* =* */
            public const int CMP_FALSE = 20;  /* !* */
            public const int CMP_EQ = 21;  /* == */

            public static string OperatorToString(int op)
            {
                switch (op)
                {
                    case ADD: return "+=";
                    case SUB: return "-=";
                    case SET: return ":=";
                    case EQ: return "=";
                    case NE: return "!=";
                    case GE: return ">=";
                    case GT: return ">";
                    case LE: return "<=";
                    case LT: return "<";
                    case REG_EQ: return "=~";
                    case REG_NE: return "!~";
                    case CMP_TRUE: return "=*";
                    case CMP_FALSE: return "!*";
                    case CMP_EQ: return "==";
                }
                return "="; // for display purposes
            }

            public static int OperatorFromString(string op)
            {
                if (op == null) return 0;
                if (op.Equals("+=")) return ADD;
                if (op.Equals("-=")) return SUB;
                if (op.Equals(":=")) return SET;
                if (op.Equals("=")) return EQ;
                if (op.Equals("!=")) return NE;
                if (op.Equals(">=")) return GE;
                if (op.Equals(">")) return GT;
                if (op.Equals("<=")) return LE;
                if (op.Equals("<")) return LT;
                if (op.Equals("=~")) return REG_EQ;
                if (op.Equals("!~")) return REG_NE;
                if (op.Equals("=*")) return CMP_TRUE;
                if (op.Equals("!*")) return CMP_FALSE;
                if (op.Equals("==")) return CMP_EQ;
                return 0;
            }
        }

        public void SetOverflow(bool b)
        {
            _overflow = b;
        }

        public bool IsOverflow()
        {
            return _overflow;
        }
    }
}
