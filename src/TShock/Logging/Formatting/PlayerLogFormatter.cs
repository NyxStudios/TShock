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
using System.Diagnostics;
using System.IO;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Parsing;
using TShock.Logging.Themes;

namespace TShock.Logging.Formatting {
    internal sealed class PlayerLogFormatter : ITextFormatter {
        // Cache the MessageTemplateParser because why not?
        private static readonly MessageTemplateParser _templateParser = new MessageTemplateParser();

        private readonly IList<ITextFormatter> _formatters = new List<ITextFormatter>();

        public PlayerLogFormatter(PlayerLogTheme theme, string outputTemplate, IFormatProvider? formatProvider) {
            Debug.Assert(theme != null, "theme should not be null");
            Debug.Assert(outputTemplate != null, "output template should not be null");

            // The general strategy is to parse the template, and create a list of formatters in the same order as the
            // properties found within the template. We can then run the formatters in order on each log event.
            var template = _templateParser.Parse(outputTemplate);
            foreach (var token in template.Tokens) {
                if (token is TextToken textToken) {
                    _formatters.Add(new TextFormatter(theme, textToken.Text));
                    continue;
                }

                var propertyToken = (PropertyToken)token;
                _formatters.Add(propertyToken.PropertyName switch {
                    OutputProperties.MessagePropertyName => new MessageTemplateFormatter(theme, formatProvider),
                    OutputProperties.TimestampPropertyName =>
                        new TimestampFormatter(theme, propertyToken.Format, formatProvider),
                    OutputProperties.LevelPropertyName => new LevelFormatter(theme),
                    OutputProperties.NewLinePropertyName => new NewLineFormatter(),
                    OutputProperties.ExceptionPropertyName => new ExceptionFormatter(theme),
                    OutputProperties.PropertiesPropertyName => new PropertiesFormatter(theme, template, formatProvider),
                    _ => new PropertyFormatter(theme, propertyToken, formatProvider)
                });
            }
        }

        public void Format(LogEvent logEvent, TextWriter output) {
            Debug.Assert(logEvent != null, "log event should not be null");
            Debug.Assert(output != null, "output should not be null");

            foreach (var formatter in _formatters) {
                formatter.Format(logEvent, output);
            }
        }
    }
}
