﻿using System;

namespace GalacticScale
{
    public class Val
    {
        public object val;

        public Val(object o)
        {
            val = o;
        }

        public bool empty => IsNull();

        public string String()
        {
            return val.ToString();
        }
        public ValueTuple<float, float> FloatFloat()
        {
            var v = val.ToString().Split('(', ',', ')');
            float.TryParse(v[1], out float i);
            float.TryParse(v[2], out float j);
            return (i,j);
        }
        public int Int(int def = -1)
        {
            if (int.TryParse(ToString(), out var i)) return i;
            GS2.Error("Failed to parse int");
            return def;
        }

        public float Float(float def = -1f)
        {
            if (float.TryParse(ToString(), out var f)) return f;
            if (double.TryParse(ToString(), out var d)) return (float) d;
            if (int.TryParse(ToString(), out var i)) return i;
            GS2.Error("Failed to parse float " + val);
            return def;
        }

        public double Double(double def = -1.0)
        {
            if (double.TryParse(ToString(), out var i)) return i;
            GS2.Error("Failed to parse double");
            return def;
        }

        public bool Bool(bool def = false)
        {
            if (bool.TryParse(ToString(), out var i)) return i;
            GS2.Error("Failed to parse bool");
            return def;
        }

        public GSSliderConfig GSSliderConfig(GSSliderConfig def = new GSSliderConfig())
        {
            if (val is GSSliderConfig) return (GSSliderConfig) val;
            GS2.Error("Failed to parse GSSliderConfig");
            return def;
        }

        public bool IsNull()
        {
            return val == null;
        }

        public static implicit operator Val(int i)
        {
            return new Val(i);
        }

        public static implicit operator Val(float i)
        {
            return new Val(i);
        }

        public static implicit operator Val(double i)
        {
            return new Val(i);
        }

        public static implicit operator Val(bool i)
        {
            return new Val(i);
        }

        public static implicit operator Val(string i)
        {
            return new Val(i);
        }
        public static implicit operator Val(ValueTuple<float, float> i) => new Val(i);
        public static implicit operator ValueTuple<float, float>(Val v) => v.FloatFloat();
        public static implicit operator int(Val v)
        {
            return v.Int();
        }

        public static implicit operator float(Val v)
        {
            return v.Float();
        }

        public static implicit operator bool(Val v)
        {
            return v.Bool();
        }

        public static implicit operator string(Val v)
        {
            return v.String();
        }

        public static implicit operator double(Val v)
        {
            return v.Double();
        }

        public static implicit operator GSSliderConfig(Val v)
        {
            return v.GSSliderConfig();
        }

        public static implicit operator Val(GSSliderConfig g)
        {
            return new Val(g);
        }

        public static bool operator ==(Val left, object right)
        {
            return left.val == right;
        }

        public static bool operator !=(Val left, object right)
        {
            return left.val != right;
        }

        public override int GetHashCode()
        {
            return val.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return val.Equals(obj);
        }

        public override string ToString()
        {
            if (val != null) return val.ToString();
            return null;
        }
    }
}