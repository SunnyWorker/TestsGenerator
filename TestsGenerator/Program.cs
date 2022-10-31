// See https://aka.ms/new-console-template for more information

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestsGenerator;



var pipeline = new Pipeline(new PipelineConfiguration(3,3,3),"D:\\Unik\\СПП\\TestsGenerator\\outputTests");
List<string> files = new() { "D:\\Unik\\СПП\\TestsGenerator\\TestsGenerator\\SampleClass.cs" };
await pipeline.PerformProcessing(files);
