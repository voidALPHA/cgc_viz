// Copyright 2017 voidALPHA, Inc.
// This file is part of the Haxxis video generation system and is provided
// by voidALPHA in support of the Cyber Grand Challenge.
// Haxxis is free software: you can redistribute it and/or modify it under the terms
// of the GNU General Public License as published by the Free Software Foundation.
// Haxxis is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
// PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with
// Haxxis. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mutation;
using Mutation.Mutators;
using Visualizers;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using System.Reflection;
using UnityEngine;

namespace Experimental
{
    public class ExperimentalUsercodeMutator : Mutator
    {
        private MutableField<string> m_UserCode = new MutableField<string>()
        {
            UseMutableData = false, LiteralValue = "Debug.Log(\"Hello World\");"
        };
        [Controllable(LabelText = "Mutation Code")]
        public MutableField<string> UserCode { get { return m_UserCode; } } 

        public override IEnumerator ReceivePayload(VisualPayload payload)
        {
            CompilerParameters CompilerParams = new CompilerParameters();
            string outputDirectory = Directory.GetCurrentDirectory();

            CompilerParams.GenerateInMemory = true;
            CompilerParams.TreatWarningsAsErrors = false;
            CompilerParams.GenerateExecutable = false;
            CompilerParams.CompilerOptions = "/optimize";

#if !UNITY_EDITOR
            string[] references = { "System.dll", "UnityEngine.dll", "Assembly-CSharp.dll" };
#else
            string[] references = { "System.dll", "Library/ScriptAssemblies/Assembly-CSharp.dll", "Library/UnityAssemblies/UnityEngine.dll" };
#endif
            CompilerParams.ReferencedAssemblies.AddRange(references);

            CSharpCodeProvider provider = new CSharpCodeProvider();
            string code = @"
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UserCode {
    public class UserCode {
        public MutableObject ExecuteCode(MutableObject mo) {
            UserProcess(mo);
            return mo;
        }
        private void UserProcess(MutableObject data) {
            " + UserCode.GetFirstValue(payload.Data) + @"
        }
    }
}";

            CompilerResults compile = provider.CompileAssemblyFromSource(CompilerParams, code);

            if(compile.Errors.HasErrors)
            {
                Debug.LogError("Compile error in user code!");
                foreach(CompilerError ce in compile.Errors)
                {
                    Debug.LogError(ce.ToString());
                }
                yield break;
            }

            Module module = compile.CompiledAssembly.GetModules()[0];
            Type mt = null;
            MethodInfo methInfo = null;

            if(module != null)
            {
                mt = module.GetType("UserCode.UserCode");
            }

            if(mt != null)
            {
                methInfo = mt.GetMethod("ExecuteCode");
            }

            if(methInfo != null)
            {
                Console.WriteLine(methInfo.Invoke(null, new object[] { payload.Data }));
            }
            yield return null;
        }
    }
}
