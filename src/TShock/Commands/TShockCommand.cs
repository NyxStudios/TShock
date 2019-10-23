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
using System.Globalization;
using System.Linq;
using System.Reflection;
using Orion.Events;
using TShock.Commands.Exceptions;
using TShock.Commands.Parsers.Attributes;
using TShock.Events.Commands;
using TShock.Properties;
using TShock.Utils.Extensions;

namespace TShock.Commands {
    internal class TShockCommand : ICommand {
        private readonly TShockCommandService _commandService;
        private readonly object _handlerObject;
        private readonly MethodInfo _handler;
        private readonly CommandHandlerAttribute _attribute;

        private readonly ISet<char> _validShortFlags = new HashSet<char>();
        private readonly ISet<string> _validLongFlags = new HashSet<string>();
        private readonly IDictionary<string, ParameterInfo> _validOptionals = new Dictionary<string, ParameterInfo>();

        public string QualifiedName => _attribute.QualifiedName;
        public string HelpText => _attribute.HelpText ?? Resources.Command_MissingHelpText;
        public string UsageText => _attribute.UsageText ?? Resources.Command_MissingUsageText;
        public bool ShouldBeLogged => _attribute.ShouldBeLogged;

        // We need to inject TShockCommandService so that we can raise a CommandExecuteEvent.
        public TShockCommand(
                TShockCommandService commandService, object handlerObject, MethodInfo handler,
                CommandHandlerAttribute attribute) {
            Debug.Assert(commandService != null, "command service should not be null");
            Debug.Assert(handlerObject != null, "handler object should not be null");
            Debug.Assert(handler != null, "handler should not be null");
            Debug.Assert(attribute != null, "attribute should not be null");

            _commandService = commandService;
            _handlerObject = handlerObject;
            _handler = handler;
            _attribute = attribute;

            // Preprocessing parameters in the constructor allows us to learn the command's flags and optionals.
            void PreprocessParameter(ParameterInfo parameterInfo) {
                var parameterType = parameterInfo.ParameterType;

                // If the parameter is a bool and it is marked with FlagAttribute, we'll note it.
                if (parameterType == typeof(bool)) {
                    var attribute = parameterInfo.GetCustomAttribute<FlagAttribute?>();
                    foreach (var flag in attribute?.Flags ?? Enumerable.Empty<string>()) {
                        if (flag.Length == 1) {
                            _validShortFlags.Add(flag[0]);
                        } else {
                            _validLongFlags.Add(flag);
                        }
                    }
                }

                // If the parameter is optional, we'll note it. We replace underscores with hyphens here since hyphens
                // aren't valid in C# identifiers.
                if (parameterInfo.IsOptional) {
                    _validOptionals.Add(parameterInfo.Name.Replace('_', '-'), parameterInfo);
                }
            }

            foreach (var parameterInfo in _handler.GetParameters()) {
                PreprocessParameter(parameterInfo);
            }
        }

        public void Invoke(ICommandSender sender, string input) {
            if (sender is null) {
                throw new ArgumentNullException(nameof(sender));
            }

            var e = new CommandExecuteEvent(this, sender, input);
            _commandService.Kernel.RaiseEvent(e, _commandService.Log);
            if (e.IsCanceled()) {
                return;
            }

            var shortFlags = new HashSet<char>();
            var longFlags = new HashSet<string>();
            var optionals = new Dictionary<string, object?>();

            object? ParseArgument(ref ReadOnlySpan<char> input, ParameterInfo parameterInfo, Type? typeHint = null) {
                var parameterType = typeHint ?? parameterInfo.ParameterType;
                if (!_commandService.Parsers.TryGetValue(parameterType, out var parser)) {
                    throw new CommandParseException(
                        string.Format(CultureInfo.InvariantCulture, Resources.CommandParse_UnrecognizedArgType,
                            parameterType));
                }

                var attributes = parameterInfo.GetCustomAttributes().ToHashSet();
                if (input.IsEmpty) {
                    throw new CommandParseException(
                        string.Format(CultureInfo.InvariantCulture, Resources.CommandParse_MissingArg,
                            parameterInfo));
                }

                return parser.Parse(ref input, attributes);
            }

            void ParseShortFlags(ref ReadOnlySpan<char> input, int space) {
                if (space <= 1) {
                    throw new CommandParseException(Resources.CommandParse_InvalidHyphenatedArg);
                }

                foreach (var c in input[1..space]) {
                    if (!_validShortFlags.Contains(c)) {
                        throw new CommandParseException(
                            string.Format(CultureInfo.InvariantCulture, Resources.CommandParse_UnrecognizedShortFlag,
                                c));
                    }

                    shortFlags.Add(c);
                }

                input = input[space..];
            }

            void ParseLongFlag(ref ReadOnlySpan<char> input, int space) {
                if (space <= 2) {
                    throw new CommandParseException(Resources.CommandParse_InvalidHyphenatedArg);
                }

                var longFlag = input[2..space].ToString();
                if (!_validLongFlags.Contains(longFlag)) {
                    throw new CommandParseException(
                        string.Format(CultureInfo.InvariantCulture, Resources.CommandParse_UnrecognizedLongFlag,
                            longFlag));
                }

                longFlags.Add(longFlag);
                input = input[space..];
            }

            void ParseOptional(ref ReadOnlySpan<char> input, int equals) {
                if (equals <= 2) {
                    throw new CommandParseException(Resources.CommandParse_InvalidHyphenatedArg);
                }

                var optional = input[2..equals].ToString();
                if (!_validOptionals.TryGetValue(optional, out var parameterInfo)) {
                    throw new CommandParseException(
                        string.Format(CultureInfo.InvariantCulture, Resources.CommandParse_UnrecognizedOptional,
                            optional));
                }

                input = input[(equals + 1)..];
                input = input.TrimStart();
                optionals[optional] = ParseArgument(ref input, parameterInfo);
            }

            // Parse all hyphenated arguments:
            // 1) Short flags are single-character flags and use one hyphen: "-f".
            // 2) Long flags are string flags and use two hyphens: "--force".
            // 3) Optionals specify values with two hyphens: "--depth=10".
            void ParseHyphenatedArguments(ref ReadOnlySpan<char> input) {
                input = input.TrimStart();
                while (input.StartsWith("-")) {
                    var space = input.IndexOfOrEnd(' ');
                    if (input.StartsWith("--")) {
                        var equals = input.IndexOfOrEnd('=');
                        if (equals < space) {
                            ParseOptional(ref input, equals);
                        } else {
                            ParseLongFlag(ref input, space);
                        }
                    } else {
                        ParseShortFlags(ref input, space);
                    }

                    input = input.TrimStart();
                }
            }

            // Parse a parameter:
            // 1) If the parameter is an ICommandSender, then inject sender.
            // 2) If the parameter is a bool and is marked with FlagAttribute, then inject the flag.
            // 3) If the parameter is optional, then inject the optional or else the default value.
            // 4) Otherwise, we parse the argument directly.
            object? ParseParameter(ParameterInfo parameterInfo, ref ReadOnlySpan<char> input) {
                var parameterType = parameterInfo.ParameterType;
                if (parameterType == typeof(ICommandSender)) {
                    return sender;
                }

                if (parameterType == typeof(bool)) {
                    var attribute = parameterInfo.GetCustomAttribute<FlagAttribute?>();
                    if (attribute != null) {
                        return attribute.Flags.Any(
                            f => f.Length == 1 && shortFlags.Contains(f[0]) || longFlags.Contains(f));
                    }
                }

                if (parameterInfo.GetCustomAttribute<ParamArrayAttribute>() != null) {
                    var elementType = parameterType.GetElementType();

                    var list = new List<object?>();
                    input = input.TrimStart();
                    while (!input.IsEmpty) {
                        list.Add(ParseArgument(ref input, parameterInfo, elementType));
                        input = input.TrimStart();
                    }

                    var array = Array.CreateInstance(elementType, list.Count);
                    for (var i = 0; i < list.Count; ++i) {
                        array.SetValue(list[i], i);
                    }

                    return array;
                }

                input = input.TrimStart();
                if (parameterInfo.IsOptional) {
                    var optional = parameterInfo.Name.Replace('_', '-');
                    if (optionals.TryGetValue(optional, out var value)) {
                        return value;
                    }

                    if (input.IsEmpty) {
                        return parameterInfo.DefaultValue;
                    }
                }

                return ParseArgument(ref input, parameterInfo);
            }

            var inputSpan = e.Input.AsSpan();
            if (_validShortFlags.Count + _validLongFlags.Count + _validOptionals.Count > 0) {
                ParseHyphenatedArguments(ref inputSpan);
            }

            var parameterInfos = _handler.GetParameters();
            var parameters = new object?[parameterInfos.Length];
            for (var i = 0; i < parameters.Length; ++i) {
                parameters[i] = ParseParameter(parameterInfos[i], ref inputSpan);
            }

            // Ensure that we've consumed all of the useful parts of the input.
            if (!inputSpan.IsWhiteSpace()) {
                throw new CommandParseException(Resources.CommandParse_TooManyArgs);
            }

            try {
                _handler.Invoke(_handlerObject, parameters);
            } catch (TargetInvocationException ex) {
                throw new CommandExecuteException(Resources.CommandExecute_Exception, ex.InnerException);
            }
        }
    }
}
