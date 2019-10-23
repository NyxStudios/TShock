﻿// Copyright (c) 2019 Pryaxis & TShock Contributors
// 
// This file is part of TShock.
// 
// TShock is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// TShock is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with TShock.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using FluentAssertions;
using TShock.Commands.Exceptions;
using Xunit;

namespace TShock.Commands.Parsers {
    public class Int32ParserTests {
        [Theory]
        [InlineData("1234", 1234, "")]
        [InlineData("+1234", 1234, "")]
        [InlineData("000", 0, "")]
        [InlineData("-1234", -1234, "")]
        [InlineData("123 test", 123, " test")]
        public void Parse(string inputString, int expected, string expectedNextInput) {
            var parser = new Int32Parser();
            var input = inputString.AsSpan();

            parser.Parse(ref input, new HashSet<Attribute>()).Should().Be(expected);

            input.ToString().Should().Be(expectedNextInput);
        }

        [Theory]
        [InlineData("")]
        [InlineData("    ")]
        public void Parse_MissingInteger_ThrowsParseException(string inputString) {
            var parser = new Int32Parser();
            Func<int> func = () => {
                var input = inputString.AsSpan();
                return parser.Parse(ref input, new HashSet<Attribute>());
            };

            func.Should().Throw<CommandParseException>();
        }

        [Theory]
        [InlineData("2147483648")]
        [InlineData("-2147483649")]
        public void Parse_IntegerOutOfRange_ThrowsParseException(string inputString) {
            var parser = new Int32Parser();
            Func<int> func = () => {
                var input = inputString.AsSpan();
                return parser.Parse(ref input, new HashSet<Attribute>());
            };

            func.Should().Throw<CommandParseException>().WithInnerException<OverflowException>();
        }

        [Theory]
        [InlineData("aaa")]
        [InlineData("123a")]
        [InlineData("123.0")]
        public void Parse_InvalidInteger_ThrowsParseException(string inputString) {
            var parser = new Int32Parser();
            Func<int> func = () => {
                var input = inputString.AsSpan();
                return parser.Parse(ref input, new HashSet<Attribute>());
            };
            func.Should().Throw<CommandParseException>().WithInnerException<FormatException>();
        }
    }
}
