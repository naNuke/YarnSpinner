﻿using Xunit;
using System;
using System.Collections;
using System.Collections.Generic;
using Yarn;
using System.IO;
using System.Linq;

using Yarn.Compiler;

namespace YarnSpinner.Tests
{


    public class DialogueTests : TestBase
    {
        [Fact]
        public void TestNodeExists ()
        {
            var path = Path.Combine(UnityDemoScriptsPath, "Sally.yarn");

            Compiler.CompileFile(path, out var program, out stringTable);

            dialogue.SetProgram (program);

            Assert.True (dialogue.NodeExists ("Sally"));

            // Test clearing everything
            dialogue.UnloadAll ();

            Assert.False (dialogue.NodeExists ("Sally"));

        }

        [Fact]
        public void TestAnalysis() 
        {

            ICollection<Yarn.Analysis.Diagnosis> diagnoses;
            Yarn.Analysis.Context context;

            // this script has the following variables:
            // $foo is read from and written to
            // $bar is written to but never read
            // $bas is read from but never written to
            // this means that there should be two diagnosis results
            context = new Yarn.Analysis.Context (typeof(Yarn.Analysis.UnusedVariableChecker));

            var path = Path.Combine(TestDataPath, "AnalysisTest.yarn.txt");

            Compiler.CompileFile(path, out var program, out stringTable);

            dialogue.SetProgram(program);
            dialogue.Analyse (context);
            diagnoses = new List<Yarn.Analysis.Diagnosis>(context.FinishAnalysis ());

            Assert.Equal (2, diagnoses.Count);

            dialogue.UnloadAll ();

            context = new Yarn.Analysis.Context (typeof(Yarn.Analysis.UnusedVariableChecker));

            string shipPath = Path.Combine(UnityDemoScriptsPath, "Ship.yarn");
            Compiler.CompileFile(shipPath, out var shipProgram, out var shipStringTable);

            string sallyPath = Path.Combine(UnityDemoScriptsPath, "Sally.yarn");
            Compiler.CompileFile(sallyPath, out var sallyProgram, out var sallyStringTable);

            stringTable = shipStringTable.Union(sallyStringTable).ToDictionary(k => k.Key, v => v.Value);            

            var combinedProgram = Program.Combine(shipProgram, sallyProgram);

            dialogue.SetProgram (combinedProgram);
            
            dialogue.Analyse (context);
            diagnoses = new List<Yarn.Analysis.Diagnosis>(context.FinishAnalysis ());

            // This script should contain no unused variables
            Assert.Empty (diagnoses);
        }

        [Fact]
        public void TestDumpingCode()
        {

            var path = Path.Combine(TestDataPath, "Example.yarn.txt");

            Compiler.CompileFile(path, out var program, out stringTable);

            dialogue.SetProgram (program);

            var byteCode = dialogue.GetByteCode ();
            Assert.NotNull (byteCode);

        }

        [Fact]
        public void TestMissingNode()
        {
            var path = Path.Combine (TestDataPath, "TestCases", "Smileys.yarn.txt");

            Compiler.CompileFile(path, out var program, out stringTable);
            
            dialogue.SetProgram (program);

            errorsCauseFailures = false;

            dialogue.SetNode("THIS NODE DOES NOT EXIST");
            dialogue.Continue();
        }

        [Fact]
        public void TestGettingCurrentNodeName()  {

            string path = Path.Combine(UnityDemoScriptsPath, "Sally.yarn");
            Compiler.CompileFile(path, out var program, out stringTable);
            
            dialogue.SetProgram (program);

            // dialogue should not be running yet
            Assert.Null (dialogue.currentNode);

            dialogue.SetNode("Sally");
            Assert.Equal ("Sally", dialogue.currentNode);

            dialogue.Stop();
            // Current node should now be null
            Assert.Null (dialogue.currentNode);
        }

        [Fact]
        public void TestGettingRawSource() {

            var path = Path.Combine(TestDataPath, "Example.yarn.txt");

            Compiler.CompileFile(path, out var program, out stringTable);
            dialogue.SetProgram (program);

            var sourceID = dialogue.GetStringIDForNode ("LearnMore");
            var source = stringTable[sourceID].text;

            Assert.NotNull (source);

            Assert.Equal ("A: HAHAHA", source);
        }
		[Fact]
		public void TestGettingTags() {

            var path = Path.Combine(TestDataPath, "Example.yarn.txt");
            Compiler.CompileFile(path, out var program, out stringTable);
			dialogue.SetProgram (program);

			var source = dialogue.GetTagsForNode ("LearnMore");

			Assert.NotNull (source);

			Assert.NotEmpty (source);

			Assert.Equal ("rawText", source.First());
		}

        [Fact]
        public void TestJumpingToLastRunLineInstruction()
        {
            string path = System.IO.Path.Combine(TestDataPath, "Example.yarn.txt");
            Yarn.Compiler.Compiler.CompileFile(path, out var program, out stringTable);

            dialogue.SetProgram(program);

            // Set node and line to start at
            dialogue.SetNode("Start");

            // Set instruction that IS NOT RunLine
            bool notRunLine = dialogue.JumpToRunLineInstruction(4);
            Assert.False(notRunLine);

            // Set instruction that IS RunLine
            bool isRunLine = dialogue.JumpToRunLineInstruction(6);
            Assert.True(isRunLine);

            dialogue.Stop();
        }

	}
}

