﻿using System.Linq;
using CommandDotNet.TestTools;
using CommandDotNet.Tokens;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CommandDotNet.Tests.FeatureTests
{
    public class CustomTokenTransformationTests
    {
        private readonly ITestOutputHelper _output;

        public CustomTokenTransformationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void ParseDirective_OutputsResults()
        {
            var result = new AppRunner<App>()
                .UseParseDirective()
                .Configure(c =>
                    c.UseTokenTransformation("test", 1,
                        (ctx, tokens) => new TokenCollection(tokens.Select(t =>
                            t.TokenType == TokenType.Value && t.Value != "Do"
                                ? Tokenizer.TokenizeValue("roses")
                                : t))))
                .RunInMem("[parse] Do --opt1 smells like".SplitArgs(), _output);

            result.OutputShouldBe(@"use [parse:verbose] to see results after each transformation
>>> from shell
  Directive: [parse]
  Value    : Do
  Option   : --opt1
  Value    : smells
  Value    : like
>>> transformed after: test > expand-clubbed-flags > split-option-assignments
  Directive: [parse]
  Value    : Do
  Option   : --opt1
  Value    : roses
  Value    : roses");
        }

        [Fact]
        public void CanRegisterCustomTokenTransformation()
        {
            var result = new AppRunner<App>()
                .Configure(c =>
                    c.UseTokenTransformation("test", 1,
                        (ctx, tokens) => new TokenCollection(tokens.Select(t =>
                            t.TokenType == TokenType.Value && t.Value != "Do"
                                ? Tokenizer.TokenizeValue("roses")
                                : t))))
                .RunInMem("Do --opt1 smells like".SplitArgs(), _output);

            result.TestOutputs.Get<string>().Should().Be("roses");
        }

        public class App
        {
            private TestOutputs TestOutputs { get; set; }

            public int Do([Option] string opt1, string arg1)
            {
                TestOutputs.Capture(opt1);
                return opt1 == arg1 ? 0 : 1;
            }
        }
    }
}