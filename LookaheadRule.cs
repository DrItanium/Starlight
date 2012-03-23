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
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Libraries.Starlight
{
	public class LookaheadRule : AdvanceableRule, ILookaheadRule
	{
		private string lookaheadSymbol;
		string ILookaheadRule.LookaheadSymbol { get { return lookaheadSymbol; } }
		public string LookaheadSymbol 
		{
		 	get
			{
				return lookaheadSymbol;
			}
			set
			{
				lookaheadSymbol = value;
			}
		}
		public LookaheadRule(string lookaheadSymbol, Rule rule, int offset)
			: base(rule, offset)
		{
			this.lookaheadSymbol = lookaheadSymbol;
		}
		public LookaheadRule(string lookaheadSymbol, string name, params AdvanceableProduction[] prods)
			: base(name, prods)
		{
			this.lookaheadSymbol = lookaheadSymbol;
		}
		public LookaheadRule() : this(string.Empty, null, 0)
		{

		}
		public LookaheadRule(string lookaheadSymbol, Rule rule)
			: this(lookaheadSymbol, rule, 0)
		{
		}
		public LookaheadRule(string lookaheadSymbol, AdvanceableRule rule)
			: base(rule)
		{
			this.lookaheadSymbol = lookaheadSymbol;
		}
		public LookaheadRule(LookaheadRule lr)
			: base(lr)
		{
			lookaheadSymbol = lr.lookaheadSymbol;
		}
		public void Repurpose(string lookahead, Rule r)
		{
		   Repurpose(lookahead, new AdvanceableRule(r));
		}
		public void Repurpose(string lookahead, AdvanceableRule r)
		{
			Clear();
			if(!r.Name.Equals(base.Name))
			  base.Name = r.Name;
			if(!lookahead.Equals(LookaheadSymbol))
				lookaheadSymbol = lookahead;
			for(int i = 0; i < r.Count; i++)
				Add(r[i]);
		}
		public override bool Equals(object other)
		{
			LookaheadRule lr = (LookaheadRule)other;
			return lr.lookaheadSymbol.Equals(lookaheadSymbol) && base.Equals(lr);
		}
		private string ToString0(AdvanceableProduction production)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("[{0} => ",Name);
			for(int i = 0;i < production.Min; i++)
				sb.AppendFormat("{0} ", production[i]);	
			sb.Append("!");
			for(int i = production.Min;i < production.Position; i++)
				sb.AppendFormat("{0} ", production[i]);
			sb.Append("@");
			for(int i = production.Position; i < production.Max; i++)
				sb.AppendFormat("{0} ", production[i]);
			sb.AppendFormat(", {0}]", LookaheadSymbol);
			return sb.ToString();
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach(var v in this)
				sb.AppendLine(ToString0(v));
			return sb.ToString();
		}

		public override int GetHashCode()
		{
			long l = lookaheadSymbol.GetHashCode() + base.GetHashCode();
			return (int)l;
		}
		public new object Clone()
		{
			return new LookaheadRule(this);
		}
	}
}
