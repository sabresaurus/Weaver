﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using Weaver.Extensions;

namespace Weaver.Addin.MethodTimer.Editor
{
    public class MethodTimerAddin : WeaverAddin
    {
        public override string Name => "Method Timer";

        private StopwatchDefinition m_stopwatchDefinition;
        private StringDefinition m_stringDefinition;

        // Timer
        private FieldReference m_timerDelegateField;
        private MethodReference m_timerDelegateInvoke; 

        public override void VisitModule(ModuleDefinition moduleDefinition)
        {
            base.VisitModule(moduleDefinition);
            m_stopwatchDefinition = new StopwatchDefinition(moduleDefinition);
            m_stringDefinition = new StringDefinition(moduleDefinition);

            moduleDefinition.ImportFluent<MethodTimerAttribute>()
                .GetStaticField(() => MethodTimerAttribute.OnMethodLogged, out m_timerDelegateField);

            moduleDefinition.ImportFluent<MethodTimerAttribute.MethodTimerDelegate>()
                .GetMethod( d=> d.Invoke("", "", new TimeSpan()), out m_timerDelegateInvoke);
        }

        public override void VisitMethod(MethodDefinition methodDefinition)
        {
            CustomAttribute attribute = methodDefinition.GetAttribute<MethodTimerAttribute>();
            if (attribute == null)
            {
                return;
            }

            methodDefinition.CustomAttributes.Remove(attribute);


            MethodBody body = methodDefinition.Body;
            Collection<Instruction> instructions = body.Instructions;

            VariableDefinition stopwatchVariable = new VariableDefinition(m_stopwatchDefinition.Reference);
            VariableDefinition elapsedMilliseconds = new VariableDefinition(TypeSystem.Int64);

            body.Variables.Add(stopwatchVariable);
            body.Variables.Add(elapsedMilliseconds);

            InjectStart(instructions, stopwatchVariable);

            for (int i = body.Instructions.Count - 1; i >= 0; i--)
            {
                Instruction instruction = body.Instructions[i];

                Instruction previous = instruction.Previous;

                if (instruction.OpCode == OpCodes.Ret)
                {
                    InjectLog(i, methodDefinition, body.Instructions, stopwatchVariable);
                }
            }
        }

        private void InjectLog(int index, MethodDefinition method, Collection<Instruction> instructions, VariableDefinition stopwatchVariable)
            => instructions.InsertRange(index, new []
            {
                Instruction.Create(OpCodes.Ldloc, stopwatchVariable),
                Instruction.Create(OpCodes.Callvirt, m_stopwatchDefinition.Stop),

                // Set up parameters
                Instruction.Create(OpCodes.Ldsfld, m_timerDelegateField),
                Instruction.Create(OpCodes.Ldstr, method.DeclaringType.FullName),
                Instruction.Create(OpCodes.Ldstr, method.Name),
                Instruction.Create(OpCodes.Ldloc, stopwatchVariable),
                Instruction.Create(OpCodes.Callvirt, m_stopwatchDefinition.Elapsed), 
                Instruction.Create(OpCodes.Callvirt, m_timerDelegateInvoke)
            });

        /// <summary>
        /// Injects the timer into our method and invokes start on it. 
        /// </summary>
        /// <param name="instructions">The instructions.</param>
        /// <param name="stopwatch">The stopwatch.</param>
        private void InjectStart(Collection<Instruction> instructions, VariableDefinition stopwatchVariable)
            => instructions.InsertRange(0, new[] {
                 Instruction.Create(OpCodes.Newobj, m_stopwatchDefinition.Consturctor),
                 Instruction.Create(OpCodes.Stloc, stopwatchVariable),
                 Instruction.Create(OpCodes.Ldloc, stopwatchVariable),
                 Instruction.Create(OpCodes.Callvirt, m_stopwatchDefinition.Start)});
    }
}
