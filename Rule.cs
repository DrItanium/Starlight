//Copyright 2012 Joshua Scoggins. All rights reserved.
//
//Redistribution and use in source and binary forms, with or without modification, are
//permitted provided that the following conditions are met:
//
//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.
//
//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY Joshua Scoggins ``AS IS'' AND ANY EXPRESS OR IMPLIED
//WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Joshua Scoggins OR
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
//ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//The views and conclusions contained in the software and documentation are those of the
//authors and should not be interpreted as representing official policies, either expressed
//or implied, of Joshua Scoggins. 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Libraries.Starlight
{
	public class Rule : List<Production>, ICloneable, IRule
	{
#if GATHERING_STATS
		public static long InstanceCount = 0, DeleteCount = 0;
#endif
		private string name;
		public string Name { get { return name; } set { name = value; } }
		public Rule(string name)
		{
#if GATHERING_STATS
			InstanceCount++;
#endif
			this.name = name;
		}
		public Rule(Rule r)
			: this(r.Name)
		{
			foreach(var v in r)
				Add(v); //why do we need copies?
		}
#if GATHERING_STATS
		~Rule()
		{
			DeleteCount++;
		}
#endif
		public override bool Equals(object other)
		{
			Rule r = (Rule)other;
			if(r.Name.Equals(Name) && r.Count == Count)
			{
				for(int i = 0; i < Count; i++)
					if(!r[i].Equals(this[i]))
						return false;
				return true;
			}
			else
				return false;
		}
		public override int GetHashCode()
		{
			long total = 0L;
			for(int i = 0; i < Count; i++)
				total += this[i].GetHashCode();
			return Name.GetHashCode() + (int)total;
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{0} => ", Name);
			foreach(Production p in this)
				sb.AppendFormat("{0} | ",p.ToString());
			return sb.ToString();
		}
		public AdvanceableRule MakeAdvanceable(int offset)
		{
			return new AdvanceableRule(this, offset);
		}
		public AdvanceableRule MakeAdvanceable()
		{
			return new AdvanceableRule(this);
		}
#if GATHERING_STATS
		public static long SubdivideCalls = 0L;
#endif
		public IEnumerable<IFixedRule> Subdivide()
		{
#if GATHERING_STATS
			SubdivideCalls++;
#endif
			List<IFixedRule> fr = new List<IFixedRule>();
			for(int i = 0; i < Count; i++)
				fr.Add(new FixedRule(this, i));
			return fr;
				//yield return new FixedRule(this, i);
		}
		public object Clone()
		{
			return new Rule(this);
		}
		private IEnumerable<Production> GetValid()
		{
			return (IEnumerable<Production>)this;
		}
		IEnumerable<IProduction> IRule.GetEnumerableInterface()
		{
			return (from x in this 
					    select (IProduction)x);
		}	
		IProduction IRule.this[int index] { get { return this[index]; } }
	}
}
