﻿/*
    Copyright 2015 MCGalaxy
        
    Dual-licensed under the Educational Community License, Version 2.0 and
    the GNU General Public License, Version 3 (the "Licenses"); you may
    not use this file except in compliance with the Licenses. You may
    obtain a copy of the Licenses at
    
    https://opensource.org/license/ecl-2-0/
    https://www.gnu.org/licenses/gpl-3.0.html
    
    Unless required by applicable law or agreed to in writing,
    software distributed under the Licenses are distributed on an "AS IS"
    BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
    or implied. See the Licenses for the specific language governing
    permissions and limitations under the Licenses.
 */
using System;

namespace Flames.Config 
{    
    public abstract class ConfigSignedIntegerAttribute : ConfigAttribute 
    {
        public ConfigSignedIntegerAttribute(string name, string section) 
            : base(name, section) { }
         
        // separate function to avoid boxing in derived classes
        protected long ParseLong(string raw, long def, long min, long max) {
            long value;
            if (!long.TryParse(raw, out value)) {
                Logger.Log(LogType.Warning, "Config key \"{0}\" has invalid integer '{2}', using default of {1}", Name, def, raw);
                value = def;
            }
            
            if (value < min) {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too small an integer, using {1}", Name, min);
                value = min;
            }
            if (value > max) {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too big an integer, using {1}", Name, max);
                value = max;
            }
            return value;
        }
        protected int ParseInteger(string raw, int def, int min, int max)
        {
            int value;
            if (!int.TryParse(raw, out value))
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" has invalid integer '{2}', using default of {1}", Name, def, raw);
                value = def;
            }

            if (value < min)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too small an integer, using {1}", Name, min);
                value = min;
            }
            if (value > max)
            {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too big an integer, using {1}", Name, max);
                value = max;
            }
            return value;
        }
    }

    public sealed class ConfigIntAttribute : ConfigSignedIntegerAttribute 
    {
        int defValue, minValue, maxValue;
        
        public ConfigIntAttribute()
            : this(null, null, 0, int.MinValue, int.MaxValue) { }
        public ConfigIntAttribute(string name, string section, int def,
                                  int min = int.MinValue, int max = int.MaxValue)
            : base(name, section) { defValue = def; minValue = min; maxValue = max; }
        
        public override object Parse(string value) {
            return ParseInteger(value, defValue, minValue, maxValue);
        }
    }
    public class ConfigByteAttribute : ConfigSignedIntegerAttribute
    {
        public ConfigByteAttribute() : this(null, null) { }
        public ConfigByteAttribute(string name, string section) : base(name, section) { }

        public override object Parse(string raw)
        {
            return (byte)ParseInteger(raw, 0, 0, byte.MaxValue);
        }
    }
    public class ConfigShortAttribute : ConfigSignedIntegerAttribute
    {
        public ConfigShortAttribute() : this(null, null) { }
        public ConfigShortAttribute(string name, string section) : base(name, section) { }

        public override object Parse(string raw)
        {
            return (short)ParseInteger(raw, 0, 0, short.MaxValue);
        }
    }
    public abstract class ConfigRealAttribute : ConfigAttribute 
    {
        public ConfigRealAttribute(string name, string section) 
            : base(name, section) { }
        
        protected double ParseReal(string raw, double def, double min, double max) {
            double value;
            if (!Utils.TryParseDouble(raw, out value)) {
                Logger.Log(LogType.Warning, "Config key \"{0}\" has invalid number '{2}', using default of {1}", Name, def, raw);
                value = def;
            }
            
            if (value < min) {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too small a number, using {1}", Name, min);
                value = min;
            }
            if (value > max) {
                Logger.Log(LogType.Warning, "Config key \"{0}\" is too big a number, using {1}", Name, max);
                value = max;
            }
            return value;
        }

        public override string Serialise(object value) {
            if (value is float)  return Utils.StringifyDouble((float)value);
            if (value is double) return Utils.StringifyDouble((double)value);
            return base.Serialise(value);
        }
    }
    
    public class ConfigFloatAttribute : ConfigRealAttribute 
    {
        float defValue, minValue, maxValue;
        
        public ConfigFloatAttribute()
            : this(null, null, 0, float.NegativeInfinity, float.PositiveInfinity) { }
        public ConfigFloatAttribute(string name, string section, float def,
                                    float min = float.NegativeInfinity, float max = float.PositiveInfinity)
            : base(name, section) { defValue = def; minValue = min; maxValue = max; }
        
        public override object Parse(string raw) {
            return (float)ParseReal(raw, defValue, minValue, maxValue);
        }
    }
    
    public class ConfigTimespanAttribute : ConfigRealAttribute 
    {
        bool mins; int def;
        public ConfigTimespanAttribute(string name, string section, int def, bool mins)
            : base(name, section) { this.def = def; this.mins = mins; }
        
        public override object Parse(string raw) {
            double value = ParseReal(raw, def, 0, int.MaxValue);
            return ParseInput(value);
        }
        
        protected TimeSpan ParseInput(double value) {
            if (mins) {
                return TimeSpan.FromMinutes(value);
            } else {
                return TimeSpan.FromSeconds(value);
            }
        }
        
        public override string Serialise(object value) {
            TimeSpan span = (TimeSpan)value;
            double time = mins ? span.TotalMinutes : span.TotalSeconds;
            return time.ToString();
        }
    }
    
    public class ConfigOptTimespanAttribute : ConfigTimespanAttribute 
    {
        public ConfigOptTimespanAttribute(string name, string section, bool mins)
            : base(name, section, -1, mins) { }
        
        public override object Parse(string raw) {
            if (string.IsNullOrEmpty(raw)) return null;
        	
            double value = ParseReal(raw, -1, -1, int.MaxValue);
            if (value < 0) return null;
            
            return ParseInput(value);
        }
        
        public override string Serialise(object value) {
            if (value == null) return "";
            
            return base.Serialise(value);
        }
    }
}