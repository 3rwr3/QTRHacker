﻿using QTRHacker.Functions.ProjectileImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QTRHacker.ProjMaker.Parse
{
	public enum TokenType
	{
		LEFT_BRACKET,
		RIGHT_BRACKET,
		LABEL,
		NAME,
		NUMBER,
		COMMA,
	}
	public class Token
	{
		public string Value
		{
			get;
		}
		public TokenType Type
		{
			get;
		}
		public int Index
		{
			get;
		}
		public Token(string Value, TokenType Type, int Index)
		{
			this.Value = Value;
			this.Type = Type;
			this.Index = Index;
		}
	}
	public class Tokenizer
	{
		public Dictionary<string, Func<float, float, IEnumerable<Proj>>> Labels
		{
			get;
		}
		public string Source
		{
			get;
		}
		public int Index
		{
			get;
			private set;
		}
		public Tokenizer(string s, Dictionary<string, Func<float, float, IEnumerable<Proj>>> handler)
		{
			Source = s;
			Index = 0;
			Labels = handler;
		}
		public Token Next()
		{
			if (Index >= Source.Length)
				return null;
			if (Source[Index] == '\n' || Source[Index] == ' ' || Source[Index] == '\t' || Source[Index] == '\r')
			{
				Index++;
				return Next();
			}
			else if (Source[Index] == '#')
			{
				Index++;
				while (Index < Source.Length && Source[Index] != '\n' && Source[Index] != '#') Index++;
				return Next();
			}
			else if (Char.IsNumber(Source[Index]) || Source[Index] == '-')
			{
				string n = "";
				int i = Index;
				while (Index < Source.Length && (Char.IsNumber(Source[Index]) || Source[Index] == '.' || Source[Index] == '-'))
					n += Source[Index++];
				return new Token(n, TokenType.NUMBER, i);
			}
			else if (Char.IsLetter(Source[Index]))
			{
				string n = "";
				int i = Index;
				while (Index < Source.Length && (Char.IsLetterOrDigit(Source[Index]) || Source[Index] == '_'))
					n += Source[Index++];
				if (Labels.Keys.Contains(n))
					return new Token(n, TokenType.LABEL, i);
				return new Token(n, TokenType.NAME, i);
			}
			else if (Source[Index] == '(')
				return new Token(Source[Index].ToString(), TokenType.LEFT_BRACKET, Index++);
			else if (Source[Index] == ')')
				return new Token(Source[Index].ToString(), TokenType.RIGHT_BRACKET, Index++);
			else if (Source[Index] == ',')
				return new Token(Source[Index].ToString(), TokenType.COMMA, Index++);
			throw new ParseException("un," + Index);//未知Token
		}
	}
}