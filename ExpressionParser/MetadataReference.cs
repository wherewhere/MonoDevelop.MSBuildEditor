//
// MetadataReference.cs: Represents a metadata reference in expression.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;

namespace MonoDevelop.MSBuildEditor.ExpressionParser
{
	internal class MetadataReference : IReference {
	
		string		itemName;
		string		metadataName;
		int start;
		int length;
		string original;
	
		public MetadataReference (string original, string itemName, string metadataName, int start, int length)
		{
			this.original = original;
			this.itemName = itemName;
			this.metadataName = metadataName;
			this.start = start;
			this.length = length;
		}
		
		public string ItemName {
			get { return itemName; }
		}
		
		public string MetadataName {
			get { return metadataName; }
		}
		
		public bool IsQualified {
			get { return (itemName == null) ? false : true; }
		}

		public int Start {
			get { return start; }
		}

		public int End {
			get { return start + length - 1; }
		}

		public override string ToString ()
		{
			if (IsQualified)
				return String.Format ("%({0}.{1})", itemName, metadataName);
			else
				return String.Format ("%({0})", metadataName);
		}

		public string OriginalString {
			get { return original; }
		}
	}
}