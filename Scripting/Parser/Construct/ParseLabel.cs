﻿using System.CodeDom;
using System.Collections.Generic;

namespace IronAHK.Scripting
{
    partial class Parser
    {
        CodeMethodInvokeExpression ParseLabel(CodeLine line)
        {
            string code = line.Code;
            int z = code.Length - 1;
            string name = z > 0 ? code.Substring(0, z) : string.Empty;
            if (code.Length < 2 || code[z] != HotkeyBound || !IsIdentifier(name))
                throw new ParseException("Invalid label name");

            PushLabel(line, name, true);

            return LocalLabelInvoke(name);
        }

        void PushLabel(CodeLine line, string name, bool fallthrough)
        {
            var last = CheckTopBlock();

            if (fallthrough && last != null)
                last.Statements.Add(LocalLabelInvoke(name));

            var method = LocalMethod(name);
            var block = new CodeBlock(line, method.Name, method.Statements, CodeBlock.BlockKind.Label) { Type = CodeBlock.BlockType.Within };
            blocks.Push(block);

            methods.Add(method.Name, method);
        }

        CodeMethodInvokeExpression LocalLabelInvoke(string name)
        {
            var invoke = (CodeMethodInvokeExpression)InternalMethods.LabelCall;
            invoke.Parameters.Add(new CodePrimitiveExpression(name));
            return invoke;
        }

        CodeBlock CheckTopBlock()
        {
            if (blocks.Count != 0 && blocks.Peek().Kind == CodeBlock.BlockKind.Label)
                return blocks.Pop();
            return null;
        }
    }
}
