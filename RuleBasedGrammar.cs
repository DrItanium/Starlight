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
	public abstract class RuleBasedGrammar<Encoding> : AbstractGrammar<Rule,Encoding>
		where Encoding : struct
	{
		private Dictionary<string, int> lookup;
		private HashSet<string> terminalSymbols, nonTerminalSymbols;
		private int numProductions;
		public override IEnumerable<string> SymbolTable { get { return terminalSymbols.Concat(nonTerminalSymbols); } }
		public override IEnumerable<string> TerminalSymbols { get { return terminalSymbols; } }
		public override IEnumerable<string> NonTerminalSymbols { get { return nonTerminalSymbols; } }
		public override int NumberOfProductions { get { return numProductions; } }

		protected RuleBasedGrammar(IEnumerable<Rule> rules)
			: this()
		{
			AddRange(rules);
		}
		protected RuleBasedGrammar() 
		{
			terminalSymbols = new HashSet<string>();
			nonTerminalSymbols = new HashSet<string>();
			lookup = new Dictionary<string, int>();
	 	}
		public override void UpdateSymbolTable()
		{
			foreach(Rule r in this)
			{
				if(terminalSymbols.Contains(r.Name))
					terminalSymbols.Remove(r.Name);
				nonTerminalSymbols.Add(r.Name);
			}
			foreach(Rule r in this)
			{
				foreach(Production p in r)
				{
					foreach(string str in p)
					{
						if(!nonTerminalSymbols.Contains(str))
							terminalSymbols.Add(str);
					}
				}
			}
		}


		public override IProduction LookupProduction(Encoding index)
		{
			return LookupRule(index)[0];
		}
		public override bool Exists(string rule)
		{
			return lookup.ContainsKey(rule);
		}
		public override Rule this[string name] { get { return this[lookup[name]]; } }
		public override int IndexOf(string name) { return lookup[name]; }
		protected override void Add_Impl(Rule r, bool delayUpdate)
		{

			if(lookup.ContainsKey(r.Name))
			{
				//add the production rules instead
				Rule curr = this[lookup[r.Name]];
				bool shouldUpdate = false;
				foreach(var v in r)
				{
					if(!curr.Contains(v)) //prevent empty from being duplicated
					{
						shouldUpdate = true;
						curr.Add(v);
						numProductions++;
					}
				}
				if(shouldUpdate && !delayUpdate) //prevent a case where no new information is given
					UpdateSymbolTable();
			}
			else
			{
				int oldCount = Count;
				//HACK HACK!
				BaseAdd(r); //prevents a cyclic loop
				numProductions += r.Count;
				lookup.Add(r.Name, oldCount);
				if(!delayUpdate)
					UpdateSymbolTable();
			}
		}
		protected override bool Remove_Impl(Rule r)
		{
			bool result = base.Remove(r);
			if(result)
				numProductions -= r.Count;
			return result;
		}
		protected override void RemoveAt_Impl(int index)
		{
			var result = this[index];
			base.RemoveAt(index);
			numProductions -= result.Count;
		}
		protected override Predicate<Rule> MakeRemoveAllFunction(Predicate<Rule> pred)
		{
			//YAY Closures!
			return (x) => 
			{
				bool result = pred(x);
				if(result)
					numProductions -= x.Count;
				return result;
			};
		}
		protected override void Clear_Impl()
		{
			numProductions = 0;
			lookup.Clear();
			terminalSymbols.Clear();
			nonTerminalSymbols.Clear();
		}
	}
}
